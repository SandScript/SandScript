﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SandScript.AbstractSyntaxTrees;

namespace SandScript;

public sealed class SemanticAnalyzer : NodeVisitor<ITypeProvider>, IStage
{
	StageDiagnostics IStage.Diagnostics => _diagnostics;
	Type IStage.PrerequisiteStage => typeof(Parser);
	Type? IStage.SortBeforeStage => null;

	internal readonly VariableManager<string, ITypeProvider> VariableTypes = new(null);
	internal readonly VariableManager<MethodSignature, ScriptMethod> VariableMethods =
		new(new IgnoreHashCodeComparer<MethodSignature>());
	private readonly VariableManager<string, ScriptVariable> _variableExternals = new(null);

	private readonly SemanticAnalyzerDiagnostics _diagnostics = new();
	private readonly TypeCheckStack _neededTypes = new();

	private SemanticAnalyzer()
	{
		foreach ( var method in SandScript.CustomMethods )
		{
			var methodSignature = MethodSignature.From( method );
			VariableTypes.Root.AddOrUpdate( methodSignature.ToString(), TypeProviders.Builtin.Method );
			VariableMethods.Root.AddOrUpdate( methodSignature, method );
		}

		foreach ( var variable in SandScript.CustomVariables )
		{
			VariableTypes.Root.AddOrUpdate( variable.Name, variable.TypeProvider );
			_variableExternals.Root.AddOrUpdate( variable.Name, variable );
		}
	}
	
	StageResult IStage.Run( Script owner, object?[] arguments )
	{
		if ( arguments.Length < 1 || arguments[0] is not Ast ast )
			throw new ArgumentException( null, nameof(arguments) );

		var sw = Stopwatch.StartNew();

		var result = AnalyzeTree( ast );
		
		sw.Stop();
		_diagnostics.Time( sw.Elapsed.TotalMilliseconds );
		
		return result ? StageResult.Success( ast ) : StageResult.Fail( ast );
	}

	private bool AnalyzeTree( Ast ast )
	{
		Visit( ast );
		
		return _diagnostics.Errors.Count == 0;
	}

	private void EnterScope( Guid guid, IEnumerable<KeyValuePair<string, ITypeProvider>>? startVariables = null )
	{
		VariableTypes.Enter( guid, startVariables );
		VariableMethods.Enter( guid );
		_variableExternals.Enter( guid );
	}

	private void LeaveScope()
	{
		VariableTypes.Leave();
		VariableMethods.Leave();
		_variableExternals.Leave();
	}

	private ITypeProvider VisitExpectingType( ITypeProvider type, Ast ast )
	{
		_neededTypes.Push( type );
		var result = Visit( ast );
		_neededTypes.Pop();

		return result;
	}

	private bool VerifyTypeLoose( ITypeProvider type, [NotNullWhen(false)] out ITypeProvider? expectedType ) =>
		_neededTypes.AssertTypeCheckLoose( type, out expectedType );

	protected override ITypeProvider VisitProgram( ProgramAst programAst )
	{
		foreach ( var statement in programAst.Statements )
		{
			var value = Visit( statement );
			if ( value == TypeProviders.Builtin.Nothing )
				continue;

			LeaveScope();
			return value;
		}

		return TypeProviders.Builtin.Nothing;
	}

	protected override ITypeProvider VisitBlock( BlockAst blockAst )
	{
		EnterScope( blockAst.Guid );
		foreach ( var statement in blockAst.Statements )
		{
			var value = Visit( statement );
			if ( value == TypeProviders.Builtin.Nothing )
				continue;

			LeaveScope();
			return value;
		}
		LeaveScope();

		return TypeProviders.Builtin.Nothing;
	}
	
	protected override ITypeProvider VisitReturn( ReturnAst returnAst )
	{
		var result = TypeProviders.Builtin.Nothing;
		if ( returnAst.Expression is not NoOperationAst )
			result = Visit( returnAst.Expression );

		if ( !VerifyTypeLoose( result, out var expectedType ) )
			_diagnostics.TypeMismatch( expectedType, result, returnAst.StartLocation );
		
		return result;
	}
	
	protected override ITypeProvider VisitAssignment( AssignmentAst assignmentAst )
	{
		if ( !VariableTypes.Current.TryGetValue( assignmentAst.Variable.VariableName, out var type ) )
		{
			_diagnostics.Undefined( assignmentAst.Variable.VariableName );
			return TypeProviders.Builtin.Nothing;
		}

		if ( _variableExternals.Current.TryGetValue( assignmentAst.Variable.VariableName, out var variable ) && !variable.CanWrite )
		{
			_diagnostics.Unwritable( assignmentAst.Variable.VariableName );
			return TypeProviders.Builtin.Nothing;
		}

		_neededTypes.Push( type );
		if ( assignmentAst.Operator.Type.GetBinaryOperatorOfAssignment() != TokenType.None )
			Visit( assignmentAst.Expression );
		_neededTypes.Pop();

		return TypeProviders.Builtin.Nothing;
	}
	
