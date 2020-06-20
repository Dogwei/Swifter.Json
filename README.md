# Swifter.Json
#### A powerful, easy-to-use and fastest json serializer and deserializer on .Net platforms.

### If you want to use Swifter.Json, please download or install the latest version on [Nuget](https://www.nuget.org/packages/Swifter.Json).
### 如果您想使用 Swifter.Json，请在 [Nuget](https://www.nuget.org/packages/Swifter.Json) 上下载或安装最新版本。

### Easy to use 简单使用
```C#
public class Demo
{
    public int Id { get; set; }
    public int Name { get; set; }
    public static void Main()
    {
        var json = JsonFormatter.SerializeObject(new { Id = 123, Name = "Dogwei" });
        var dic = JsonFormatter.DeserializeObject<Dictionary<string, object>>(json);
        var obj = JsonFormatter.DeserializeObject<Demo>(json);
    }
}
```


## Supported data structures and types 支持的数据类型和结构
```C#
bool, byte, sbyte, short, ushort, char, int, uint, long, ulong, IntPtr, UIntPtr,
float, double, decimal, string, enum, DateTime, DateTimeOffset, TimeSpan, Guid,
BigInteger, Complex, DBNull, Nullable<T>, Tuple<...>, ValueTuple<...>, Version,
Uri, Assembly, Type, MemberInfo, MethodInfo, FieldInfo, PropertyInfo, ConstructorInfo,	
EventInfo, Array, Multidimensional-Arrays, IList, IList<T>, ICollection, ICollection<T>,	
IDictionary, IDictionary<TKey, TValue>, IEnumerable, IEnumerable<T>, DataSet, DataTable,	
DataRow, DbRowObject, DbDataReader...	
Other types are treated as object 其他类型当作对象处理	
```	

## Supported platforms and runtimes 支持的平台和运行时
```
.NET Framework 2.0+, .NET Core 2.0+, .NET Standard 2.0+, MONO, MONO AOT, MONO FULL-AOT,
Unity, Xamarin.iOS, Xamarin.Android

Uncertain：Unity IL2CPP

Unsupported: Sliverlight

Note:
    .NET Core use the Core version, and other platforms use the Framework version or Standard version.
    Because the Core version is performance-optimized.
    the Framework version and Standard version are optimized for compatibility.
    the Framework version and Standard version can run directly on AOT platforms.
注意：
    .NET Core 请使用 Core 版本，其他平台和运行时请使用 Framework 版本或 Standard 版本。
    因为 Core 版本专为性能优化，Framework 版本和 Standard 版本为兼容性优化。
    Framework 版本和 Standard 版本可以直接在 AOT 平台上运行。
```

## Supported features 支持的功能
```
LoopReferencingNull:
    Objects that appear a loop reference during serialization are treated as Null.
    在序列化时出现循环引用的对象将用 Null 表示。
    
MultiReferencingReference:
    Allow use { "$ref": "#/obj/1/target" } to represent objects that are repeatedly referenced.
    允许使用 { "$ref": "#/obj/1/target" } 写法表示重复引用的对象。
    
AsOrderedObjectDeserialize
    Perform as-ordered object fields parsing, which can improve parsing performance of ordered Json objects.
    执行假定有序的对象字段解析，这可以提高有序 Json 对象的解析性能。
    
DeflateDeserialize
    Perform deflate(no spaces) Json parsing, which can improve parsing performance.
    执行紧凑（无空白字符）的 Json 解析，这可以提高解析性能。
    
Indented
    Json indents and wraps during serialization, which makes Json beautiful.
    序列化时对 Json 进行缩进和换行，让 Json 变得好看。
    
CamelCaseWhenSerialize
    Convert the fields name in the object to camel case during serialization. 
    序列化时，将对象中的字段名称转换为骆驼命名法。 ::: new { Name = "Dogwei" } -> { "name": "Dogwei" }
    
IgnoreNull | IgnoreZero | IgnoreEmptyString
    Null, 0, "" values are ignored during serialization. 
    序列化时分别跳过 Null, 0 和 "" 值。 ::: { A = (string)null, B = 0, C = "", D = 1 } -> { "D": 1 }
    
For more features, please see Swifter.Json.JsonFormatterOptions enum.
更多功能请看 Swifter.Json.JsonFormatterOptions 配置项。
```
## Performance 性能
![Performance](performance.png)
###### ServiceStack.Json, Jil, LitJson, NetJson and etc libraries are not shown because there are too many errors; if necessary, you can clone the test program on GitHub and run. Most of the Json serialization libraries of .NET have been included.

