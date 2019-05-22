# Swifter.Json

A powerful, easy-to-use and fastest json serializer and deserializer on .Net platforms.

### Swifter.Json Features

(1) Supports the vast majority of data types on. Net. Includes dictionaries, collections, iterators, data readers, tables, and so on.

(2) Supports .Net Framework 2.0+, .NET Core 2.0+, .Net Standard 2.0+, Mono, Xamarin, Unity, and more platforms.

(3) It is almost Bug-free. If you ran into problems, you can post issue on Github, I'll fix them or help you.

### Why do I have to repeat made wheel?

At present, the JSON tools on .Net platforms all have some obvious shortcoming: some are easy to use but with low perfermence, some perfermence are high but not steady, some are steady but with low perfermence and is complicated.

We urgently need a powerful, stable, high-performance and easy-to-use JSON tool!

So Swifter.Json was produced.

### Benchmarks

The .Net Core 3.0 Preview running results:

![Run on the .Net Core 3.0 Preview.](https://github.com/Dogwei/Swifter.Json/blob/master/benchmark.png)

~~~
* The numbers in the chart represent time consumption.

* The third-party lib in the Test are from the latest official Version on Nuget.

* Later, on the .Net Core 2.1+ I will use Span<char> to optimize the parsing of Long string.
~~~

#### [Click here to view the .Net Framework 4.7.1 running results](https://github.com/Dogwei/Swifter.Json/blob/master/benckmark_for_framework_4.7.1.png)

### Demo

#### Esay to use:

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

#### Indent Json:

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

#### Ignore empty values:

##### Way one:

```C#
    // This way is configured through JsonFormatterOptions.
    // This way supports all data types, including class, dictionary, list, datatable, etc.
    public class Demo
    {
        public static void Main()
        {
            var jsonFormatter = new JsonFormatter(
                  JsonFormatterOptions.IgnoreNull
                | JsonFormatterOptions.IgnoreZero
                | JsonFormatterOptions.IgnoreEmptyString
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

            // By default, Arrays is not have turn on filter.
            Console.WriteLine(jsonFormatter.Serialize(list));
            // Output: [0,"",null,1,"Dogwei","System.Object"]
            
            // Turn on Arrays filter.
            jsonFormatter.Options |= JsonFormatterOptions.ArrayOnFilter;

            Console.WriteLine(jsonFormatter.Serialize(list));
            // [1,"Dogwei","System.Object"]
        }
    }
```

##### Way two:

```C#
    // This way is implemented through attributes and supports only entity class/struct.
    // This way ignores default values of all types, such as: default(int) = 0, default(string) = null.
    // This way ignores empty struct.
    // This way does not ignores empty string("").
    // This way does not ignores default values in the nested class/struct.
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

#### Set serialization formats:

```C#
    public class Demo
    {
        // RWFormat only supports members of types that implement the IFormattable interface.
        // The basic types and DateTime, decimal, and other system types all implement the IFormattable interface.
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

#### Set ignore case:

##### Way one:

```C#
    // This way has higher priority.
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

##### Way two:

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
            // This configuration set only at initialization, After FastObjectRW<T>.Create executes, the value cannot be modify.
            // This configuration takes effect on the specified type and its derived class.
            // But configure FastObjectRW<object>.CurrentOptions is not valid. because it always read the value of FastObjectRW.DefaultOptions.
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
### More demos will arrive soon.

#### Note: My English is not good enough but I will try my best.😄
