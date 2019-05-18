# Swifter.Json

### 1.2.2 更新:

#### 1：增加异步方法。
#### 2：修改 AspNetCore 使用异步方法。
#### 3：型新版本需要一些测试，所以暂不发布到 Nuget。

### 1.2.1 更新:

#### 1: 再度提高性能 (主要原理是对不常见行为禁止内联，提高常见行为的内联成功率)。
#### 2: 解决枚举序列化出错，ValueInterface&lt;T&gt;.SetInterface() 不起作用等 BUG。
#### 3: 增加特性定义 (反)序列化行为 ([RWFormat], [RWField], [RWObject] 等特性)。
#### 4: 增加 AspNetCore 的扩展方法 ConfigureJsonFormatter(this IServiceCollection services)。现在可以很方便将 Swifter.Json 配置到 MVC 了。

### 效率评测图

![评测用时图](https://github.com/Dogwei/Swifter.Json/blob/master/benchmark.png)

~~~
* 此次测试运行在 .Net Core 3.0 预览版上，并增加了 SpanJson 库。
* 测试中的第三方库均来自 Nuget 上最新正式版本。
* 这次评测让我深知 Span<T> 的硬件加速的性能，我会考虑对 .Net Core 2.1+ 进行特殊处理，提高长字符串解析的性能。
~~~

### [点此查看 .Net Framework 4.7.1 评测结果图](https://github.com/Dogwei/Swifter.Json/blob/master/benckmark_for_framework_4.7.1.png)

#### Swifter.Json 仍然支持 .Net Framework 2.0+, .Net Core 2.0+, .Net Standard 2.0+, Mono, Xamarin, Unity 等平台。
#### Swifter.Json 支持 .Net 上绝大多数的数据类型。包括字典，集合，迭代器，数据读取器，表格等等。
#### 建议在 Nuget 包管理上下载最新的 Swifter.Json 库 (最新版本 1.2.1)。

#### 虽然此前一直强调 Swifter.Json 的性能，但其实它的可扩展性和代码重用性才是可圈可点的。为了这些， Swifter.Json 在性能上其实做了很大让步！
#### 之前决定针对 .Net Core 3.0 使用 Avx2 指令优化，但并没有提升效果，可能是我打开方式不对，所以暂没有此类优化。

### 示例

#### 简单使用
```C#
    public class Demo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public static void Main()
        {
            var json = JsonFormatter.SerializeObject(new { Id = 1, Name = "Dogwei" });
            var dic = JsonFormatter.DeserializeObject<Dictionary<string, object>>(json);
            var obj = JsonFormatter.DeserializeObject<Demo>(json);;
        }
    }
```
#### Json 缩进
```C#
    public class Demo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public static void Main()
        {
            var jsonFormatter = new JsonFormatter(JsonFormatterOptions.Indented);

            var json = jsonFormatter.Serialize(new Demo { Id = 1, Name = "Dogwei" });
            var obj = jsonFormatter.Deserialize<Demo>(json);

            Console.WriteLine(json);

            /*
             * Output:
             * {
             *   "Id": 1,
             *   "Name": "Dogwei"
             * }
            **/
        }
    }
```
#### 忽略 Null 值
##### 方式 1:
```C#
	// 此方式通过 JsonFormatterOptions 配置。
	// 此方式支持所有结构类型，包括 class, dictionary, list, datatable 等...
    public class Demo
    {
        public static void Main()
        {
            var jsonFormatter = new JsonFormatter(
                  JsonFormatterOptions.IgnoreNull // 忽略 Null 值
                | JsonFormatterOptions.IgnoreZero // 忽略基础类型的 0 值
                | JsonFormatterOptions.IgnoreEmptyString // 忽略字符串 "" 值
                );

            var dic = new Dictionary<string, object>
            {
                { "TestZero", 0 },
                { "TestEmptyString", "" },
                { "TestNull", null },
                { "TestNonZero", 1 },
                { "TestNonEmptyString", "Dogwei" },
                { "TestNonNull", new object() }
            };

            Console.WriteLine(jsonFormatter.Serialize(dic));
            // Output: {"TestNonZero":1,"TestNonEmptyString":"Dogwei","TestNonNull":"System.Object"}

            var list = dic.Values.ToList();

            // 考虑到数组索引等因素，数组默认不启动筛选。
            Console.WriteLine(jsonFormatter.Serialize(list));
            // Output: [0,"",null,1,"Dogwei","System.Object"]


            jsonFormatter.Options |= JsonFormatterOptions.ArrayOnFilter; // 数组启动筛选

            Console.WriteLine(jsonFormatter.Serialize(list));
            // [1,"Dogwei","System.Object"]
        }
    }
```
##### 方式 2:
```C#
    // 此方式通过特性实现，只支持实体类或结构 (Entity class/struct)。
    // 此配置会忽略一个类型所有的 default 值, 比如: default(int) = 0, default(string) = null。
    // 注意：此方式会忽略空结构 (empty struct)。
    // 注意：此方式不会忽略空字符串 (EmptyString: "")。
    // 注意：此方式不会忽略嵌套的对象或结构中的 default 值。
    [RWObject(SkipDefaultValue = RWBoolean.Yes)]
    public class Demo
    {
        public int TestZero { get; set; } = 0;
        public int TestNonZero { get; set; } = 1;

        public string TestEmptyString { get; set; } = "";
        public string TestNonEmptyString { get; set; } = "Dogwei";

        public object TestNull { get; set; } = null;
        public object TestNonNull { get; set; } = new object();

        public TestStruct TestEmptyStruct { get; set; }
        public TestStruct TestNonEmptyStruct { get; set; } = new TestStruct() { TypeCode = TypeCode.Int64, TypeName = typeof(long).Name };

        public static void Main()
        {
            var obj = new Demo();

            var json = JsonFormatter.SerializeObject(obj, JsonFormatterOptions.Indented);

            Console.WriteLine(json);
            /*
             * Output: 
             * {
             *   "TestEmptyString": "",
             *   "TestNonEmptyString": "Dogwei",
             *   "TestNonEmptyStruct": {
             *     "TestNull": null,
             *     "TypeCode": "Int64",
             *     "TypeName": "Int64"
             *   },
             *   "TestNonNull": "System.Object",
             *   "TestNonZero": 1
             * }
            **/
        }

        public struct TestStruct
        {
            public TypeCode TypeCode { get; set; }

            public string TypeName { get; set; }

            public object TestNull { get; set; }
        }
    }
```
#### 设置序列化格式
```C#
    public class Demo
    {
        // RWFormat 只支持实现了 IFormattable 接口的类型。
        // 基础类型及 DateTime, decimal, 等系统类型都实现了 IFormattable 接口。
        [RWFormat("000")]
        public int Id { get; set; } = 1;

        public string Name { get; set; } = "Dogwei";

        [RWFormat("yyyy-MM-dd HH:mm:ss")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        public static void Main()
        {
            var obj = new Demo();

            var json = JsonFormatter.SerializeObject(obj, JsonFormatterOptions.Indented);

            obj = JsonFormatter.DeserializeObject<Demo>(json);

            Console.WriteLine(json);
            /*
             * Output: 
             * {
             *   "CreateTime": "2019-05-14 10:51:13",
             *   "Id": "001",
             *   "Name": "Dogwei"
             * }
            **/
        }
    }
```
#### 处理重复引用
```C#
    public class Demo
    {
        public int HashCode => GetHashCode();

        public object Parent { get; set; }

        public object Me => this;
        
        public object Child { get; set; }

        public static void Main()
        {
            var obj = new Demo();
            var chi = new Demo();

            obj.Child = chi;
            chi.Parent = obj;

            // 处理方式一：将循环引用的对象设为 Null。
            var jsonFormatter = new JsonFormatter(JsonFormatterOptions.LoopReferencingNull | JsonFormatterOptions.Indented);

            Console.WriteLine(jsonFormatter.Serialize(obj));
            /*
             * {
             *   "Child": {
             *     "Child": null,
             *     "HashCode": 30015890,
             *     "Me": null,
             *     "Parent": null
             *   },
             *   "HashCode": 55530882,
             *   "Me": null,
             *   "Parent": null
             * }
            **/

            // 配合忽略 Null 使用将会忽略循环引用的字段。
            jsonFormatter.Options |= JsonFormatterOptions.IgnoreNull;

            Console.WriteLine(jsonFormatter.Serialize(obj));
            /*
             * {
             *   "Child": {
             *     "HashCode": 30015890,
             *   },
             *   "HashCode": 55530882
             * }
            **/

            // 处理方式二：将重复引用以 $ref 形式序列化。
            // 此方式符合 Json 格式，但不是 Json 标准。
            // 许多 Json 编辑工具都承认 $ref 格式，如 VS。
            // 注意：此方式可能无法在浏览器中处理。
            jsonFormatter = new JsonFormatter(JsonFormatterOptions.MultiReferencingReference | JsonFormatterOptions.Indented);

            Console.WriteLine(jsonFormatter.Serialize(obj));
            /*
             * {
             *   "Child": {
             *     "Child": null,
             *     "HashCode": 30015890,
             *     "Me": { "$ref": "#/Child" },
             *     "Parent": { "$ref": "#" }
             *   },
             *   "HashCode": 55530882,
             *   "Me": { "$ref": "#" },
             *   "Parent": null
             * }
            **/

            // 处理方式三：设置最大结构深度。

            jsonFormatter = new JsonFormatter(JsonFormatterOptions.Indented | JsonFormatterOptions.IgnoreNull);

            jsonFormatter.MaxDepth = 2;
            
            Console.WriteLine(jsonFormatter.Serialize(obj));
            /*
             * {
             *   "Child": {
             *     "HashCode": 30015890,
             *     "Me": {
             *     },
             *     "Parent": {
             *     }
             *   },
             *   "HashCode": 55530882,
             *   "Me": {
             *     "Child": {
             *     },
             *     "HashCode": 55530882,
             *     "Me": {
             *     }
             *   }
             * }
            **/

            // 处理方式四：抛出异常

            jsonFormatter = new JsonFormatter(JsonFormatterOptions.LoopReferencingException);

            Console.WriteLine(jsonFormatter.Serialize(obj));
            // JsonLoopReferencingException: Json serializating members '#/Child' and '#/Child/Me' loop referencing.
        }
    }
```
#### 自定义类型的行为
##### 方式一：
```C#
	public class Demo
    {
        // 特性方式只会对指定的成员生效。
        [RWField(InterfaceType = typeof(LongIdInterface))]
        public LongId Id { get; set; } = new LongId() { Value = 999 };

        public string Name { get; set; } = "Dogwei";

        public static void Main()
        {
            var obj = new Demo();

            var json = JsonFormatter.SerializeObject(obj);

            obj = JsonFormatter.DeserializeObject<Demo>(json);

            Console.WriteLine(json);
            // {"Id":"3e7","Name":"Dogwei"}
        }

        public class LongId
        {
            public long Value { get; set; }
        }

        public class LongIdInterface : IValueInterface<LongId>
        {
            public LongId ReadValue(IValueReader valueReader)
            {
                return new LongId() { Value = NumberHelper.InstanceByRadix(16).ParseInt64(valueReader.ReadString()) };
            }

            public void WriteValue(IValueWriter valueWriter, LongId value)
            {
                valueWriter.WriteString(NumberHelper.InstanceByRadix(16).ToString(value.Value));
            }
        }
    }
```
##### 方式二：
```C#
    public class Demo
    {
        public LongId Id { get; set; } = new LongId() { Value = 999 };

        public string Name { get; set; } = "Dogwei";

        public static void Main()
        {
            var obj = new Demo();

            var jsonFormatter = new JsonFormatter();

            // 此方式只会对 jsonFormatter 这个实例生效。
            // 用 jsonFormatter 此实例序列化或反序列化的所有 LongId 值都会生效。
            jsonFormatter.SetValueInterface(new LongIdInterface());

            var json = jsonFormatter.Serialize(obj);

            obj = jsonFormatter.Deserialize<Demo>(json);

            Console.WriteLine(json);
            // {"Id":"3e7","Name":"Dogwei"}
        }

        public class LongId
        {
            public long Value { get; set; }
        }

        public class LongIdInterface : IValueInterface<LongId>
        {
            public LongId ReadValue(IValueReader valueReader)
            {
                return new LongId() { Value = NumberHelper.InstanceByRadix(16).ParseInt64(valueReader.ReadString()) };
            }

            public void WriteValue(IValueWriter valueWriter, LongId value)
            {
                valueWriter.WriteString(NumberHelper.InstanceByRadix(16).ToString(value.Value));
            }
        }
    }
```
##### 方式三：
```C#
    public class Demo
    {
        public LongId Id { get; set; } = new LongId() { Value = 999 };

        public string Name { get; set; } = "Dogwei";

        public static void Main()
        {
            var obj = new Demo();

            // 此方式全局生效。
            // 只要类型是 LongId 的值都会走 LongIdInterface。
            ValueInterface<LongId>.SetInterface(new LongIdInterface());

            var json = JsonFormatter.SerializeObject(obj);

            obj = JsonFormatter.DeserializeObject<Demo>(json);

            Console.WriteLine(json);
            // {"Id":"3e7","Name":"Dogwei"}
        }

        public class LongId
        {
            public long Value { get; set; }
        }

        public class LongIdInterface : IValueInterface<LongId>
        {
            public LongId ReadValue(IValueReader valueReader)
            {
                return new LongId() { Value = NumberHelper.InstanceByRadix(16).ParseInt64(valueReader.ReadString()) };
            }

            public void WriteValue(IValueWriter valueWriter, LongId value)
            {
                valueWriter.WriteString(NumberHelper.InstanceByRadix(16).ToString(value.Value));
            }
        }
    }
```
#### 反序列化为 Dynamic
```C#
    public class Demo
    {
        public static void Main()
        {
            var list = new List<Dictionary<string, object>>
            {
                { new Dictionary<string, object>() { { "Id", 1},{ "Name", "Dogwei"} }},
                { new Dictionary<string, object>() { { "Id", 2},{ "Name", "sg"} }},
                { new Dictionary<string, object>() { { "Id", 3},{ "Name", "cxw"} }},
                { new Dictionary<string, object>() { { "Id", 4},{ "Name", "eway"} }},
            };

            var json = JsonFormatter.SerializeObject(list);

            dynamic dym = JsonFormatter.DeserializeObject<JsonValue>(json);

            Console.WriteLine(dym[0].Name); // Dogwei
            Console.WriteLine(dym[1].Name); // sg
            Console.WriteLine(dym[2].Id); // 3
            Console.WriteLine(dym[3].Id); // 4
        }
    }
```
#### 设置忽略大小写
##### 方式一：
```C#
    // 特性方式优先级更高一点。
    [RWObject(IgnoreCace = RWBoolean.Yes)]
    public class Demo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public static void Main()
        {
            var dic = new Dictionary<string, object>
            {
                { "id", 1},
                { "name", "Dogwei" }
            };

            var json = JsonFormatter.SerializeObject(dic);

            var obj = JsonFormatter.DeserializeObject<Demo>(json);

            Console.WriteLine(JsonFormatter.SerializeObject(obj));
            // {"Id":1,"Name":"Dogwei"}
        }
    }
```
##### 方式二：
```C#
	public class BaseDemo
    {

    }

    public class Demo : BaseDemo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public static void Main()
        {
            // 此配置必须在初始化时设置，在 FastObjectRW<T>.Create 执行之后将无法修改此值。
            // 此配置会对指定的类型和其派生类生效
            // 但指定 FastObjectRW<object> 的配置是无效的，因为它始终读取 FastObjectRW.DefaultOptions 的值。
            FastObjectRW<BaseDemo>.CurrentOptions |= FastObjectRWOptions.IgnoreCase;

            var dic = new Dictionary<string, object>
            {
                { "id", 1},
                { "name", "Dogwei" }
            };

            var json = JsonFormatter.SerializeObject(dic);

            var obj = JsonFormatter.DeserializeObject<Demo>(json);

            Console.WriteLine(JsonFormatter.SerializeObject(obj));
            // {"Id":1,"Name":"Dogwei"}
		}
	}
```
#### 设置异常行为
##### 一：设置当属性不含 get/set 方法时读取或设置该属性不引发异常。
```C#
    public class Demo
    {
        public int Id { get; }

        public string Name { get; }

        public static void Main()
        {
            // 默认情况下，当属性不含 get/set 方法时读取或设置该属性将引发 MemberAccessException 异常。
            // 此配置设置此情况不引发异常而是跳过设置或取得 Null 值。
			// 注意：[RWObject] 特性同样可以设置此配置。
            FastObjectRW.DefaultOptions &= ~FastObjectRWOptions.CannotSetException;

            var json = JsonFormatter.SerializeObject(new { Id = 1, Name = "Dogwei" });
            
            var obj = JsonFormatter.DeserializeObject<Demo>(json);

            json = JsonFormatter.SerializeObject(obj);

            Console.WriteLine(json);
            // {"Id":0,"Name":null}
        }
    }
```
##### 二：设置当属性不含 get/set 方法时读取或设置该属性不引发异常。
```C#
    public class Demo
    {
        public static void Main()
        {
            // 默认情况下，当读取或设置一个不存在的属性时将引发 MissingMemberException 异常。
            // 此配置设置此情况不引发异常而是跳过设置或取得 Null 值。
			// 注意：[RWObject] 特性同样可以设置此配置。
            FastObjectRW.DefaultOptions &= ~FastObjectRWOptions.NotFoundException;

            var json = JsonFormatter.SerializeObject(new { Id = 1, Name = "Dogwei" });
            
            var obj = JsonFormatter.DeserializeObject<Demo>(json);

            json = JsonFormatter.SerializeObject(obj);

            Console.WriteLine(json);
            // {}
        }
    }
```
##### 三：设置 Json 超出最大深度异常。
```C#
	public class Demo
    {
        public object Me => this;

        public static void Main()
        {
            // 默认情况下，当 Json 结构超出 MaxDepth 定义的深度时不引发异常，且不序列化超出部分。
            // 此配置设置此情况将引发异常。
            var jsonFormatter = new JsonFormatter(JsonFormatterOptions.OutOfDepthException);

            var obj = new Demo();
            
            var json = jsonFormatter.Serialize(obj);
            // JsonOutOfDepthException: Json struct depth out of the max depth.
        }
    }
```
#### 设置 XObjectRW<T> 为对象读写器。
```C#
    // XObjectRW 同样支持通过特性定制行为。
    [RWObject(IgnoreCace = RWBoolean.Yes)]
    public class Demo
    {
        [RWFormat("000")]
        public int Id { get; set; }

        public string Name { get; set; }

        public static void Main()
        {
            // 默认情况下，ValueInterface 的默认对象接口类型是 FastObjectInterface<>。
            // 若想改为 XObjectInterface<> 可以如下设置。
            // 首先引入 Nuget 包 Swifter.Reflection。
            // 然后在程序初始化时执行此句就可以了。
            ValueInterface.DefaultObjectInterfaceType = typeof(XObjectInterface<>);
            // XObjectRW 和 FastObjectRW 使用起来并无区别。
            // XObjectRW 是纯 C# 代码 + 反射 + 委托实现。功能更强大并在少次使用时内存占用较少，适合客户端使用。
            // FastObjectRW 是几乎是纯 IL 生成，性能极高，首次使用初始化耗时较长，但内存占用也不是很大，非常适合服务端使用。
            // 其实在客户端使用 FastObjectRW 也没有任何毛病，即使您的客户端有成千上万个实体类，FastObjectRW 内存占用也不会超过 300MB。
            // 相比 Newtonsoft，不管是 FastObjectRW 还是 XObjectRW，它们的内存占用都要小得多！性能也要快得多！

            var obj = new Demo { Id = 1, Name = "Dogwei" };

            var json = JsonFormatter.SerializeObject(obj);

            obj = JsonFormatter.DeserializeObject<Demo>(json);

            Console.WriteLine(json);
            // {"Id":"001","Name":"Dogwei"}
        }
    }
```
#### 使用 RWHelper 进行数据复制。
```C#
    public class Demo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public static void Main()
        {
            var dic = new Dictionary<string, string>
            {
                { "Id", "1" },
                { "Name", "Dogwei" }
            };

            var dicReader = RWHelper.CreateReader(dic).As<string>();
            var objWriter = RWHelper.CreateWriter<Demo>();

            // objWriter.Content = new Demo();
            objWriter.Initialize();

            // objWriter.Content.Id = dicReader["Id"].ReadInt32();
            // objWriter.Content.Name = dicReader["Name"].ReadString();
            RWHelper.Copy(dicReader, objWriter);

            Demo obj = RWHelper.GetContent<Demo>(objWriter);

            Console.WriteLine(JsonFormatter.SerializeObject(obj));
            // {"Id":1,"Name":"Dogwei"}

            // 反之亦然

            var objReader = RWHelper.CreateReader(obj);
            var dicWriter = RWHelper.CreateWriter(typeof(Dictionary<string, object>));

            dicWriter.Initialize();

            RWHelper.Copy(objReader, dicWriter);

            Console.WriteLine(JsonFormatter.SerializeObject(RWHelper.GetContent<object>(dicWriter)));
            // {"Id":1,"Name":"Dogwei"}
        }
    }
```
#### 将 Swifter.Json 配置到 AspNetCore。
```C#

    // 从 Nuget 上引入最新版的 Swifter.Extensions.AspNetCore 包。
    // 然后在 Startup 中的 ConfigureServices 方法里加入如下代码。

    services.ConfigureJsonFormatter();


    // 类似这样：

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            ...

            services.ConfigureJsonFormatter();
        }
    }
```