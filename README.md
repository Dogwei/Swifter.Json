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

### 文档： [Wiki](https://github.com/Dogwei/Swifter.Json/wiki/Swifter.Json)

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
