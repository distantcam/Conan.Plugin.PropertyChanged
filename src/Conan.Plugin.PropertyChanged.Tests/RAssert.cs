﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Conan.Plugin.PropertyChanged.Tests
{
    public static class RAssert
    {
        public static void PropertyChangedNotFired<T>(string code, T value, string className = "TestClass", string propertyName = "Property")
        {
            HasPropertyChanged<T>(code, (instance, property) =>
            {
                property.SetValue(instance, value);
            }, 0, className, propertyName);
        }

        public static void PropertyChangedFired<T>(string code, T value, string className = "TestClass", string propertyName = "Property")
        {
            HasPropertyChanged<T>(code, (instance, property) =>
            {
                property.SetValue(instance, value);
            }, 1, className, propertyName);
        }

        public static void ChangeGuardDoesNotRefire<T>(string code, T value, string className = "TestClass", string propertyName = "Property")
        {
            HasPropertyChanged<T>(code, (instance, property) =>
            {
                property.SetValue(instance, value);
                property.SetValue(instance, value);
            }, 1, className, propertyName);
        }

        public static void ChangeGuardDoesDetectChange<T>(string code, T value1, T value2, string className = "TestClass", string propertyName = "Property")
        {
            HasPropertyChanged<T>(code, (instance, property) =>
            {
                property.SetValue(instance, value1);
                property.SetValue(instance, value2);
                property.SetValue(instance, value1);
            }, 3, className, propertyName);
        }

        private static void HasPropertyChanged<T>(string code, Action<INotifyPropertyChanged, PropertyInfo> action, int expectedCount, string className = "TestClass", string propertyName = "Property")
        {
            var (instance, property) = CreateTestInstance(code, className, propertyName);

            property.SetValue(instance, default(T));

            var count = 0;
            instance.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                    count++;
            };

            action(instance, property);

            Assert.Equal(expectedCount, count);
        }

        private static (INotifyPropertyChanged Instance, PropertyInfo Property) CreateTestInstance(string code, string className, string propertyName)
        {
            var (assembly, diagnostics) = Compile(code);
            var type = assembly.GetTypes().Single(x => x.Name == className);
            var property = type.GetProperty(propertyName);

            var instance = (INotifyPropertyChanged)Activator.CreateInstance(type);

            return (instance, property);
        }

        public static (Assembly Assembly, List<Diagnostic> Diagnostics) Compile(string code)
        {
            // Parse the C# code...
            CSharpParseOptions parseOptions = new CSharpParseOptions()
              .WithKind(SourceCodeKind.Regular) // ...as representing a complete .cs file
              .WithLanguageVersion(LanguageVersion.Latest); // ...enabling the latest language features

            // Compile the C# code...
            CSharpCompilationOptions compileOptions =
              new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary) // ...to a dll
              .WithWarningLevel(2)
              .WithOptimizationLevel(OptimizationLevel.Release) // ...in Release configuration
              .WithAllowUnsafe(enabled: true); // ...enabling unsafe code

            // Invoke the compiler...
            Compilation compilation =
                CSharpCompilation.Create("TestInMemoryAssembly") // ..with some fake dll name
                .WithOptions(compileOptions)
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ISet<>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(RAssert).Assembly.Location),
#if DEBUG
                    MetadataReference.CreateFromFile(@"..\..\..\Conan.Plugin.PropertyChanged\bin\Debug\Conan.Plugin.PropertyChanged.dll")
#else
                    MetadataReference.CreateFromFile(@"..\..\..\Conan.Plugin.PropertyChanged\bin\Release\Conan.Plugin.PropertyChanged.dll")
#endif
                );

            // Parse and compile the C# code into a *.dll and *.xml file in-memory
            var tree = CSharpSyntaxTree.ParseText(code, parseOptions);
            compilation = compilation.AddSyntaxTrees(tree);

            var diagnostics = new List<Diagnostic>();

            var rewriter = new PropertyChangedRewriter();

            compilation = rewriter.Rewrite(compilation, diagnostics.Add);

            var peStream = new MemoryStream();
            var emitResult = compilation.Emit(peStream);
            if (!emitResult.Success)
                throw new InvalidOperationException("Compilation failed: " + string.Join("\n", emitResult.Diagnostics));

            // Parse the *.dll (with Cecil) and the *.xml (with XDocument)
            peStream.Seek(0, SeekOrigin.Begin);

            return (Assembly.Load(peStream.ToArray()), diagnostics);
        }
    }
}
