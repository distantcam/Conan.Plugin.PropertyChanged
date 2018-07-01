using Xunit;

namespace Conan.Plugin.PropertyChanged.Tests
{
    public class CompileTests
    {
        [Fact]
        public void MultipleClasses()
        {
            var code = @"using System.ComponentModel;
public class PropertyChangedBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
public class ExampleClass : PropertyChangedBase
{
    public CustomStruct Custom { get; set; }
}
public struct CustomStruct
{
    public CustomStruct(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public override bool Equals(object obj) => Value.Equals(obj);
    public override int GetHashCode() => Value.GetHashCode();
}
";

            RAssert.Compile(code);
        }
    }
}
