using Xunit;

namespace Conan.Plugin.PropertyChanged.Tests
{
	public class SingleFireTest
	{
		[Theory]
		[InlineData("bool", true, false)]
		[InlineData("byte", (byte)1, (byte)2)]
		[InlineData("char", 'c', 'd')]
		[InlineData("double", 1.0, 1.1)]
		[InlineData("float", 1.0f, 1.1f)]
		[InlineData("int", 1, 2)]
		[InlineData("long", (long)1, (long)2)]
		[InlineData("sbyte", (sbyte)1, (sbyte)2)]
		[InlineData("short", (short)1, (short)2)]
		[InlineData("string", "foo", "bar")]
		[InlineData("uint", (uint)1, (uint)2)]
		[InlineData("ulong", (ulong)1, (ulong)2)]
		[InlineData("ushort", (ushort)1, (ushort)2)]
		[InlineData("int?", 1, 2)]
        public void ClassWithBase<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class PropertyChangedBase : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}}

public class TestClass : PropertyChangedBase
{{
    public {type} Property {{ get; set; }}
}}";
            RAssert.PropertyChangedFired(code, value1);
        }
		[Theory]
		[ClassData(typeof(ComplexTestData))]
        public void ClassWithBase_ComplexTypes<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class PropertyChangedBase : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}}

public class TestClass : PropertyChangedBase
{{
    public {type} Property {{ get; set; }}
}}";
            RAssert.PropertyChangedFired(code, value1);
        }
		[Theory]
		[InlineData("bool", true, false)]
		[InlineData("byte", (byte)1, (byte)2)]
		[InlineData("char", 'c', 'd')]
		[InlineData("double", 1.0, 1.1)]
		[InlineData("float", 1.0f, 1.1f)]
		[InlineData("int", 1, 2)]
		[InlineData("long", (long)1, (long)2)]
		[InlineData("sbyte", (sbyte)1, (sbyte)2)]
		[InlineData("short", (short)1, (short)2)]
		[InlineData("string", "foo", "bar")]
		[InlineData("uint", (uint)1, (uint)2)]
		[InlineData("ulong", (ulong)1, (ulong)2)]
		[InlineData("ushort", (ushort)1, (ushort)2)]
		[InlineData("int?", 1, 2)]
        public void SingleClass<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class TestClass : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	public {type} Property {{ get; set; }}
}}";
            RAssert.PropertyChangedFired(code, value1);
        }
		[Theory]
		[ClassData(typeof(ComplexTestData))]
        public void SingleClass_ComplexTypes<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class TestClass : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	public {type} Property {{ get; set; }}
}}";
            RAssert.PropertyChangedFired(code, value1);
        }
		[Theory]
		[InlineData("bool", true, false)]
		[InlineData("byte", (byte)1, (byte)2)]
		[InlineData("char", 'c', 'd')]
		[InlineData("double", 1.0, 1.1)]
		[InlineData("float", 1.0f, 1.1f)]
		[InlineData("int", 1, 2)]
		[InlineData("long", (long)1, (long)2)]
		[InlineData("sbyte", (sbyte)1, (sbyte)2)]
		[InlineData("short", (short)1, (short)2)]
		[InlineData("string", "foo", "bar")]
		[InlineData("uint", (uint)1, (uint)2)]
		[InlineData("ulong", (ulong)1, (ulong)2)]
		[InlineData("ushort", (ushort)1, (ushort)2)]
		[InlineData("int?", 1, 2)]
        public void BeforeAfterHelper<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class TestClass : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name, object before, object after) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	public {type} Property {{ get; set; }}
}}";
            RAssert.PropertyChangedFired(code, value1);
        }
		[Theory]
		[ClassData(typeof(ComplexTestData))]
        public void BeforeAfterHelper_ComplexTypes<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class TestClass : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name, object before, object after) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	public {type} Property {{ get; set; }}
}}";
            RAssert.PropertyChangedFired(code, value1);
        }
	}
	public class GuardTest
	{
		[Theory]
		[InlineData("bool", true, false)]
		[InlineData("byte", (byte)1, (byte)2)]
		[InlineData("char", 'c', 'd')]
		[InlineData("double", 1.0, 1.1)]
		[InlineData("float", 1.0f, 1.1f)]
		[InlineData("int", 1, 2)]
		[InlineData("long", (long)1, (long)2)]
		[InlineData("sbyte", (sbyte)1, (sbyte)2)]
		[InlineData("short", (short)1, (short)2)]
		[InlineData("string", "foo", "bar")]
		[InlineData("uint", (uint)1, (uint)2)]
		[InlineData("ulong", (ulong)1, (ulong)2)]
		[InlineData("ushort", (ushort)1, (ushort)2)]
		[InlineData("int?", 1, 2)]
        public void ClassWithBase<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class PropertyChangedBase : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}}

