using System;
using System.Diagnostics;
using SandScript.AbstractSyntaxTrees;
using SandScript.Exceptions;

namespace SandScript;

public sealed class Interpreter : NodeVisitor<object?>, IStage
{
	StageDiagnostics IStage.Diagnostics => Diagnostics;
	Type IStage.PrerequisiteStage => typeof(Optimizer);
	Type? IStage.SortBeforeStage => null;
	
	internal Script Owner { get; private set; }

	internal readonly VariableManager<string, object?> Variables = new(null);
	internal readonly VariableManager<MethodSignature, object?> MethodVariables =
		new(new IgnoreHashCodeComparer<MethodSignature>());
	
	internal readonly InterpreterDiagnostics Diagnostics = new();
	internal bool Returning;

	private Interpreter()
	{
		Owner = null!;
		
		foreach ( var method in SandScript.CustomMethods )
		{
			var methodSignature = MethodSignature.From( method );
			Variables.Root.Add( methodSignature.ToString(), method );
			MethodVariables.Root.Add( methodSignature, method );
		}

		foreach ( var variable in SandScript.CustomVariables )
			Variables.Root.Add( variable.Name, variable );
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
			Diagnostics.Time( sw.Elapsed.TotalMilliseconds );
			
			return StageResult.Success( result );
		}
		catch ( Exception e )
		{
			return StageResult.Fail( new RuntimeException( e ) );
		}
	}

	private object? Interpret( Ast ast ) => Visit( ast );

	protected override object? VisitProgram( ProgramAst programAst )
	{
		foreach ( var statement in programAst.Statements )
		{
			var result = Visit( statement );
			if ( !Returning )
				continue;

			Returning = false;
			Variables.Leave();
			return result;
		}
		
		return null;
	}

	protected override object? VisitBlock( BlockAst blockAst )
	{
		Variables.Enter( blockAst.Guid );
		foreach ( var statement in blockAst.Statements )
		{
			var result = Visit( statement );
			if ( !Returning )
				continue;

			Variables.Leave();
			return result;
		}
		Variables.Leave();

		return null;
	}
	
	protected override object? VisitReturn( ReturnAst returnAst )
	{
		Returning = true;
		return Visit( returnAst.ExpressionAst );
	}
	
	protected override object? VisitAssignment( AssignmentAst assignmentAst )
	{
		var variableName = assignmentAst.VariableAst.VariableName;
		Variables.Current.TryGetValue( variableName, out var value, out var container );

		object? newValue;
		if ( assignmentAst.Operator.Type == TokenType.Equals )
			newValue = Visit( assignmentAst.ExpressionAst );
		else
		{
			var binaryOperator = assignmentAst.Operator.Type.GetBinaryOperatorOfAssignment();
			var operation = value.GetTypeProvider()!.BinaryOperations[binaryOperator];
			newValue = operation( value, Visit( assignmentAst.ExpressionAst ) );
		}

		if ( value is ScriptVariable variable )
			variable.SetValue( newValue );
		else
			container[variableName] = newValue;

		return null;
	}

	protected override object? VisitBinaryOperator( BinaryOperatorAst binaryOperatorAst )
	{
		var left = Visit( binaryOperatorAst.LeftAst );
		var operation = left.GetTypeProvider()!.BinaryOperations[binaryOperatorAst.Operator.Type];

		return operation( left, Visit( binaryOperatorAst.RightAst ) );
	}

	protected override object? VisitUnaryOperator( UnaryOperatorAst unaryOperatorAst )
	{
		var operand = Visit( unaryOperatorAst.OperandAst );
		var operation = operand.GetTypeProvider()!.UnaryOperations[unaryOperatorAst.Operator.Type];

		return operation( operand );
	}

	protected override object? VisitIf( IfAst ifAst ) =>
		Visit( (bool)Visit( ifAst.BooleanExpressionAst )! ? ifAst.TrueBodyAst : ifAst.FalseBodyAst );

	protected override object? VisitFor( ForAst forAst )
	{
		Variables.Enter( forAst.Guid );
		Visit( forAst.VariableDeclarationAst );
		while ( (bool)Visit( forAst.BooleanExpressionAst )! )
		{
			var result = Visit( forAst.BodyAst );
			if ( !Returning )
			{
				Visit( forAst.IteratorAst );
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
		while ( (bool)Visit( whileAst.BooleanExpressionAst )! )
		{
			var result = Visit( whileAst.BodyAst );
			if ( Returning )
				return result;
		}

		return null;
	}

	protected override object? VisitDoWhile( DoWhileAst doWhileAst )
	{
		do
		{
			var result = Visit( doWhileAst.BodyAst );
			if ( Returning )
				return result;
		} while ( (bool)Visit( doWhileAst.BooleanExpressionAst )! );

		return null;
	}

	protected override object? VisitMethodDeclaration( MethodDeclarationAst methodDeclarationAst )
	{
		var method = new ScriptMethod( methodDeclarationAst );
		var methodSignature = MethodSignature.From( method );
		
		Variables.Current.AddOrUpdate( methodSignature.ToString(), method );
		MethodVariables.Current.AddOrUpdate( methodSignature, method );

		return null;
	}
	
	protected override object? VisitMethodCall( MethodCallAst methodCallAst )
	{
		MethodVariables.Current.TryGetValue( MethodSignature.From( methodCallAst ), out var variable );

		var arguments = new object?[methodCallAst.ArgumentAsts.Length];
		for ( var i = 0; i < methodCallAst.ArgumentAsts.Length; i++ )
			arguments[i] = Visit( methodCallAst.ArgumentAsts[i] );
		
		return ((ScriptMethod)variable!).Invoke( this, arguments );
	}

	protected override object VisitParameter( ParameterAst parameterAst )
	{
		throw new NotImplementedException();
	}

	protected override object? VisitVariableDeclaration( VariableDeclarationAst variableDeclarationAst )
	{
		var defaultValue = Visit( variableDeclarationAst.DefaultExpressionAst ) ??
		                   variableDeclarationAst.VariableTypeAst.TypeProvider.CreateDefault();

		foreach ( var variable in variableDeclarationAst.VariableNameAsts )
			Variables.Current.AddOrUpdate( variable.VariableName, defaultValue );

		return null;
	}

	protected override object? VisitVariable( VariableAst variableAst )
	{
		Variables.Current.TryGetValue( variableAst.VariableName, out var variable );
		return variable is ScriptVariable sv ? sv.GetValue() : variable;
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
