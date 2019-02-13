# Swifter.Json

这是迄今为止 .Net 平台功能最强大，性能最佳的 JSON 序列化和反序列化库。

#### 之所以说强大，因为这些功能其他框架没有！
	1): 支持深度复杂的对象结构且易于使用。
	2): 用 $ref 表示重复和循环引用的序列化和反序列化。
	3): 目前唯一支持 ref 属性的 JSON 库。 
	4): 支持几乎所有您常用的类型！并允许您自定义类型的行为。
	5): 支持 .Net Framework 2.0 +, .Net Core 2.0+, .Net Standard 2.0+, Mono, Xamarin, Unity。

#### 性能为何优异？
	1): 最优秀的整型和浮点型 ToString 和 Parse 方法实现。
	2): Emit 实现的高性能对象映射工具。
	3): 本地内存分配！拒绝 .Net 托管二次内存。
	4): 使用线程缓存，让您的程序运行越久速度越快。
	5): 内部全指针运算，相当于使用了 .Net Core 新技术 Span<T>！

#### Swifter.Json 实用功能
	1): 缩进美化 Json。
	2): 允许忽略 0 或 null 或 "" 值。
	3): 允许使用 [RWField] 特性定制属性或字段的行为。
	4): 允许设置最大深度来限制内容大小。

#### Swifter.Json 支持的类型
	bool, byte, sbyte, char, shoft, ushoft, int, uint, long, ulong,
	float, double, decimal, string, enum DateTime, DateTimeOffset,
	Guid, TimeSpan, DBNull, Nullable<T>, Version, Type,
	Array, Multidimensional-Arrays, IList, IList<T>, ICollection,
	ICollection<T>, IDictionary, IDictionary<TKey, TValue>,
	IEnumerable, IEnumerable<T>, DataTable, DbDataReader ...
	其余类型将会被当作 Object，以 属性键/属性值 的形式映射。

#### Swifter.Json 安全吗？
	每次发布之前我都会测试至少一个月，并且经过大量的测试，在实际项目中使用未发布的版本
	来确保发布版本的稳定性。但即使这样，我也无法保证它一定安全。所以，如果您发现了
	Bug 或某些不合理的地方请及时联系我 QQ:1287905882，邮箱 1287905882@qq.com。
	
#### 如何安装？

	```
	Nuget> Install-Package Swifter.Json -Version 1.1.2
	```

#### 性能测试对比

	*:  图标中的颜色随所用时间从 绿色 渐变为 黄色。当用时超过 3 倍时将以亮黄色显示。
			Timeout: 表示用时过久。
			Exception: 表示发生了异常。
			Error: 未发生异常，但结果不正确。

	*:	Swifter.Json 第一次执行需要额外的时间来生成一个 “操作类” (FastObjectRW<T>)，
		之后执行则会越来越快。所以如果您的程序需要长期运行，那么 Swifter.Json 是您优的选择。
		当然如果您的程序不适用这种模式，那么下面介绍的 XObjectRW<T> 也许适合您。

	*:	测试时其他库所使用的版本



