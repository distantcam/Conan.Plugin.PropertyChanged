namespace Conan.Plugin.PropertyChanged.Tests
{
    public class CustomClass
    {
        public CustomClass(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public override bool Equals(object obj) => Value.Equals(obj);
        public override int GetHashCode() => Value.GetHashCode();
    }

    public struct CustomStruct
    {
        public CustomStruct(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public override bool Equals(object obj) => obj is CustomStruct otherStruct && Value == otherStruct.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }
}
