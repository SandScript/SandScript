using System.Collections.Immutable;
using System.Diagnostics;
using SandScript.AbstractSyntaxTrees;

namespace SandScript;

public sealed class Optimizer : NodeVisitor<Ast>, IStage
{
	StageDiagnostics IStage.Diagnostics => _diagnostics;
	Type IStage.PrerequisiteStage => typeof(SemanticAnalyzer);
	Type? IStage.SortBeforeStage => null;

	private readonly VariableManager<string, bool> _removedVariables = new();
	private readonly VariableManager<MethodSignature, bool> _removedMethods = new();
	
	private readonly OptimizerDiagnostics _diagnostics = new();

	private int _numChanges;

	private Optimizer()
	{
	}
	
	StageResult IStage.Run( Script owner, object?[] arguments )
	{
		if ( arguments.Length < 1 || arguments[0] is not Ast ast )
			throw new ArgumentException( null, nameof(arguments) );

		var sw = Stopwatch.StartNew();
		
		_numChanges = 0;
		var optimizedAst = OptimizeTree( ast );
		
		sw.Stop();
		_diagnostics.Time( sw.Elapsed.TotalMilliseconds );
		
		if ( _diagnostics.Errors.Count != 0 )
			return StageResult.Fail( optimizedAst );

		return _numChanges > 0 ? StageResult.NeedsRepeating( optimizedAst ) : StageResult.Success( optimizedAst );
	}

	private Ast OptimizeTree( Ast ast ) => Visit( ast );

	private Ast AddChange( Ast ast )
	{
		//Console.WriteLine(Environment.StackTrace);
		//Console.WriteLine("----------");
		_numChanges++;
		return ast;
	}

	protected override Ast VisitProgram( ProgramAst programAst )
	{
		var newCompound = Visit( programAst.Compound );
		return newCompound is NoOperationAst
			? AddChange( new NoOperationAst( programAst.StartLocation ) )
			: new ProgramAst( (CompoundStatementAst)newCompound );
	}
	
	protected override Ast VisitCompoundStatement( CompoundStatementAst compoundStatementAst )
	{
		var newStatements = ImmutableArray.CreateBuilder<Ast>();
		foreach ( var statement in compoundStatementAst.Statements )
		{
			var result = Visit( statement );
			if ( result is not NoOperationAst )
				newStatements.Add( result );
		}

		return newStatements.Count == 0
			? AddChange( new NoOperationAst( compoundStatementAst.StartLocation ) )
			: new CompoundStatementAst( compoundStatementAst.StartLocation, newStatements.ToImmutable() );
	}

	protected override Ast VisitBlock( BlockAst blockAst )
	{
		var newStatements = ImmutableArray.CreateBuilder<Ast>();
		foreach ( var statement in blockAst.Statements )
		{
			var result = Visit( statement );
			if ( result is not NoOperationAst )
				newStatements.Add( result );
		}

		return newStatements.Count == 0
			? AddChange( new NoOperationAst( blockAst.StartLocation ) )
			: new BlockAst( blockAst.StartLocation, newStatements.ToImmutable() );
	}

	protected override Ast VisitReturn( ReturnAst returnAst ) =>
		new ReturnAst( returnAst.StartLocation, Visit( returnAst.Expression ) );

	protected override Ast VisitAssignment( AssignmentAst assignmentAst ) =>
		new AssignmentAst( assignmentAst.Variable, assignmentAst.Operator, Visit( assignmentAst.Expression ) );

	protected override Ast VisitBinaryOperator( BinaryOperatorAst binaryOperatorAst )
	{
		var newLeft = Visit( binaryOperatorAst.Left );
		var newRight = Visit( binaryOperatorAst.Right );
		
		if ( newLeft is not LiteralAst leftLiteral || newRight is not LiteralAst rightLiteral )
			return new BinaryOperatorAst( newLeft, binaryOperatorAst.Operator, newRight );
		
		var binaryOperation = leftLiteral.TypeProvider.BinaryOperations[binaryOperatorAst.Operator.Type];
		var newValue = binaryOperation( leftLiteral.Value, rightLiteral.Value );
		var newToken = new Token( TokenType.Literal, newValue!, leftLiteral.StartLocation );
		return AddChange( new LiteralAst( newToken, leftLiteral.TypeProvider ) );
	}

	protected override Ast VisitUnaryOperator( UnaryOperatorAst unaryOperatorAst )
	{
		var newOperand = Visit( unaryOperatorAst.Operand );

		if ( newOperand is not LiteralAst literalAst )
			return new UnaryOperatorAst( unaryOperatorAst.Operator, newOperand );

		var unaryOperation = literalAst.TypeProvider.UnaryOperations[unaryOperatorAst.Operator.Type];
		var newValue = unaryOperation( literalAst.Value );
		var newToken = new Token( TokenType.Literal, newValue!, literalAst.StartLocation );
		return AddChange( new LiteralAst( newToken, literalAst.TypeProvider ) );
	}

