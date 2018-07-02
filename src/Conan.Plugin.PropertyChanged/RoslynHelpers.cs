using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Conan.Plugin.PropertyChanged
{
    public static class RoslynHelpers
    {
        public static ArgumentListSyntax CreateArguments(params ExpressionSyntax[] args) =>
            args.Length == 1 ?
            ArgumentList(SingletonSeparatedList(Argument(args[0]))) :
            ArgumentList(SeparatedList<ArgumentSyntax>(args
                .Select(a => (SyntaxNodeOrToken)Argument(a)).Link(Token(SyntaxKind.CommaToken))));

        public static IEnumerable<T> Link<T>(this IEnumerable<T> items, T joiner)
        {
            using (var enumerator = items.GetEnumerator())
            {
                if (enumerator.MoveNext())
                    yield return enumerator.Current;

                while (enumerator.MoveNext())
                {
                    yield return joiner;
                    yield return enumerator.Current;
                }
            }
        }

        public static AccessorDeclarationSyntax WithSemicolon(this AccessorDeclarationSyntax accessorDeclarationSyntax) =>
            accessorDeclarationSyntax.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        public static LiteralExpressionSyntax AsStringLiteral(this string text) =>
            LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(text));

        public static IdentifierNameSyntax AsIdentifierName(this string name) => IdentifierName(name);

        public static IdentifierNameSyntax AsIdentifierName(this ISymbol symbol) => IdentifierName(symbol.Name);

        public static BlockSyntax CallMethod(this BlockSyntax block, string name, params ExpressionSyntax[] args) =>
            block.AddStatements(ExpressionStatement(InvocationExpression(name.AsIdentifierName()).WithArgumentList(CreateArguments(args))));

        public static BlockSyntax CallMethod(this BlockSyntax block, IMethodSymbol method, params ExpressionSyntax[] args) =>
            CallMethod(block, method.Name, args);

        public static BlockSyntax Return(this BlockSyntax block, ExpressionSyntax expression = null) =>
            block.AddStatements(ReturnStatement(expression));

        public static BlockSyntax Assign(this BlockSyntax block, ExpressionSyntax left, ExpressionSyntax right) =>
            block.AddStatements(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right)));

        public static BlockSyntax Assign(this BlockSyntax block, ISymbol left, ISymbol right) =>
            Assign(block, left.AsIdentifierName(), right.AsIdentifierName());

        public static BlockSyntax Assign(this BlockSyntax block, ISymbol left, string right) =>
            Assign(block, left.AsIdentifierName(), right.AsIdentifierName());

        public static BlockSyntax Assign(this BlockSyntax block, string left, ISymbol right) =>
            Assign(block, left.AsIdentifierName(), right.AsIdentifierName());

        public static BlockSyntax Assign(this BlockSyntax block, ISymbol left, ExpressionSyntax right) =>
            Assign(block, left.AsIdentifierName(), right);

        public static BlockSyntax Assign(this BlockSyntax block, string left, ExpressionSyntax right) =>
            Assign(block, left.AsIdentifierName(), right);

        public static FieldDeclarationSyntax AsFieldDeclaration(this IFieldSymbol fieldSymbol) =>
            FieldDeclaration(VariableDeclaration(
                ParseTypeName(fieldSymbol.Type.ToString()),
                SeparatedList(new[] { VariableDeclarator(Identifier(fieldSymbol.Name)) })
            ));

        public static VariableDeclarationSyntax Var() => VariableDeclaration(IdentifierName("var"));

        public static BlockSyntax If(this BlockSyntax block, ExpressionSyntax condition, StatementSyntax statement) =>
            block.AddStatements(IfStatement(condition, statement));

        public static BlockSyntax DeclareVariable(this BlockSyntax block, string name, ExpressionSyntax value) =>
            block.AddStatements(
                LocalDeclarationStatement(
                    Var()
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier(name))
                            .WithInitializer(
                                EqualsValueClause(value))))));


    }
}