## Demos 示例
##### (1) Deserialize to dynamic 反序列化为 dynamic
```C#
        var list = new List<object>
        {
            { new Dictionary<string, object>() { { "Id", 1 }, { "Name", "Dogwei" } }},
            { new Dictionary<string, object>() { { "Id", 2 }, { "Name", "sg" } }},
            { new Dictionary<string, object>() { { "Id", 3 }, { "Name", "cxw" } }},
            { new Dictionary<string, object>() { { "Id", 4 }, { "Name", "eway" } }},
            {
                new Dictionary<string, object>() { 
                    { "Id", 5 }, 
                    { "Name", "Xinwei Chen" }, 
                    { "Data", new Dictionary<string, object> { { "Age", 21 }, { "Sex", "Male" } } }
                }
            },
        };

        var json = JsonFormatter.SerializeObject(list);

        dynamic dym = JsonFormatter.DeserializeObject<JsonValue>(json);

        Console.WriteLine(dym[0].Name); // Dogwei
        Console.WriteLine(dym[1].Name); // sg
        Console.WriteLine(dym[2].Id); // 3
        Console.WriteLine(dym[3].Id); // 4
        Console.WriteLine(dym[4].Data.Age); // 21
```
##### (2) Attributes 特性
```C#
[RWObject(SkipDefaultValue = RWBoolean.Yes)]
public class Demo
{
    public int Id;
    public string Name;
    [RWField("Age")]
    private int age;
    [RWField(SkipDefaultValue = RWBoolean.No)]
    public int? Sex;
    [RWFormat("yyyy-MM-dd")]
    public DateTime Birthday { get; set; }

    public static void Main()
    {
        var obj = new Demo { Name = "Dogwei", age = 24, Birthday = DateTime.Parse("1996-01-08") };
        var json = JsonFormatter.SerializeObject(obj);
        var dest = JsonFormatter.DeserializeObject<Demo>(json);

        Console.WriteLine(json);
        // {"Age":24,"Birthday":"1996-01-08","Name":"Dogwei","Sex":null}
    }
}
```
##### (3) Advanced 进阶用法
```C#
    var datatable = ValueCopyer.ValueOf(new[] {
        new { Id = 1, Guid = Guid.NewGuid(), Name = "Dogwei" },
        new { Id = 2, Guid = Guid.NewGuid(), Name = "cxw" },
        new { Id = 3, Guid = Guid.NewGuid(), Name = "sg" },
        new { Id = 4, Guid = Guid.NewGuid(), Name = "eway" },
    }).ReadDataTable();
    
    var jsonFormatter = new JsonFormatter(JsonFormatterOptions.Indented);
    jsonFormatter.SetDataTableRWOptions(DataTableRWOptions.WriteToArrayFromBeginningSecondRows);
    jsonFormatter.SetValueFormat<Guid>("N");

    var json = jsonFormatter.Serialize(datatable);
    var dest = JsonFormatter.DeserializeObject<DataTable>(json);

    Console.WriteLine(json);
    /*
    [
      {
        "Guid": "1615527f673c499fac8de16847ad8783",
        "Id": 1,
        "Name": "Dogwei"
      },
      [
        "a23d1980185749118796fb5db7fb57a1",
        2,
        "cxw"
      ],
      [
        "9f76a802148d420da52716cf8a90b13d",
        3,
        "sg"
      ],
      [
        "ba03739cd44a49fab7b3de2558f84ebe",
        4,
        "eway"
      ]
    ]
    */
```
```C#
    var dic = new Dictionary<string, object>
    {
        { "Id", 123 },
        { "SystemNo", "9110" },
        { "IMEI", 31415926535897UL }
    };
    
    var jsonFormatter = new JsonFormatter();
    jsonFormatter.SetValueInterface(new MyUInt64Interface());

    var json = jsonFormatter.Serialize(dic);
    var obj = new { Id = 0, SystemNo = "", IMEI = 0UL, SIMId = 999 };

    jsonFormatter.DeserializeTo(json, RWHelper.CreateWriter(obj));

    Console.WriteLine(json); // {"Id":123,"SystemNo":"9110","IMEI":"0x1C92972436D9"}

    Console.WriteLine(obj.Id); // 123
    Console.WriteLine(obj.IMEI); // 31415926535897
    Console.WriteLine(obj.SIMId); // 999

    public class MyUInt64Interface : IValueInterface<ulong>
    {
        public unsafe ulong ReadValue(IValueReader valueReader)
        {
            var str = valueReader.ReadString();

            fixed (char* chars = str)
            {
                return NumberHelper.GetNumberInfo(chars, str.Length).ToUInt64(16);
            }
        }

        public void WriteValue(IValueWriter valueWriter, ulong value)
        {
            valueWriter.WriteString($"0x{value:X}");
        }
    }
```
##### (4) Use Swifter.Json on AspNetCore. 在 AspNetCore 上使用 Swifter.Json
```C#
First, reference the latest version of Swifter.Extensions.AspNetCore package on Nuget. And configure as follows.
首先在 Nuget 上引用最新的 Swifter.Extensions.AspNetCore 包，并如下配置。

    /** Configure */
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureJsonFormatter();
        }
    }
    
In this way, when the client use the application/json header request,
it will use Swifter.Json serialize results and deserialize parameters.
Or you can use the JsonResult to explicitly return Json content.
这样配置后，当客户端使用 application/json 头请求时，就会使用 Swifter.Json 序列化返回值或反序列化参数。
或者您可以使用 JsonResult 显式的返回 Json 内容。
```
