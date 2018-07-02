using System.Collections;
using System.Collections.Generic;

namespace Conan.Plugin.PropertyChanged.Tests
{
    public class ComplexTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "decimal", 1.0m, 1.1m };
            yield return new object[] { "CustomClass", new CustomClass(1), new CustomClass(2) };
            yield return new object[] { "CustomStruct", new CustomStruct(1), new CustomStruct(2) };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
