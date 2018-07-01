namespace TestProject
{
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
