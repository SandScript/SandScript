using System;
using System.Collections.Generic;
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

		}
	}


	public void AddGlobal( string varName, ScriptValue value )
	{

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

		return ScriptValue.From( method.Invoke( interpreter, args ) );
	}

	public ScriptValue Call( ScriptValue sv, params object?[] args )
	{
		if ( sv.TypeProvider != TypeProviders.Builtin.Method )
			throw new TypeMismatchException( TypeProviders.Builtin.Method, sv.TypeProvider );

		return Call( (ScriptMethod)sv.Value!, args );
	}

	}

	public ScriptValue? Execute( string text ) => Execute( text, out ScriptDiagnostics _ );
	public ScriptValue? Execute( string text, out ScriptDiagnostics diagnostics )
	{

		var results = Run( text, false, out diagnostics );
		if ( results.Length == 0 )
			return null;

		if ( results[0] is ScriptValue sv )
			return sv;
		
		return ScriptValue.From( results[0] );
	}

	public static Script Create()
	{
		return new Script();
	}

	public static Script Execute( string text, out ScriptValue? returnValue ) =>
		Execute( text, out returnValue, out _ );
	public static Script Execute( string text, out ScriptValue? returnValue, out ScriptDiagnostics diagnostics )
	{
		returnValue = script.Execute( text, out diagnostics );
		return script;
	}
}
