using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ulearn.Core.CSharp.Validators;

public class ExcessLinesValidator : BaseStyleValidator
{
	public override List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel)
	{
		var bracesPairs = userSolution.BuildBracesPairs().ToArray();

		return bracesPairs
			.Select(ReportWhenExistExcessLineBetweenDeclaration)
			.Concat(bracesPairs.SelectMany(ReportWhenExistExcessLineBetweenBraces))
			.Concat(bracesPairs.Select(ReportWhenNotExistLineBetweenBlocks))
			.Where(x => x != null)
			.OrderBy(e => e.Span.StartLinePosition.Line)
			.ToList();
	}

	private static IEnumerable<SolutionStyleError> ReportWhenExistExcessLineBetweenBraces(BracesPair bracesPair)
	{
		var openBraceLine = GetStartLine(bracesPair.Open);
		var closeBraceLine = GetStartLine(bracesPair.Close);
		var statements = GetStatements(bracesPair.Open.Parent);
		if (statements.Length == 0)
			yield break;

		var firstStatement = statements.First();
		var lastStatement = statements.Last();

		var firstStatementLine = GetStartLine(firstStatement);
		var lastStatementLine = GetEndLine(lastStatement);

		if (openBraceLine != firstStatementLine
			&& openBraceLine + 1 != firstStatementLine
			&& !IsCommentOrRegionOrDisabledText(bracesPair.Open.Parent, openBraceLine + 1))
			yield return new SolutionStyleError(StyleErrorType.ExcessLines01, bracesPair.Open);

		if (closeBraceLine != lastStatementLine
			&& closeBraceLine - 1 != lastStatementLine
			&& !IsCommentOrRegionOrDisabledText(bracesPair.Open.Parent, closeBraceLine - 1))
			yield return new SolutionStyleError(StyleErrorType.ExcessLines02, bracesPair.Close);
	}

	private static SolutionStyleError ReportWhenExistExcessLineBetweenDeclaration(BracesPair bracesPair)
	{
		var openBraceLine = GetStartLine(bracesPair.Open);
		var declarationLine = GetEndDeclarationLine(bracesPair.Open.Parent);
		if (declarationLine == -1 || openBraceLine - declarationLine <= 1)
			return null;

		return new SolutionStyleError(StyleErrorType.ExcessLines03, bracesPair.Open);
	}

	private static SolutionStyleError ReportWhenNotExistLineBetweenBlocks(BracesPair bracesPair)
	{
		var closeBraceLine = GetStartLine(bracesPair.Close);
		var openBraceLine = GetStartLine(bracesPair.Open);

		var nextSyntaxNode = GetNextNode(bracesPair.Open.Parent);
		if (nextSyntaxNode is null or StatementSyntax)
			return null;
		var nextSyntaxNodeLine = GetStartLine(nextSyntaxNode);
		if (closeBraceLine + 1 == nextSyntaxNodeLine && closeBraceLine != openBraceLine)
			return new SolutionStyleError(StyleErrorType.ExcessLines04, bracesPair.Close);

		return null;
	}

	private static SyntaxNode GetNextNode(SyntaxNode syntaxNode)
	{
		SyntaxNode[] statements;
		SyntaxNode target;
		if (syntaxNode.Parent is MethodDeclarationSyntax or StatementSyntax or ConstructorDeclarationSyntax)
		{
			statements = GetStatements(syntaxNode.Parent.Parent);
			target = syntaxNode.Parent;
		}
		else
		{
			statements = GetStatements(syntaxNode.Parent);
			target = syntaxNode;
		}

		if (statements.Length == 0)
			return null;

		for (var i = 0; i < statements.Length; ++i)
		{
			var statement = statements[i];
			if (statement == target && i < statements.Length - 1)
				return statements[i + 1];
		}

		return null;
	}

	/// <summary>
	/// Check is line is inside multiline comment or inside #region..#endregion or inside #ifdef..#endif
	/// </summary>
	private static bool IsCommentOrRegionOrDisabledText(SyntaxNode syntaxNode, int line)
	{
		return syntaxNode.DescendantTrivia()
			.Where(x => x.IsKind(SyntaxKind.MultiLineCommentTrivia)
						|| x.IsKind(SyntaxKind.SingleLineCommentTrivia)
						|| x.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
						|| x.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
						|| x.IsKind(SyntaxKind.RegionDirectiveTrivia)
						|| x.IsKind(SyntaxKind.EndRegionDirectiveTrivia)
						|| x.IsKind(SyntaxKind.IfDirectiveTrivia)
						|| x.IsKind(SyntaxKind.EndIfDirectiveTrivia)
						|| x.IsKind(SyntaxKind.DisabledTextTrivia))
			.Any(x =>
			{
				var startLine = x.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
				var endLine = x.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
				return line >= startLine && line <= endLine;
			});
	}

	private static SyntaxNode[] GetStatements(SyntaxNode syntaxNode)
	{
		return syntaxNode switch
		{
			BlockSyntax blockSyntax => blockSyntax.Statements.Cast<SyntaxNode>().ToArray(),
			ClassDeclarationSyntax classDeclarationSyntax => classDeclarationSyntax.Members.Cast<SyntaxNode>().ToArray(),
			NamespaceDeclarationSyntax namespaceDeclarationSyntax => namespaceDeclarationSyntax.Usings.Cast<SyntaxNode>().Concat(namespaceDeclarationSyntax.Members).ToArray(),
			CompilationUnitSyntax compilationUnitSyntax => compilationUnitSyntax.Members.Cast<SyntaxNode>().ToArray(),
			_ => Array.Empty<SyntaxNode>()
		};
	}

	private static int GetEndDeclarationLine(SyntaxNode syntaxNode)
	{
		switch (syntaxNode)
		{
			case ConstructorDeclarationSyntax constructorDeclarationSyntax:
				var initializer = constructorDeclarationSyntax.Initializer;
				return initializer != null
					? GetEndLine(initializer)
					: GetEndArgumentsLine(constructorDeclarationSyntax);
			case TypeDeclarationSyntax typeDeclarationSyntax:
				var baseListSyntax = typeDeclarationSyntax.BaseList;
				var typeConstraints = typeDeclarationSyntax.ConstraintClauses;
				if (typeConstraints.Any())
					return GetEndLine(typeConstraints.Last());
				return baseListSyntax == null
					? GetStartLine(typeDeclarationSyntax.Identifier)
					: GetEndLine(baseListSyntax);
			case BlockSyntax blockSyntax:
				if (blockSyntax.Parent.IsKind(SyntaxKind.Block))
					return -1;
				return GetEndDeclarationLine(blockSyntax.Parent);
			case MethodDeclarationSyntax methodDeclarationSyntax:
				var constraintClauseSyntaxes = methodDeclarationSyntax.ConstraintClauses;
				return !constraintClauseSyntaxes.Any()
					? GetEndArgumentsLine(methodDeclarationSyntax)
					: GetEndLine(constraintClauseSyntaxes.Last());
			case IfStatementSyntax ifStatementSyntax:
				return GetEndLine(ifStatementSyntax.CloseParenToken);
			case WhileStatementSyntax whileStatementSyntax:
				return GetEndLine(whileStatementSyntax.CloseParenToken);
			case ForEachStatementSyntax forEachStatementSyntax:
				return GetEndLine(forEachStatementSyntax.CloseParenToken);
			case ForStatementSyntax forStatementSyntax:
				return GetEndLine(forStatementSyntax.CloseParenToken);
			case LocalFunctionStatementSyntax localFunctionStatementSyntax:
				return GetEndLine(localFunctionStatementSyntax.ParameterList);
			case CatchClauseSyntax catchClauseSyntax:
				return catchClauseSyntax.Filter is null
					? GetEndLine(catchClauseSyntax.Declaration)
					: GetEndLine(catchClauseSyntax.Filter);
			default:
				return GetStartLine(syntaxNode);
		}
	}

	private static int GetStartLine(SyntaxToken syntaxToken)
	{
		return syntaxToken.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
	}

	private static int GetStartLine(SyntaxNode syntaxNode)
	{
		return syntaxNode.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
	}

	private static int GetEndLine(SyntaxToken syntaxToken)
	{
		return syntaxToken.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
	}

	private static int GetEndLine(SyntaxNode syntaxNode)
	{
		return syntaxNode.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
	}

	private static int GetEndArgumentsLine(MethodDeclarationSyntax declarationSyntax)
	{
		var parametersCloseToken = declarationSyntax.ParameterList.CloseParenToken;
		return parametersCloseToken.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
	}

	private static int GetEndArgumentsLine(ConstructorDeclarationSyntax declarationSyntax)
	{
		var parametersCloseToken = declarationSyntax.ParameterList.CloseParenToken;
		return parametersCloseToken.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
	}
}