	protected override ITypeProvider VisitBinaryOperator( BinaryOperatorAst binaryOperatorAst )
	{
		var leftType = VisitExpectingType( TypeProviders.Builtin.Variable, binaryOperatorAst.Left );
		VisitExpectingType( leftType, binaryOperatorAst.Right );
		var operandResultType = binaryOperatorAst.Operator.Type.GetOperatorResultType();
		operandResultType = operandResultType == TypeProviders.Builtin.Variable
			? leftType
			: operandResultType;
		
		if ( !VerifyTypeLoose( operandResultType, out var expectedType ) )
			_diagnostics.TypeMismatch( expectedType, leftType, binaryOperatorAst.StartLocation );

		if ( leftType.BinaryOperations.ContainsKey( binaryOperatorAst.Operator.Type ) )
			return leftType;

		_diagnostics.UnsupportedBinaryOperatorForType( binaryOperatorAst.Operator.Type, leftType, binaryOperatorAst.StartLocation );
		return leftType;
	}

	protected override ITypeProvider VisitUnaryOperator( UnaryOperatorAst unaryOperatorAst )
	{
		var operandType = VisitExpectingType( TypeProviders.Builtin.Variable, unaryOperatorAst.Operand );
		var operandResultType = unaryOperatorAst.Operator.Type.GetOperatorResultType();
		operandResultType = operandResultType == TypeProviders.Builtin.Variable
			? operandType
			: operandResultType;
		
		if ( !VerifyTypeLoose( operandResultType, out var expectedType ) )
			_diagnostics.TypeMismatch( expectedType, operandType, unaryOperatorAst.StartLocation );

		if ( operandType.UnaryOperations.ContainsKey( unaryOperatorAst.Operator.Type ) )
			return operandType;
		
		_diagnostics.UnsupportedUnaryOperatorForType( unaryOperatorAst.Operator.Type, operandType, unaryOperatorAst.StartLocation );
		return operandType;
	}
	
	protected override ITypeProvider VisitIf( IfAst ifAst )
	{
		VisitExpectingType( TypeProviders.Builtin.Boolean, ifAst.BooleanExpression );
		
		var result = Visit( ifAst.TrueBranch );
		if ( !VerifyTypeLoose( result, out var expectedType ) )
			_diagnostics.TypeMismatch( expectedType, result, ifAst.TrueBranch.StartLocation );

		if ( ifAst.FalseBranch is NoOperationAst )
			return result;
		
		result = Visit( ifAst.FalseBranch );
		if ( !VerifyTypeLoose( result, out expectedType ) )
			_diagnostics.TypeMismatch( expectedType, result, ifAst.FalseBranch.StartLocation );

		return result;
	}

	protected override ITypeProvider VisitFor( ForAst forAst )
	{
		EnterScope( forAst.Guid );
		VisitExpectingType( TypeProviders.Builtin.Number, forAst.VariableDeclaration );
		VisitExpectingType( TypeProviders.Builtin.Boolean, forAst.BooleanExpression );
		VisitExpectingType( TypeProviders.Builtin.Nothing, forAst.Iterator );
		var result = Visit( forAst.Block );
		LeaveScope();

		return result;
	}
	
	protected override ITypeProvider VisitWhile( WhileAst whileAst )
	{
		VisitExpectingType( TypeProviders.Builtin.Boolean, whileAst.BooleanExpression );
		return Visit( whileAst.Block );
	}

	protected override ITypeProvider VisitDoWhile( DoWhileAst doWhileAst )
	{
		VisitExpectingType( TypeProviders.Builtin.Boolean, doWhileAst.BooleanExpression );
		return Visit( doWhileAst.Block );
	}

	protected override ITypeProvider VisitMethodDeclaration( MethodDeclarationAst methodDeclarationAst )
	{
		var method = new ScriptMethod( methodDeclarationAst );
		var methodSignature = MethodSignature.From( method );
		if ( VariableMethods.Current.TryGetValue( methodSignature, out _, out var container ) )
		{
			_diagnostics.Redefined( methodSignature.ToString(), container.Guid );
			return TypeProviders.Builtin.Nothing;
		}
		
		VariableTypes.Current.AddOrUpdate( methodSignature.ToString(), TypeProviders.Builtin.Method );
		VariableMethods.Current.AddOrUpdate( methodSignature, new ScriptMethod( methodDeclarationAst ) );

		EnterScope( methodDeclarationAst.Guid );
		foreach ( var parameter in methodDeclarationAst.Parameters )
			Visit( parameter );
		VisitExpectingType( methodDeclarationAst.ReturnType.TypeProvider, methodDeclarationAst.Scope );
		LeaveScope();
			
		return TypeProviders.Builtin.Nothing;
	}