public class TestClass : PropertyChangedBase
{{
    public {type} Property {{ get; set; }}
}}";
            RAssert.ChangeGuardDoesNotRefire(code, value1);
        }
		[Theory]
		[ClassData(typeof(ComplexTestData))]
        public void ClassWithBase_ComplexTypes<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class PropertyChangedBase : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}}

public class TestClass : PropertyChangedBase
{{
    public {type} Property {{ get; set; }}
}}";
            RAssert.ChangeGuardDoesNotRefire(code, value1);
        }
		[Theory]
		[InlineData("bool", true, false)]
		[InlineData("byte", (byte)1, (byte)2)]
		[InlineData("char", 'c', 'd')]
		[InlineData("double", 1.0, 1.1)]
		[InlineData("float", 1.0f, 1.1f)]
		[InlineData("int", 1, 2)]
		[InlineData("long", (long)1, (long)2)]
		[InlineData("sbyte", (sbyte)1, (sbyte)2)]
		[InlineData("short", (short)1, (short)2)]
		[InlineData("string", "foo", "bar")]
		[InlineData("uint", (uint)1, (uint)2)]
		[InlineData("ulong", (ulong)1, (ulong)2)]
		[InlineData("ushort", (ushort)1, (ushort)2)]
		[InlineData("int?", 1, 2)]
        public void SingleClass<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class TestClass : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	public {type} Property {{ get; set; }}
}}";
            RAssert.ChangeGuardDoesNotRefire(code, value1);
        }
		[Theory]
		[ClassData(typeof(ComplexTestData))]
        public void SingleClass_ComplexTypes<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class TestClass : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	public {type} Property {{ get; set; }}
}}";
            RAssert.ChangeGuardDoesNotRefire(code, value1);
        }
		[Theory]
		[InlineData("bool", true, false)]
		[InlineData("byte", (byte)1, (byte)2)]
		[InlineData("char", 'c', 'd')]
		[InlineData("double", 1.0, 1.1)]
		[InlineData("float", 1.0f, 1.1f)]
		[InlineData("int", 1, 2)]
		[InlineData("long", (long)1, (long)2)]
		[InlineData("sbyte", (sbyte)1, (sbyte)2)]
		[InlineData("short", (short)1, (short)2)]
		[InlineData("string", "foo", "bar")]
		[InlineData("uint", (uint)1, (uint)2)]
		[InlineData("ulong", (ulong)1, (ulong)2)]
		[InlineData("ushort", (ushort)1, (ushort)2)]
		[InlineData("int?", 1, 2)]
        public void BeforeAfterHelper<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class TestClass : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name, object before, object after) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	public {type} Property {{ get; set; }}
}}";
            RAssert.ChangeGuardDoesNotRefire(code, value1);
        }
		[Theory]
		[ClassData(typeof(ComplexTestData))]
        public void BeforeAfterHelper_ComplexTypes<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class TestClass : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name, object before, object after) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	public {type} Property {{ get; set; }}
}}";
            RAssert.ChangeGuardDoesNotRefire(code, value1);
        }
	}
	public class MultiFireTest
	{
		[Theory]
		[InlineData("bool", true, false)]
		[InlineData("byte", (byte)1, (byte)2)]
		[InlineData("char", 'c', 'd')]
		[InlineData("double", 1.0, 1.1)]
		[InlineData("float", 1.0f, 1.1f)]
		[InlineData("int", 1, 2)]
		[InlineData("long", (long)1, (long)2)]
		[InlineData("sbyte", (sbyte)1, (sbyte)2)]
		[InlineData("short", (short)1, (short)2)]
		[InlineData("string", "foo", "bar")]
		[InlineData("uint", (uint)1, (uint)2)]
		[InlineData("ulong", (ulong)1, (ulong)2)]
		[InlineData("ushort", (ushort)1, (ushort)2)]
		[InlineData("int?", 1, 2)]
        public void ClassWithBase<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class PropertyChangedBase : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}}

