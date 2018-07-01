using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Conan.Plugin.PropertyChanged
{
    public interface ISimpleRewriter
    {
        Compilation Rewrite(Compilation compilation, Action<Diagnostic> reportDiagnostic);
    }
}