#### 代码示例

	1): 简单使用

	```C#
    public class Demo
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public static void Main()
        {
            var json = JsonFormatter.SerializeObject(new { Id = 1, Name = "Dogwei" });
            var dictionary = JsonFormatter.DeserializeObject<Dictionary<string, object>>(json);
            var obj = JsonFormatter.DeserializeObject<Demo>(json);
        }
    }
	```

	2): 处理重复引用

	```C#
    public class Demo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Demo Instance { get; set; }

        public static void Main()
        {
            var jsonFormatter = new JsonFormatter(JsonFormatterOptions.MultiReferencingReference);

            var obj = new Demo() { Id = 1, Name = "Dogwei" };

            obj.Instance = obj;

            var json = jsonFormatter.Serialize(obj);

            var deser = jsonFormatter.Deserialize<Demo>(json);

            Console.WriteLine(json); // {"Id":1,"Instance":{"$ref":"#"},"Name":"Dogwei"}

            Console.WriteLine(deser.Instance == deser); // True
        }
    }
	```

	3): RWField 特性

	```C#
    public class Demo
    {
        [RWField("First Name")]
        public string Name { get; set; }

        [RWField]
        public int Age;

        [RWField(Access = RWFieldAccess.Ignore)]
        public int Sex { get; set; }
        [RWField(Order = 1)]
        public int Id { get; set; }

        public static void Main()
        {
            var obj = new Demo() { Id = 1, Name = "Dogwei", Age = 22, Sex = 1 };

            var json = JsonFormatter.SerializeObject(obj);

            Console.WriteLine(json); // {"Id":1,"Age":22,"First Name":"Dogwei"}
        }
    }
	```

	4): 设置日期格式

	```C#
    public class Demo
    {
        public static void Main()
        {
            var jsonFormatter = new JsonFormatter();

            jsonFormatter.SetDateTimeFormat("yyyy-MM-dd HH:mm:ss");

            var json = jsonFormatter.Serialize(DateTime.Now);

            Console.WriteLine(json); // "2019-02-13 11:03:46"
        }
    }
	```

	5): 自定义类型的行为

	```C#
    public class Demo
    {
        public string Name { get; set; }

        public int Sex { get; set; }

        public bool IsMan { get => Sex == 1; }

        public unsafe static void Main()
        {
            var jsonFormatter = new JsonFormatter();
            
            jsonFormatter.SetValueInterface<bool>(new MyBooleanInterface());

            var obj = new Demo() { Name = "Dogwei", Sex = 1 };

            var json = jsonFormatter.Serialize(obj);

            Console.WriteLine(json); // {"IsMan":"yes","Name":"Dogwei","Sex":1}
        }
    }

    public class MyBooleanInterface : IValueInterface<bool>
    {
        public bool ReadValue(IValueReader valueReader)
        {
            var value = valueReader.ReadString();

            switch (value)
            {
                case "yes":
                case "true":
                    return true;
                case "no":
                case "false":
                    return false;
                default:
                    return Convert.ToBoolean(value);
            }
        }

        public void WriteValue(IValueWriter valueWriter, bool value)
        {
            valueWriter.WriteString(value ? "yes" : "no");
        }
    }
	```

	6): 设置缓存大小

	```C#
    public class Demo
    {
        public static void Main()
        {
            HGlobalCache.MaxSize = 1024 * 500; // 500KB

            var json = JsonFormatter.SerializeObject(new { MaxJsonLength = 256000 });
        }
    }
	```

	7): 序列化超大文件

	```C#
    public class Demo
    {
        public static void Main()
        {
            var bigObject = new BigObject();

            using (FileStream fileStream = new FileStream("/BigObject.json", FileMode.Create, FileAccess.ReadWrite))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    JsonFormatter.SerializeObject(bigObject, streamWriter);
                }
            }
        }
    }
	```

	8): 使用适用小型应用程序的 XObjectRW<T>

	```C#
    public class Demo
    {
        public static void Main()
        {
            // Default (FastObjectInterface)    : 初始化开销较大，内存较大，性能优异。
            // XObjectInterface                 : 初始化开销小，内存占用少，性能也不错。

            ValueInterface.DefaultObjectInterfaceType = typeof(XObjectInterface<>);

            var json = JsonFormatter.SerializeObject(new { Id = 1, Name = "Dogwei" });

            Console.WriteLine(json); // {"Id":1,"Name":"Dogwei"}
        }
    }
	```


# Swifter.Core

Swifter.Core 有助于您解除语言限制，编写您最优秀的 .Net 程序。

#### Swifter.Core 实现的旗舰功能：
	1): 几乎所有常用类型的超高性能对象映射工具。
	2): 效率超高数学算法！超 .Net 自带算法 10+ 倍。
	3): 开放的委托接口！创建您最实用，性能最好的委托吧。
	4): 极致性能的缓存工具。线程安全的 亿/秒 读取性能，比线程不安全的 Dictionary 还要快两倍！
	5): 开放指针工具！允许您获取对象的地址，字段偏移量，类型的大小等底层信息。
	6): 高性能的类型转换工具 XConvert！允许您将任意类型转换为任意类型，只要它们本身支持转换。
	7): 解决 .Net20 到 .Net471 的版本兼容问题。 引用 Swifter.Core 允许您在低版本中使用 元组，dynamic，LINQ 等。


#### 已支持映射的对象或值类型有
	bool, byte, sbyte, char, shoft, ushoft, int, uint, long, ulong,
	float, double, decimal, string, enum DateTime, DateTimeOffset,
	Guid, TimeSpan, DBNull, Nullable<T>, Version, Type,
	Array, Multidimensional-Arrays, IList, IList<T>, ICollection,
	ICollection<T>, IDictionary, IDictionary<TKey, TValue>,
	IEnumerable, IEnumerable<T>, DataTable, DbDataReader ...
	其余类型将会被当作 Object，以 属性键/属性值 的形式映射。

#### 高效的数学算法
	1): 大数字加减乘除算法
	2): 整型和浮点型 2-64 进制 ToString 和 Parse 算法
	3): Guid 和 时间的 ToString 和 Parse 算法。

#### 创新技术
	1): Difference-Switch 字符串匹配算法，比 Hash 匹配快 3 倍！
	2): 支持 ref 属性！现在允许您在实体中定义 ref 属性降低程序内存啦。
	3): 内部三种实现创建委托，支持创建 99.9% 方法的委托！（仅当 TypedReference* 作为参数类型的方法不支持。）

#### 数学算法性能对比

#### 对象映射简单性能对比

#### 缓存性能对比

#### 委托动态执行的性能对比


#### 高端玩法
	1): 将一个对象的复制到字典中：

	反之亦然。


	2): 将一个对象转为结构地址，并设置它的私有字段的值：

	3): 将一个数字转换为 52 进制的字符串：