public class TestClass : PropertyChangedBase
{{
    public {type} Property {{ get; set; }}
}}";
            RAssert.ChangeGuardDoesDetectChange(code, value1, value2);
        }
		[Theory]
		[ClassData(typeof(ComplexTestData))]
        public void ClassWithBase_ComplexTypes<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class PropertyChangedBase : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}}

public class TestClass : PropertyChangedBase
{{
    public {type} Property {{ get; set; }}
}}";
            RAssert.ChangeGuardDoesDetectChange(code, value1, value2);
        }
		[Theory]
		[InlineData("bool", true, false)]
		[InlineData("byte", (byte)1, (byte)2)]
		[InlineData("char", 'c', 'd')]
		[InlineData("double", 1.0, 1.1)]
		[InlineData("float", 1.0f, 1.1f)]
		[InlineData("int", 1, 2)]
		[InlineData("long", (long)1, (long)2)]
		[InlineData("sbyte", (sbyte)1, (sbyte)2)]
		[InlineData("short", (short)1, (short)2)]
		[InlineData("string", "foo", "bar")]
		[InlineData("uint", (uint)1, (uint)2)]
		[InlineData("ulong", (ulong)1, (ulong)2)]
		[InlineData("ushort", (ushort)1, (ushort)2)]
		[InlineData("int?", 1, 2)]
        public void SingleClass<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class TestClass : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	public {type} Property {{ get; set; }}
}}";
            RAssert.ChangeGuardDoesDetectChange(code, value1, value2);
        }
		[Theory]
		[ClassData(typeof(ComplexTestData))]
        public void SingleClass_ComplexTypes<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class TestClass : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	public {type} Property {{ get; set; }}
}}";
            RAssert.ChangeGuardDoesDetectChange(code, value1, value2);
        }
		[Theory]
		[InlineData("bool", true, false)]
		[InlineData("byte", (byte)1, (byte)2)]
		[InlineData("char", 'c', 'd')]
		[InlineData("double", 1.0, 1.1)]
		[InlineData("float", 1.0f, 1.1f)]
		[InlineData("int", 1, 2)]
		[InlineData("long", (long)1, (long)2)]
		[InlineData("sbyte", (sbyte)1, (sbyte)2)]
		[InlineData("short", (short)1, (short)2)]
		[InlineData("string", "foo", "bar")]
		[InlineData("uint", (uint)1, (uint)2)]
		[InlineData("ulong", (ulong)1, (ulong)2)]
		[InlineData("ushort", (ushort)1, (ushort)2)]
		[InlineData("int?", 1, 2)]
        public void BeforeAfterHelper<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class TestClass : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name, object before, object after) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	public {type} Property {{ get; set; }}
}}";
            RAssert.ChangeGuardDoesDetectChange(code, value1, value2);
        }
		[Theory]
		[ClassData(typeof(ComplexTestData))]
        public void BeforeAfterHelper_ComplexTypes<T>(string type, T value1, T value2)
        {
            var code = $@"using System.ComponentModel;
using Conan.Plugin.PropertyChanged.Tests;
public class TestClass : INotifyPropertyChanged
{{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name, object before, object after) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	public {type} Property {{ get; set; }}
}}";
            RAssert.ChangeGuardDoesDetectChange(code, value1, value2);
        }
	}
}