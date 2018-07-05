using System.ComponentModel;

namespace TestProject
{
    public class MissingPropertyChangedExampleClass : INotifyPropertyChanged
    {
        public int Number { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class BadPropertyChangedExampleClass : INotifyPropertyChanged
    {
        public int Number { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(object name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name.ToString()));
    }

    public class ExampleClass : PropertyChangedBase
    {
        private int _property;
        public int Property { get => _property; set => _property = value; }

        public int Number { get; set; }

        public bool Flag { get; set; }

        public CustomStruct Custom { get; set; }

        public string GivenNames { get; set; }

        public string FamilyName { get; set; }

        public string FullName => $"{GivenNames} {FamilyName}";
    }

    public class ExampleBeforeAfterClass : INotifyPropertyChanged
    {
        public int Number { get; set; }

        public bool Flag { get; set; }

        public CustomStruct Custom { get; set; }

        public string GivenNames { get; set; }

        public string FamilyName { get; set; }

        public string FullName => $"{GivenNames} {FamilyName}";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name, object before, object after) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
