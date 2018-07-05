using Xunit;

namespace Conan.Plugin.PropertyChanged.Tests
{
    public class SpecialTests
    {
        [Fact]
        public void WrongHelperType()
        {
            var code = @"using System.ComponentModel;
public class TestClass : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(object name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name.ToString()));
	public int Property { get; set; }
}
";

            RAssert.PropertyChangedNotFired(code, 1);
        }

        [Fact]
        public void NormalPropertyNotConverted()
        {
            var code = @"using System.ComponentModel;
public class TestClass : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private int _property;
    public int Property { get => _property; set => _property = value; }
}
";

            RAssert.PropertyChangedNotFired(code, 1);
        }
    }
}