	protected override Ast VisitIf( IfAst ifAst )
	{
		var newBooleanExpression = Visit( ifAst.BooleanExpression );
		var falseBranch = Visit( ifAst.FalseBranch );
		
		if ( newBooleanExpression is LiteralAst literalAst && !(bool)literalAst.Value )
			return AddChange( falseBranch );
		
		var trueBranch = Visit( ifAst.TrueBranch );
		if ( trueBranch is NoOperationAst && falseBranch is NoOperationAst )
			return AddChange( new NoOperationAst( ifAst.StartLocation ) );
		
		return new IfAst( ifAst.StartLocation, newBooleanExpression, (BlockAst)trueBranch, falseBranch );
	}

	protected override Ast VisitFor( ForAst forAst )
	{
		var newBooleanExpression = Visit( forAst.BooleanExpression );
		if ( newBooleanExpression is LiteralAst literalAst && !(bool)literalAst.Value )
			return AddChange( new NoOperationAst( forAst.StartLocation ) );
		
		var newBlock = Visit( forAst.Block );
		if ( newBlock is NoOperationAst )
			return AddChange( new NoOperationAst( forAst.StartLocation ) );

		var newIterator = Visit( forAst.Iterator );
		return new ForAst( forAst.StartLocation, forAst.VariableDeclaration, newBooleanExpression,
			(AssignmentAst)newIterator, (BlockAst)newBlock );
	}

	protected override Ast VisitWhile( WhileAst whileAst )
	{
		var newBooleanExpression = Visit( whileAst.BooleanExpression );
		if ( newBooleanExpression is LiteralAst literalAst && !(bool)literalAst.Value )
			return AddChange( new NoOperationAst( whileAst.StartLocation ) );
		
		var newBlock = Visit( whileAst.Block );
		return newBlock is NoOperationAst
			? AddChange( new NoOperationAst( whileAst.StartLocation ) )
			: new WhileAst( whileAst.StartLocation, newBooleanExpression, (BlockAst)newBlock );
	}

	protected override Ast VisitDoWhile( DoWhileAst doWhileAst )
	{
		var newBooleanExpression = Visit( doWhileAst.BooleanExpression );
		var newBlock = Visit( doWhileAst.Block );

		if ( newBooleanExpression is LiteralAst literalAst && !(bool)literalAst.Value )
			return AddChange( newBlock );
		
		return newBlock is NoOperationAst
			? AddChange( new NoOperationAst( doWhileAst.StartLocation ) )
			: new DoWhileAst( doWhileAst.StartLocation, newBooleanExpression, (BlockAst)newBlock );
	}

	// If method is unused and not global. Remove it.
	protected override Ast VisitMethodDeclaration( MethodDeclarationAst methodDeclarationAst )
	{
		var newCompound = Visit( methodDeclarationAst.Compound );

		if ( newCompound is not NoOperationAst )
		{
			return new MethodDeclarationAst( methodDeclarationAst.ReturnType,
				methodDeclarationAst.MethodNameVariable, methodDeclarationAst.Parameters,
				(CompoundStatementAst)newCompound );
		}

		_removedMethods.Current.Set( MethodSignature.From( methodDeclarationAst ), true );
		return AddChange( new NoOperationAst( methodDeclarationAst.StartLocation ) );
	}

	// If method can resolve to a literal, create one.
	// If method is unused or undefined due to previous optimization. Remove it.
	protected override Ast VisitMethodCall( MethodCallAst methodCallAst )
	{
		if ( _removedMethods.Current.TryGet( MethodSignature.From( methodCallAst ), out _, out _ ) )
			return AddChange( new NoOperationAst( methodCallAst.StartLocation ) );

		var newArguments = ImmutableArray.CreateBuilder<Ast>();
		foreach ( var argument in methodCallAst.Arguments )
			newArguments.Add( Visit( argument ) );

		return new MethodCallAst( methodCallAst.NameToken, newArguments.ToImmutable(), methodCallAst.ArgumentTypes );
	}

	// If variable is only set and never gotten while not being a global. Remove it.
	protected override Ast VisitVariableDeclaration( VariableDeclarationAst variableDeclarationAst ) =>
		new VariableDeclarationAst( variableDeclarationAst.VariableType, variableDeclarationAst.VariableNames,
			Visit( variableDeclarationAst.DefaultExpression ) );

	// If variable is unused or undefined due to previous optimization. Remove it.
	protected override Ast VisitVariable( VariableAst variableAst ) => variableAst;

	protected override Ast VisitVariableType( VariableTypeAst variableTypeAst ) => variableTypeAst;

	protected override Ast VisitLiteral( LiteralAst literalAst ) => literalAst;

	protected override Ast VisitNoOperation( NoOperationAst noOperationAst ) => noOperationAst;

	protected override Ast VisitComment( CommentAst commentAst ) => new NoOperationAst( commentAst.StartLocation );

	protected override Ast VisitWhitespace( WhitespaceAst whitespaceAst ) =>
		new NoOperationAst( whitespaceAst.StartLocation );

	public static Ast Optimize( Ast ast ) => new Optimizer().OptimizeTree( ast );
}
