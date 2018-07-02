using System;
using System.ComponentModel;
using System.Linq;
using Xunit;

namespace Conan.Plugin.PropertyChanged.Tests
{
    public class DependencyTests
    {
        [Fact]
        public void GetterBody()
        {
            var code = @"using System.ComponentModel;
public class TestClass : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public string GivenNames { get; set; }
    public string FamilyName { get; set; }

    public string FullName { get { return $""{GivenNames} {FamilyName}""; } }
}
";
            AssertDependency(code);
        }

        [Fact]
        public void GetterExpressionBody()
        {
            var code = @"using System.ComponentModel;
public class TestClass : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public string GivenNames { get; set; }
    public string FamilyName { get; set; }

    public string FullName { get => $""{GivenNames} {FamilyName}""; }
}
";
            AssertDependency(code);
        }

        [Fact]
        public void Something()
        {
            var code = @"using System.ComponentModel;
public class TestClass : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public string GivenNames { get; set; }
    public string FamilyName { get; set; }

    public string FullName => $""{GivenNames} {FamilyName}"";
}
";
            AssertDependency(code);
        }

        private void AssertDependency(string code)
        {
            var assembly = RAssert.Compile(code);
            var type = assembly.GetTypes().Single(x => x.Name == "TestClass");

            var givenNamesProperty = type.GetProperty("GivenNames");
            var familyNameProperty = type.GetProperty("FamilyName");

            var instance = (INotifyPropertyChanged)Activator.CreateInstance(type);

            var count = 0;
            instance.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "FullName")
                    count++;
            };

            givenNamesProperty.SetValue(instance, "Cameron");
            familyNameProperty.SetValue(instance, "MacFarland");

            Assert.Equal(2, count);
        }
    }
}