	protected override ITypeProvider VisitMethodCall( MethodCallAst methodCallAst )
	{
		foreach ( var argument in methodCallAst.Arguments )
		{
			var argumentType = VisitExpectingType( TypeProviders.Builtin.Variable, argument );
			methodCallAst.ArgumentTypes = methodCallAst.ArgumentTypes.Add( argumentType );
		}

		var callSignature = MethodSignature.From( methodCallAst );
		if ( !VariableMethods.Current.TryGetValue( callSignature, out var method ) )
		{
			_diagnostics.Undefined( callSignature.ToString() );
			return TypeProviders.Builtin.Nothing;
		}
		
		if ( !VerifyTypeLoose( method.ReturnTypeProvider, out var expectedType ) )
			_diagnostics.TypeMismatch( expectedType, method.ReturnTypeProvider, methodCallAst.StartLocation );

		var numArguments = methodCallAst.Arguments.Length;
		if ( numArguments != method.Parameters.Count )
			_diagnostics.ArgumentCountMismatch( method.Parameters.Count, numArguments, methodCallAst.StartLocation );

		for ( var i = 0; i < numArguments; i++ )
		{
			var parameter = method.Parameters[i];
			if ( i >= method.Parameters.Count )
			{
				_diagnostics.MissingParameter( parameter.Item1, methodCallAst.MethodName, methodCallAst.StartLocation );
				continue;
			}
			
			VisitExpectingType( parameter.Item2, methodCallAst.Arguments[i] );
		}
		
		return method.ReturnTypeProvider;
	}

	protected override ITypeProvider VisitParameter( ParameterAst parameterAst )
	{
		VariableTypes.Current.AddOrUpdate( parameterAst.ParameterName.VariableName, parameterAst.ParameterType.TypeProvider );
		return parameterAst.ParameterType.TypeProvider;
	}

	protected override ITypeProvider VisitVariableDeclaration( VariableDeclarationAst variableDeclarationAst )
	{
		var value = VisitExpectingType( variableDeclarationAst.VariableType.TypeProvider,
			variableDeclarationAst.DefaultExpression );
		value = value != TypeProviders.Builtin.Nothing
			? value
			: variableDeclarationAst.VariableType.TypeProvider;
		
		if ( value == TypeProviders.Builtin.Nothing || value == TypeProviders.Builtin.Variable )
			_diagnostics.MissingType( variableDeclarationAst.StartLocation );
		
		foreach ( var variable in variableDeclarationAst.VariableNames )
		{
			var variableName = variable.VariableName;
			
			if ( VariableTypes.Current.TryGetValue( variableName, out _, out var container ) )
			{
				_diagnostics.Redefined( variableName, container.Guid );
				continue;
			}

			VariableTypes.Current.AddOrUpdate( variableName, value );
		}

		return TypeProviders.Builtin.Nothing;
	}

	protected override ITypeProvider VisitVariable( VariableAst variableAst )
	{
		if ( !VariableTypes.Current.TryGetValue( variableAst.VariableName, out var variableType ) )
		{
			_diagnostics.Undefined( variableAst.VariableName );
			return TypeProviders.Builtin.Nothing;
		}

		if ( _variableExternals.Current.TryGetValue( variableAst.VariableName, out var variable ) && !variable.CanRead )
		{
			_diagnostics.Unreadable( variableAst.VariableName );
			return variable.TypeProvider;
		}
		
		if ( !VerifyTypeLoose( variableType, out var expectedType ) )
			_diagnostics.TypeMismatch( expectedType, variableType, variableAst.StartLocation );
		
		return variableType;
	}

	protected override ITypeProvider VisitVariableType( VariableTypeAst variableTypeAst ) =>
		variableTypeAst.TypeProvider;
	
	protected override ITypeProvider VisitLiteral( LiteralAst literalAst )
	{
		if ( !VerifyTypeLoose( literalAst.TypeProvider, out var expectedType ) )
			_diagnostics.TypeMismatch( expectedType, literalAst.TypeProvider, literalAst.StartLocation );
		
		return literalAst.TypeProvider;
	}

	protected override ITypeProvider VisitNoOperation( NoOperationAst noOperationAst ) =>
		TypeProviders.Builtin.Nothing;

	protected override ITypeProvider VisitComment( CommentAst commentAst ) =>
		TypeProviders.Builtin.Nothing;

	protected override ITypeProvider VisitWhitespace( WhitespaceAst whitespaceAst ) =>
		TypeProviders.Builtin.Nothing;
	
	public static bool Analyze( Ast ast ) => new SemanticAnalyzer().AnalyzeTree( ast );
}
