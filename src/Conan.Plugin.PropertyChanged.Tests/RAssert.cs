using Conan.Plugin.PropertyChanged;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Conan.Plugin.PropertyChanged.Tests
{
    public static class RAssert
    {
        public static void PropertyChangedFired<T>(string code, T value, string className = "TestClass", string propertyName = "Property")
        {
            var (instance, property) = CreateTestInstance(code, className, propertyName);

            property.SetValue(instance, default(T));

            var count = 0;
            instance.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                    count++;
            };

            property.SetValue(instance, value);

            Assert.Equal(1, count);
        }

        public static void ChangeGuardDoesNotRefire<T>(string code, T value, string className = "TestClass", string propertyName = "Property")
        {
            var (instance, property) = CreateTestInstance(code, className, propertyName);

            property.SetValue(instance, default(T));

            var count = 0;
            instance.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                    count++;
            };

            property.SetValue(instance, value);
            property.SetValue(instance, value);

            Assert.Equal(1, count);
        }

        public static void ChangeGuardDoesDetectChange<T>(string code, T value1, T value2, string className = "TestClass", string propertyName = "Property")
        {
            var (instance, property) = CreateTestInstance(code, className, propertyName);

            property.SetValue(instance, default(T));

            var count = 0;
            instance.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                    count++;
            };

            property.SetValue(instance, value1);
            property.SetValue(instance, value2);
            property.SetValue(instance, value1);

            Assert.Equal(3, count);
        }

        private static (INotifyPropertyChanged Instance, PropertyInfo Property) CreateTestInstance(string code, string className, string propertyName)
        {
            var assembly = Compile(code);
            var type = assembly.GetTypes().Single(x => x.Name == className);
            var property = type.GetProperty(propertyName);

            var instance = (INotifyPropertyChanged)Activator.CreateInstance(type);

            return (instance, property);
        }

        public static Assembly Compile(string code)
        {
            // Parse the C# code...
            CSharpParseOptions parseOptions = new CSharpParseOptions()
              .WithKind(SourceCodeKind.Regular) // ...as representing a complete .cs file
              .WithLanguageVersion(LanguageVersion.Latest); // ...enabling the latest language features

            // Compile the C# code...
            CSharpCompilationOptions compileOptions =
              new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary) // ...to a dll
              .WithOptimizationLevel(OptimizationLevel.Release) // ...in Release configuration
              .WithAllowUnsafe(enabled: true); // ...enabling unsafe code

            // Invoke the compiler...
            Compilation compilation =
                CSharpCompilation.Create("TestInMemoryAssembly") // ..with some fake dll name
                .WithOptions(compileOptions)
                .AddReferences(
#if DEBUG
                    MetadataReference.CreateFromFile(@"..\..\..\Conan.Plugin.PropertyChanged\bin\Debug\Conan.Plugin.PropertyChanged.dll"),
#else
                    MetadataReference.CreateFromFile(@"..\..\..\Conan.Plugin.PropertyChanged\bin\Release\Conan.Plugin.PropertyChanged.dll"),
#endif
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(RAssert).Assembly.Location)
                );

            // Parse and compile the C# code into a *.dll and *.xml file in-memory
            var tree = CSharpSyntaxTree.ParseText(code, parseOptions);
            compilation = compilation.AddSyntaxTrees(tree);

            var rewriter = new PropertyChangedRewriter();

            compilation = rewriter.Rewrite(compilation, d => { });

            var peStream = new MemoryStream();
            var emitResult = compilation.Emit(peStream);
            if (!emitResult.Success)
                throw new InvalidOperationException("Compilation failed: " + string.Join("\n", emitResult.Diagnostics));

            // Parse the *.dll (with Cecil) and the *.xml (with XDocument)
            peStream.Seek(0, SeekOrigin.Begin);

            return Assembly.Load(peStream.ToArray());
        }
    }
}
