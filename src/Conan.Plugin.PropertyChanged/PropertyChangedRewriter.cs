using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace Conan.Plugin.PropertyChanged
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PropertyChangedRewriter : CompilationRewriter, ISimpleRewriter
    {
        public override Compilation Rewrite(CompilationRewriterContext context) =>
            Rewrite(context.Compilation, context.ReportDiagnostic);

        public Compilation Rewrite(Compilation compilation, Action<Diagnostic> reportDiagnostic)
        {
            var rewriter = new PropertyChangedSyntaxRewriter(compilation, reportDiagnostic);

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
