using System.Diagnostics;
using JetBrains.Annotations;
using SandScript.AbstractSyntaxTrees;
using SandScript.Exceptions;

namespace SandScript;

public sealed class Interpreter : NodeVisitor<object?>, IStage
{
	StageDiagnostics IStage.Diagnostics => _diagnostics;
	Type IStage.PrerequisiteStage => typeof(Optimizer);
	Type? IStage.SortBeforeStage => null;
	
	internal Script Owner { get; private set; }

	internal readonly VariableManager<string, object?> Variables = new();
	internal readonly VariableManager<MethodSignature, object?> MethodVariables = new();

	private readonly InterpreterDiagnostics _diagnostics = new();
	private bool _returning;

	[UsedImplicitly]
	private Interpreter()
	{
		Owner = null!;
		
		foreach ( var method in SandScript.CustomMethods )
		{
			var methodSignature = MethodSignature.From( method );
			Variables.Global.Set( methodSignature.ToString(), method );
			MethodVariables.Global.Set( methodSignature, method );
		}

		foreach ( var variable in SandScript.CustomVariables )
			Variables.Global.Set( variable.Name, variable );
	}
	
	StageResult IStage.Run( Script owner, object?[] arguments )
	{
		if ( arguments.Length < 1 || arguments[0] is not Ast ast )
			throw new ArgumentException( null, nameof(arguments) );

		Owner = owner;

		try
		{
			var sw = Stopwatch.StartNew();

			var result = Interpret( ast );
			
			sw.Stop();
			_diagnostics.Time( sw.Elapsed.TotalMilliseconds );
			
			return StageResult.Success( result );
		}
		catch ( Exception e )
		{
			return StageResult.Fail( new RuntimeException( e ) );
		}
	}

	private object? Interpret( Ast ast ) => Visit( ast );

	protected override object? VisitProgram( ProgramAst programAst ) => Visit( programAst.Compound );

	protected override object? VisitCompoundStatement( CompoundStatementAst compoundStatementAst )
	{
		foreach ( var statement in compoundStatementAst.Statements )
		{
			var result = Visit( statement );
			if ( !_returning )
				continue;

			_returning = false;
			return result;
		}
		
		return null;
	}

	protected override object? VisitBlock( BlockAst blockAst )
	{
		Variables.Enter( "Block", null );
		foreach ( var statement in blockAst.Statements )
		{
			var result = Visit( statement );
			if ( !_returning )
				continue;

			Variables.Leave();
			return result;
		}
		Variables.Leave();

		return null;
	}
	
	protected override object? VisitReturn( ReturnAst returnAst )
	{
		_returning = true;
		return Visit( returnAst.Expression );
	}
	
	protected override object? VisitAssignment( AssignmentAst assignmentAst )
	{
		var variableName = assignmentAst.Variable.VariableName;
		var value = Variables.Current.Get( variableName, out var container );

		object? newValue;
		if ( assignmentAst.Operator.Type == TokenType.Equals )
			newValue = Visit( assignmentAst.Expression );
		else
		{
			var binaryOperator = assignmentAst.Operator.Type.GetBinaryOperatorOfAssignment();
			var operation = value.GetTypeProvider()!.BinaryOperations[binaryOperator];
			newValue = operation( value, Visit( assignmentAst.Expression ) );
		}

		if ( value is ScriptVariable variable )
			variable.SetValue( newValue );
		else
			container!.Set( variableName, newValue );

		return null;
	}

	protected override object? VisitBinaryOperator( BinaryOperatorAst binaryOperatorAst )
	{
		var left = Visit( binaryOperatorAst.Left );
		var operation = left.GetTypeProvider()!.BinaryOperations[binaryOperatorAst.Operator.Type];

		return operation( left, Visit( binaryOperatorAst.Right ) );
	}

	protected override object? VisitUnaryOperator( UnaryOperatorAst unaryOperatorAst )
	{
		var operand = Visit( unaryOperatorAst.Operand );
		var operation = operand.GetTypeProvider()!.UnaryOperations[unaryOperatorAst.Operator.Type];

		return operation( operand );
	}

	protected override object? VisitIf( IfAst ifAst ) =>
		Visit( (bool)Visit( ifAst.BooleanExpression )! ? ifAst.TrueBranch : ifAst.FalseBranch );

	protected override object? VisitFor( ForAst forAst )
	{
		Variables.Enter( "InternalFor", null );
		Visit( forAst.VariableDeclaration );
		while ( (bool)Visit( forAst.BooleanExpression )! )
		{
			if ( !_returning )
			var result = Visit( forAst.Block );
			{
				Visit( forAst.Iterator );
				continue;
			}
			
			Variables.Leave();
			return result;
		}
		Variables.Leave();
		
		return null;
	}

	protected override object? VisitWhile( WhileAst whileAst )
	{
		while ( (bool)Visit( whileAst.BooleanExpression )! )
		{
			if ( _returning )
			var result = Visit( whileAst.Block );
				return result;
		}

		return null;
	}

	protected override object? VisitDoWhile( DoWhileAst doWhileAst )
	{
		do
		{
			if ( _returning )
			var result = Visit( doWhileAst.Block );
				return result;
		} while ( (bool)Visit( doWhileAst.BooleanExpression )! );

		return null;
	}

	protected override object? VisitMethodDeclaration( MethodDeclarationAst methodDeclarationAst )
	{
		var method = new ScriptMethod( methodDeclarationAst );
		var methodSignature = MethodSignature.From( method );
		
		Variables.Current.Set( methodSignature.ToString(), method );
		MethodVariables.Current.Set( methodSignature, method );

		return null;
	}
	
	protected override object? VisitMethodCall( MethodCallAst methodCallAst )
	{
		var variable = MethodVariables.Current.Get( MethodSignature.From( methodCallAst ), out _ );

		var arguments = new object?[methodCallAst.Arguments.Length];
		for ( var i = 0; i < methodCallAst.Arguments.Length; i++ )
			arguments[i] = Visit( methodCallAst.Arguments[i] );
		
		return ((ScriptMethod)variable!).Invoke( this, arguments );
	}

	protected override object? VisitVariableDeclaration( VariableDeclarationAst variableDeclarationAst )
	{
		var defaultValue = Visit( variableDeclarationAst.DefaultExpression ) ??
		                   variableDeclarationAst.VariableType.TypeProvider.CreateDefault();

		foreach ( var variable in variableDeclarationAst.VariableNames )
			Variables.Current.Set( variable.VariableName, defaultValue );

		return null;
	}

	protected override object? VisitVariable( VariableAst variableAst )
	{
		var value = Variables.Current.Get( variableAst.VariableName, out _ );
		return value is ScriptVariable variable ? variable.GetValue() : value;
	}

	protected override object VisitVariableType( VariableTypeAst variableTypeAst )
	{
		throw new NotImplementedException();
	}

	protected override object VisitLiteral( LiteralAst literalAst ) => literalAst.Value;

	protected override object? VisitNoOperation( NoOperationAst noOperationAst ) => null;
	
	protected override object VisitComment( CommentAst commentAst )
	{
		throw new NotImplementedException();
	}

	protected override object VisitWhitespace( WhitespaceAst whitespaceAst )
	{
		throw new NotImplementedException();
	}
}
