using System.ComponentModel;

namespace TestProject
{
    public class ExampleClass : PropertyChangedBase
    {
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
