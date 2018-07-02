using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Conan.Plugin.PropertyChanged
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PropertyChangedRewriter : CompilationRewriter, ISimpleRewriter
    {
        private static readonly DiagnosticDescriptor ThisDescriptor = new DiagnosticDescriptor("PC0001", "PropertyChanged", "{0}", "Design", DiagnosticSeverity.Warning, true);

        class PropertyChangedSyntaxRewriter : CSharpSyntaxRewriter
        {
            private readonly Compilation compilation;
            private readonly INamedTypeSymbol inpcSymbol;

            private List<MemberDeclarationSyntax> membersToAdd = new List<MemberDeclarationSyntax>();
            private ConcurrentDictionary<string, ImmutableArray<string>> dependantProperties = new ConcurrentDictionary<string, ImmutableArray<string>>();
            private IMethodSymbol helperMethod;

            public PropertyChangedSyntaxRewriter(Compilation compilation)
            {
                this.compilation = compilation;
                inpcSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName);
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                helperMethod = FindHelperMethod(node);
                if (helperMethod == null)
                    return node;

                membersToAdd.Clear();

                var model = compilation.GetSemanticModel(node.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(node);
                var properties = node.Members.OfType<PropertyDeclarationSyntax>();
                SetupDependencies(properties, model, symbol);

                var newClass = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

                return newClass.AddMembers(membersToAdd.ToArray());
            }

            public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
            {
                helperMethod = FindHelperMethod(node);
                if (helperMethod == null)
                    return node;

                membersToAdd.Clear();

                var model = compilation.GetSemanticModel(node.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(node);
                var properties = node.Members.OfType<PropertyDeclarationSyntax>();
                SetupDependencies(properties, model, symbol);

                var newStruct = (StructDeclarationSyntax)base.VisitStructDeclaration(node);

                return newStruct.AddMembers(membersToAdd.ToArray());
            }

            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                var symbol = compilation.GetSemanticModel(node.SyntaxTree).GetDeclaredSymbol(node);

                var backingField = symbol.ContainingType.GetMembers().OfType<IFieldSymbol>()
                    .FirstOrDefault(f => f.AssociatedSymbol?.Equals(symbol) == true);
                if (backingField == null)
                    return node;

                // http://roslynquoter.azurewebsites.net/

                BlockSyntax setterBlock = null;

                if (helperMethod.Parameters.Length == 1)
                {
                    // string name
                    if (helperMethod.Parameters[0].Type.SpecialType.Equals(SpecialType.System_String))
                    {
                        setterBlock = Block()
                            .If(Compare(backingField), ReturnStatement())
                            .Assign(backingField, "value")
                            .CallMethod(helperMethod, node.Identifier.Text.AsStringLiteral());

                        if (dependantProperties.TryGetValue(node.Identifier.Text, out var dps))
                        {
                            setterBlock = setterBlock.AddStatements(dps.Select(dp =>
                                ExpressionStatement(InvocationExpression(helperMethod.Name.AsIdentifierName())
                                    .WithArgumentList(RoslynHelpers.CreateArguments(
                                        dp.AsStringLiteral()
                                    )))).ToArray());
                        }
                    }
                }

                if (helperMethod.Parameters.Length == 3)
                {
                    // string name, object before, object after
                    if (helperMethod.Parameters[0].Type.SpecialType.Equals(SpecialType.System_String) &&
                        helperMethod.Parameters[1].Type.SpecialType.Equals(SpecialType.System_Object) &&
                        helperMethod.Parameters[2].Type.SpecialType.Equals(SpecialType.System_Object))
                    {
                        setterBlock = Block()
                            .If(Compare(backingField), ReturnStatement())
                            .DeclareVariable("before", node.Identifier.Text.AsIdentifierName())
                            .Assign(backingField, "value")
                            .DeclareVariable("after", node.Identifier.Text.AsIdentifierName())
                            .CallMethod(helperMethod, node.Identifier.Text.AsStringLiteral(), "before".AsIdentifierName(), "after".AsIdentifierName());

                        if (dependantProperties.TryGetValue(node.Identifier.Text, out var dps))
                        {
                            setterBlock = setterBlock.AddStatements(dps.Select(dp =>
                                ExpressionStatement(InvocationExpression(helperMethod.Name.AsIdentifierName())
                                    .WithArgumentList(RoslynHelpers.CreateArguments(
                                        dp.AsStringLiteral(),
                                        "before".AsIdentifierName(),
                                        "after".AsIdentifierName()
                                    )))).ToArray());
                        }

                    }
                }

                if (setterBlock != null)
                {
                    membersToAdd.Add(backingField.AsFieldDeclaration());
                    return PropertyDeclaration(node.Type, node.Identifier)
                        .AddModifiers(node.Modifiers.ToArray())
                        .AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration,
                            Block().Return(backingField.Name.AsIdentifierName())))
                        .AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration,
                            setterBlock));
                }

                // TODO report error that helper could not be called
                return node;
            }

            private void SetupDependencies(IEnumerable<PropertyDeclarationSyntax> properties, SemanticModel model, INamedTypeSymbol symbol)
            {
                dependantProperties.Clear();

                foreach (var property in properties)
                {
                    IEnumerable<IdentifierNameSyntax> nodes = null;

                    if (property.ExpressionBody != null)
                        nodes = property.ExpressionBody.DescendantNodes().OfType<IdentifierNameSyntax>();
                    else if (property.AccessorList != null)
                    {
                        var getter = property.AccessorList.Accessors.FirstOrDefault(a => a.Kind() == SyntaxKind.GetAccessorDeclaration);
                        if (getter != null)
                        {
                            if (getter.Body != null)
                                nodes = getter.Body.DescendantNodes().OfType<IdentifierNameSyntax>();
                            else if (getter.ExpressionBody != null)
                                nodes = getter.ExpressionBody.DescendantNodes().OfType<IdentifierNameSyntax>();
                        }
                    }

                    if (nodes == null)
                        continue;

                    foreach (var item in nodes)
                    {
                        var info = model.GetSymbolInfo(item);
                        var propertySymbol = info.Symbol as IPropertySymbol;
                        if (propertySymbol.ContainingType.Equals(symbol))
                        {
                            dependantProperties.AddOrUpdate(propertySymbol.Name, _ => ImmutableArray<string>.Empty.Add(property.Identifier.Text), (_, l) => l.Add(property.Identifier.Text));
                        }
                    }
                }
            }

            private static ExpressionSyntax Compare(IFieldSymbol field)
            {
                if (field.Type.SpecialType == SpecialType.System_Boolean ||
                    field.Type.SpecialType == SpecialType.System_Char ||
                    field.Type.SpecialType == SpecialType.System_SByte ||
                    field.Type.SpecialType == SpecialType.System_Byte ||
                    field.Type.SpecialType == SpecialType.System_Int16 ||
                    field.Type.SpecialType == SpecialType.System_UInt16 ||
                    field.Type.SpecialType == SpecialType.System_Int32 ||
                    field.Type.SpecialType == SpecialType.System_UInt32 ||
                    field.Type.SpecialType == SpecialType.System_Int64 ||
                    field.Type.SpecialType == SpecialType.System_UInt64 ||
                    field.Type.SpecialType == SpecialType.System_Single ||
                    field.Type.SpecialType == SpecialType.System_Double)
                    return BinaryExpression(SyntaxKind.EqualsExpression, field.AsIdentifierName(), "value".AsIdentifierName());

                return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        PredefinedType(
                            Token(SyntaxKind.ObjectKeyword)),
                        IdentifierName("Equals")))
                    .WithArgumentList(RoslynHelpers.CreateArguments(field.AsIdentifierName(), "value".AsIdentifierName()));
            }

            private IMethodSymbol FindHelperMethod(ClassDeclarationSyntax node)
            {
                var type = compilation.GetSemanticModel(node.SyntaxTree).GetDeclaredSymbol(node);

                if (!ImplementsInterface(type, inpcSymbol))
                    return null;

                return FindHelperMethod(type);
            }

            private IMethodSymbol FindHelperMethod(StructDeclarationSyntax node)
            {
                var type = compilation.GetSemanticModel(node.SyntaxTree).GetDeclaredSymbol(node);

                if (!ImplementsInterface(type, inpcSymbol))
                    return null;

                return FindHelperMethod(type);
            }

            private static IMethodSymbol FindHelperMethod(INamedTypeSymbol type)
            {
                if (type == null)
                    return null;

                if (type.GetMembers("OnPropertyChanged").FirstOrDefault() is IMethodSymbol helper)
                    return helper;

                return FindHelperMethod(type.BaseType);
            }

            private static bool ImplementsInterface(ITypeSymbol subType, ITypeSymbol superInterface)
            {
                if (subType == null)
                    return false;

                foreach (var @interface in subType.AllInterfaces)
                {
                    if (@interface.TypeKind == TypeKind.Interface && @interface == superInterface)
                    {
                        return true;
                    }
                }
                return ImplementsInterface(subType.BaseType, superInterface);
            }
        }

        public override Compilation Rewrite(CompilationRewriterContext context) =>
            Rewrite(context.Compilation, context.ReportDiagnostic);

        public Compilation Rewrite(Compilation compilation, Action<Diagnostic> reportDiagnostic)
        {
            var rewriter = new PropertyChangedSyntaxRewriter(compilation);

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var newRoot = rewriter.Visit(root);
                var newSyntaxTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);
                compilation = compilation.ReplaceSyntaxTree(syntaxTree, newSyntaxTree);
            }

            return compilation;
        }
    }
}
