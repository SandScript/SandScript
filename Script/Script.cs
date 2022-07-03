using System.Diagnostics.CodeAnalysis;
using SandScript.Exceptions;

namespace SandScript;

public class Script
{
	public IReadOnlyDictionary<string, ScriptValue> Globals
	{
		get
		{
			if ( !TryGetStage<Interpreter>( out var interpreter ) )
				throw new StageMissingException( typeof(Interpreter) );

			var globals = new Dictionary<string, ScriptValue>();
			foreach ( var rawGlobal in interpreter.Variables.Root )
				globals.Add( rawGlobal.Key, ScriptValue.From( rawGlobal.Value ) );
			return globals;
		}
	}
	
	public string LastText { get; private set; }
	
	private readonly List<IStage> _stages = new();
	private readonly Dictionary<Type, object?[]> _stageResults = new();

	private Script()
	{
	}

	public void AddStage<T>() where T : IStage => AddStage( (T)Activator.CreateInstance( typeof(T), true )! );

	private void AddStage<T>( T stage ) where T : IStage
	{
		if ( stage.PrerequisiteStage is not null && !HasStage( stage.PrerequisiteStage, out _ ) )
			AddStage( (IStage)Activator.CreateInstance( stage.PrerequisiteStage, true )! );

		if ( stage.SortBeforeStage is not null && HasStage( stage.SortBeforeStage, out var index ) )
			_stages.Insert( index.Value, stage );
		else
			_stages.Add( stage );
	}

	public bool HasStage<T>() where T : IStage => HasStage( typeof(T), out _ );

	private bool HasStage<T>( [NotNullWhen( true )] out int? index ) => HasStage( typeof(T), out index );
	
	private bool HasStage( Type stageType, [NotNullWhen(true)] out int? index )
	{
		for ( var i = 0; i < _stages.Count; i++ )
		{
			if ( stageType != _stages[i].GetType() )
				continue;

			index = i;
			return true;
		}

		index = null;
		return false;
	}

	public bool HasStageTypeOf<T>() where T : IStage
	{
		foreach ( var stage in _stages )
		{
			if ( stage is T )
				return true;
		}

		return false;
	}

	private T? GetStage<T>() where T : IStage
	{
		if ( HasStage<T>( out var index ) )
			return (T)_stages[index.Value];

		return default;
	}

	private bool TryGetStage<T>( [NotNullWhen(true)] out T? stage ) where T : IStage
	{
		if ( HasStage<T>() )
		{
			stage = GetStage<T>();
			return true;
		}

		stage = default;
		return false;
	}

	public object?[]? GetStageResults<T>() where T : IStage
	{
		var tType = typeof(T);
		return _stageResults.ContainsKey( tType ) ? _stageResults[tType] : null;
	}

	public void AddGlobal( string varName, ScriptValue value )
	{
		if ( !TryGetStage<SemanticAnalyzer>( out var analyzer ) )
			throw new StageMissingException( typeof(SemanticAnalyzer) );

		if ( !TryGetStage<Interpreter>( out var interpreter ) )
			throw new StageMissingException( typeof(Interpreter) );

		if ( analyzer.VariableTypes.Root.ContainsKey( varName ) )
			throw new GlobalRedefinedException( varName );

		var valueTypeProvider = TypeProviders.GetByType( value.Type );
		if ( valueTypeProvider is null )
			throw new TypeUnsupportedException( value.Type );
		
		analyzer.VariableTypes.Root.Add( varName, valueTypeProvider );;
		interpreter.Variables.Root.Add( varName, value );

		if ( valueTypeProvider != TypeProviders.Builtin.Method )
			return;

		var method = (ScriptMethod)value.Value!;
		var methodSignature = MethodSignature.From( method );
		analyzer.VariableMethods.Root.Add( methodSignature, method );
		interpreter.MethodVariables.Root.Add( methodSignature, value );
	}

	public ScriptValue Call( ScriptMethod method, params object?[] args )
	{
		if ( !TryGetStage<Interpreter>( out var interpreter ) )
			throw new StageMissingException( typeof(Interpreter) );

		return ScriptValue.From( method.Invoke( interpreter, args ) );
	}

	public ScriptValue Call( ScriptValue sv, params object?[] args )
	{
		if ( sv.TypeProvider != TypeProviders.Builtin.Method )
			throw new TypeMismatchException( TypeProviders.Builtin.Method, sv.TypeProvider );

		return Call( (ScriptMethod)sv.Value!, args );
	}

	public object?[] Run( string text, bool lexNonEssentialTokens, out ScriptDiagnostics diagnostics )
	{
		LastText = text;
		var stepArguments = new object?[] {text, lexNonEssentialTokens};
		diagnostics = new ScriptDiagnostics();

		foreach ( var stage in _stages )
		{
			bool shouldRepeat;

			do
			{
				var stageResult = stage.Run( this, stepArguments );
				diagnostics.AddStageAndClear( stage.Diagnostics );
				if ( stageResult.Type == StageResultType.Failed )
					return new object?[] {null};
			
				stepArguments = stageResult.Results;

				var stageType = stage.GetType();
				if ( !_stageResults.TryAdd( stageType, stepArguments ) )
					_stageResults[stage.GetType()] = stepArguments;
				
				shouldRepeat = stageResult.Type == StageResultType.NeedsRepeating;
			} while ( shouldRepeat );
		}
		
		return stepArguments;
	}

	public ScriptValue? Execute( string text ) => Execute( text, out ScriptDiagnostics _ );

	public ScriptValue? Execute( string text, out ScriptDiagnostics diagnostics )
	{
		if ( !HasStage<Interpreter>() )
			throw new StageMissingException( typeof(Interpreter) );

		var results = Run( text, false, out diagnostics );
		if ( results.Length == 0 )
			return null;

		if ( results[0] is ScriptValue sv )
			return sv;
		
		return ScriptValue.From( results[0] );
	}

	public static Script Create() => new();

	public static Script Execute( string text, out ScriptValue? returnValue ) =>
		Execute( text, out returnValue, out _ );

	public static Script Execute( string text, out ScriptValue? returnValue, out ScriptDiagnostics diagnostics )
	{
		var script = Create();
		script.AddStage<Interpreter>();

		returnValue = script.Execute( text, out diagnostics );
		return script;
	}
}
