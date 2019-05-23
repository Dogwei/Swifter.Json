# Swifter.Json

### [Github](https://github.com/Dogwei/Swifter.Json)

### [Wiki](https://github.com/Dogwei/Swifter.Json/wiki/Swifter.Json)

#### 在 .Net 平台上的一个功能强大，简单易用，稳定又不失高性能的 JSON 序列化和反序列化工具。

#### Swifter.Json 已经经过了大量测试和线上项目中运行许久来确保它的稳定性。

## 特性

#### 1: 支持 .Net 上绝大多是的数据类型，且轻松扩展；包括但不限于：实体，字典，集合，迭代器，数据读取器和表格。

#### 2: 支持 .Net 我已知的大多数平台，包括但不限于：Framework2.0+, Core 2.0+, Standard 2.0+, Mono, Xamarin, Unity(测试版本为 2018.3).

#### 3: 它几乎是无 BUG 的，如果您遇到了问题，可以在 Github 上发布一个 issue，或者 QQ:1287905882，我会尽力帮助您。

#### 4：所有公开成员都有中文说明，中文语言人的福音😄；将来可能添加英文说明。

## 缺点

#### 1: 暂没有英文接口说明，但成员命名是英文的。

#### 2：总共有三个 DLL 文件，Swifter.Core(278KB)(这是 Swifter 的核心库，我不希望它与 Json 挂钩，而是它作为一个巨人，为类库开发者提供很多帮助)，Swifter.Unsafe(10KB)(这是用 IL 生成的 DLL，用于指针操作；并不是不安全的)，Swifter.Json(52KB)(Swifter 的 Json 解析部分)；文件不大也不小。
##### ：在 Standard 下还需要 System.Reflection.Emit 和 System.Reflection.Emit.Lightweight 库。

#### 3：在 Standard 和 Framework 3.5 及更低版本，Swifter.Json 可能性能略减，因为我不敢在这些版本上使用针对性优化，这些版本缺少一些接口，并且可能会在一个未知的平台上运行（如 Unity 和 Xamarin）。

## 部分 .Net 现有的 JSON 工具特性对比

![Feature Comparison](Features.png)

###### 平台兼容性     ✓：兼容大多数平台的大多数版本；乄：兼容部分平台，且版本要求较高；✗：只能在单一平台上运行。
###### 稳定性         ✓：在大多数测试中未出现 BUG；乄：一些不常见操作会出现 BUG；✗：常见操作会出现 BUG。
###### 功能性         ✓：支持大多数的数据类型和方法；乄：支持常用的数据类型和方法；✗：部分常用数据类型和方法不支持。
###### 扩展性         ✓：高度允许自定义格式和处理方式；乄：支持常用的格式设置；✗：不能自定义格式。
###### 高性能         ✓：相比 Newtonsoft 平均快 4x 以上；乄：相比 Newtonsoft 平均快 2x 以上；✗：相比 Newtonsoft 差不多或者更慢。
###### 小分配(内存)   ✓：执行过程中分配的内存极少；乄：必要的内存占用较少；✗：执行过程中分配的大量的临时内存。
###### 大小(文件)     ✓：小于 100KB；乄：大于 100KB 小于 500 KB；✗：大于 500 KB。

## 部分 .Net 现有的 JSON 工具性能对比

#### .Net Core 3.0 Previews running results.

![.Net Core 3.0 Previews running results](benchmark.png)

#### .Net Framework 4.7.1 Previews running results.

![.Net Framework 4.7.1 Previews running results](benckmark_for_framework_4.7.1.png)

##### 图中的数字代表用时(ms). 表格颜色随用时从 绿色 渐变为 黄色。当用时超过 3 倍时将以亮黄色显示。
##### Swifter.Json 第一次执行需要额外的时间来生成一个 “操作类(FastObjectRW&lt;T&gt;)” 后续会越来越快。所以如果您的程序需要长期运行，那么 Swifter.Json 是您优的选择。如果您的程序不适用这种模式，那么下面介绍的 XObjectRW&lt;T&gt; 也许适合您。

## Swifter.Json 的工作原理

#### 以下的实体类作为例子，解释 Swifter.Json 序列化的全过程。

```C#

public class Demo
{
    public int Id { get; set; }

    public string Name { get; set; }
}

```

#### 当执行以下操作时，

```C#

JsonFormatter.SerializeObject(new Demo { Id = 1, Name = "Dogwei" });

```

#### Swifter.Json 首先会创建一个 JsonSerializer 实例(此实例是一个 internal class)，此类实现了 Swifter.RW.IValueWriter 接口。

#### 然后 Swifter.Json 会执行 Swifter.RW.ValueInterface&lt;Demo&gt;.WriteValue(jsonSerializer, demo); 操作。

#### 在 ValueInterface&lt;Demo&gt;.WriteValue 里会匹配 Demo 的 IValueInterface&lt;Demo&gt; 的实现类；默认情况下，它会匹配到 Swifter.RW.FastObjectInterface&lt;T&gt; 这个实现类。

#### 赋予泛型参数，然后执行 FastObjectInterface&lt;Demo&gt; 的 WriteValue(IValueWriter valueWriter, Demo value) 方法。

#### 在该方法里，它首先检查了 value 是否为 Null，如果是则执行 valueWriter.DirectWrite(null); 方法，表示在 JsonSerializer 写入一个 Null，然后返回。

#### 然后检查 value 的引用，是否为 “父类引用，子类实例” 的对象，如果是则重新匹配子类的 IValueInterface&lt;T&gt; 实现类。

#### 之后是：执行 var fastObjectRW = FastObjectRW&lt;Demo&gt;.Create();，创建一个数据读写器，它实现了 IDataReader&lt;string&gt; 和 IDataWriter&lt;string&gt; 接口。

#### 然后初始化数据读写器：fastObjectRW.Initialize(value);，这相当于把数据读写器中的 Demo 上下文 (Context) 设置为 value。

#### 再调用 valueWriter.WriterObject(IDataReader&lt;string&gt; dataReader); 方法。这就回到了 JsonSerializer 的 WriterObject 方法里。

#### 在该方法里，首先直接写入了一个 '{' 字符。

#### 然后执行 dataReader.OnReadAll(this);，OnReadAll 是用 IL 生成一个方法，它会遍历 Demo 中所有公开属性的名称和值写入到 IDataWriter&lt;string&gt; 里。
###### 这里补充：JsonSerializer 除了实现了 IValueWriter 接口外，还实现了，IDataWriter&lt;string&gt; 和 IDataWriter&lt;int&gt;，这两个是写入 “对象” 和 写入 “数组” 的接口。

#### 在 OnReadAll 里会执行两个操作：dataWriter["Id"].WriteInt32(Context.Id); dataWriter["Name"].WriteString(Context.Name);。

#### dataWriter["Id"] 操作会写入一个 '"Id":' 的 JSON 字符串；然后返回 IValueWriter 实例，因为 JsonSerializer 本身就是 IValueWriter 的实现类，所以返回它本身。

#### 在 WriteInt32 里，JsonSerializer 器会执行 offset += Swifter.Tools.NumberHelper.Decimal.ToString(value, hGBuffer.GetCharPointer() + offset); Append(',');
###### 补充：hGBuffer 是一个本地内存的缓存，是一个非托管内存，它必须要释放，Swifter.Json 将释放放在 try {} finally {} 里，以确保在任何情况下都会释放。
###### offset 表示当前 Json 字符串的写入位置。
###### NumberHelper 是一个高性能低分配的数字算法，主要包括 浮点数和整形的 ToString 算法 和 Parse 算法，它支持 2-64 进制；Decimal 表示十进制。

#### 在 WriteString里 JsonSerializer 器会执行 Append('"'); InternalWriteString(value); Append('"'); Append(',');
#### 在 InternalWriteString 里，JsonSerializer 会根据字符串的长度选择两种写入方式；
#### 第一种方式是扩容字符串两倍的内存空间，然后将字符串全部写入，以确保字符串在包含转义字符时能够完整写入，此方式性能更好。
#### 第二种方式是扩容字符串等量的内存空间，然后逐个字符写入，当内存满的时候再次扩容，直至字符串全部写入。
#### 当字符串长度大于 300 时选用第二种方式，否则选用第一种方式。
###### 这两种方式是参考了其他 JSON 开源库之后最终采用我认为最好的方式，性能对比对应 ShortString 和 LongString 的 ser 测试。

#### 完了之后会返回到 JsonSerializer 的 WriterObject 里，该方法会去掉最后一个 ',' 字符，然后拼上 '}' 字符，然后再拼上 ','。

#### 然后返回到 JsonFormatter 的 SerializeObject 里，该方法会执行 new string(hGBuffer.GetCharPointer(), 0, jsonSerializer.offset - 1); 获取该 JSON 字符串。然后释放 JsonSerializer 器。最后再返回给调用者
###### 释放 JsonSerializer 时，它会一起将 hGBuffer 也释放了。

#### 至此，JSON 序列化工作就完成了。

## 以下解释 Swifter.Json 的反序列化过程。还是那个 Demo 类。

```C#

// 现在我们得到一个 JSON 字符串。
var json = "{\"Id\":1,\"Name\":\"Dogwei\"}";

```

#### 执行如下操作：

```C#

JsonFormatter.DeserializeObject<Demo>(json);

```

#### Swifter.Json 首先会 fixed json 取得 json 的内存地址 pJson；然会执行 var jsonDeserializer = new JsonDeserializer(pJson, 0, json.Length) 创建解析器实例，此类实现了 IValueReader 接口。

#### 然后 Swifter.Json 会执行 ValueInterface&lt;Demo&gt;.ReadValue(jsonDeserializer); 操作。

#### 在 ValueInterface&lt;Demo&gt;.ReadValue 里也是会匹配 Demo 的 IValueInterface&lt;T&gt; 的实现类；它还是会匹配到 FastObjectInterface&lt;Demo&gt; 这个类。

#### 然后执行 FastObjectInterface&lt;Demo&gt; 的 ReadValue(IValueReader valueReader) 方法。

#### 在该方法里，它进行没有任何判断，直接创建了一个 FastObjectRW&lt;Demo&gt;；因为这里是第二次创建，所以马上就能创建好。

#### 然后执行 valueReader.ReadObject(IDataWriter&lt;string&gt; dataWriter); 方法。现在回到 JsonDeserializer 的 ReadObject 方法里。

#### 在该方法里，首先判断 JsonValueType 是否等于 Object。如果不是则调用一个 NoObject 方法。

#### 在 NoObject 方法里，如果 JsonValueType 是 String，Number 或 Boolean ，则抛出异常；如果是 Null 则直接返回，如果是 Array，则执行 dataWriter.As&lt;int&gt; 将对象写入器转为 数组写入器，然后调用 ReadArray(IDataWriter&lt;int&gt; dataWriter); 方法。

#### 如果 JsonValueType 是 Object 类型，则执行 dataWriter.Initialize() 操作，此方法内部会执行 Context = new Demo(); 操作。

#### 然会跳过空白字符，找到一个键的开始索引，然后解析这个键得到字符串，如 "Id"；如果格式不正确则会引发异常。
###### Swifter.Json 支持 单引号的键 和 双引号的键 和 没有引号的键。没有引号的键会去除前后空白字符;比如 { Id : 123 } 得到 的键就是 "Id"。
###### 注意：Swifter.Json 虽然支持没有引号的键，但它不支持没有引号的 字符串值！虽然支持它很容易，但我还是不希望它被当作其他格式的解析工具用。

#### 得到键之后，解析器会查找第一个 ':'，如果没有找到 ':' 字符，将会引发 JsonDeserializeException。
###### 注意：字符期间的所有字符都会被跳过！即：{"Id" ABCD  : 123} 这个字符串也会被正常解析，"Id" 将作为 123 的键。

#### 现在找到 ':' 了，将索引设为 ':' 处 +1，然后跳过空白字符，来到 值 的位置，也就是 1 的位置。

#### 此时将调用 dataWriter.OnWriteValue(string name, IValueReader valueReader); 方法来通知对象写入器去读取该值赋给 "Id" 属性。

#### 在 dataWriter.OnWriteValue 内部会根据 name 匹配在 Id 属性，然后执行 Context.Id = valueReader.ReadInt32();

#### 然会回到 JsonDeserializer 的 ReadInt32 方法；该方法会检查当前索引处的 JsonValueType 是否为 Number，如果不是将引发异常或者进行类型转换。

#### 现在 JsonValueType 是 Number，ReadInt32 会首先执行 var numLength = NumberHelper.Decimal.TryParse(pJson + index, length - index, out int result); 操作。

#### 该方法会尝试解析一个常规十进制的整形字符串，并返回解析成功的字符数量，通过判断该返回值是否等于 0 就能得否解析成功。

#### 如果解析成功则 index += numLength; 然会返回。如果成功则执行 int.Parse 解析；int.Parse 解析失败则抛出异常，成功则继续解析。
###### 其实在这中间还有一个尝试，就是执行 NumberHelper.GetNumberInfo 去解析，该方法支持 指数，0xFF 和 1_000_000 的数字；执行该方法后还是失败的话再去调用 int.Parse。如果真的到了 int.Parse 这一步，那么肯定是抛出异常的。
###### 虽然 Swifter.Json 支持 指数，0xFF 和 1_000_000 样式的数字，但是仅限于没有引号包裹的数字；有引号包裹的数字字符串，如果尝试被一个 int 类型的属性接收，那么是直接执行 int.Parse！ 此方法是系统默认的格式，并不支持这些样式的数字。
###### 如果您想支持 "1_000_000" 或者其他样式的数字字符串，可以使用自定义类型处理器或者设置 CultureInfo 的默认样式来实现。详情请看 Wiki。

#### 数字解析完成之后返回到 dataWriter.OnWriteValue 方法，此方法赋值 Id 之后在返回到 dataWriter.ReadObject 方法里。

#### 此时解析器会跳过空白字符，然后得到位于索引处的一个非空白字符；如果此字符为 '}' 则结束解析并返回，如果此字符是 ',' 则尝试解析下一个 键。
###### 注意这里是尝试解析，也就是说如果，此时再次解析到 '}' 则也会正常结束解析；比如：{"Id":123,} 也会被正常解析，但 {"Id":123,,} 则会出现异常。
###### 还有：当解析到 '}' 字符时，如果当前 '}' 对应 JSON 中第一个(根)对象的 '{'，那么将结束解析！也就是说如果在该 '}' 之后还有内容的话会被忽略！
###### 如果您想解析类似 {"Id":1}{"Id":2} 类似这样的字符串，请自行使用 "[]" 包裹。
###### 特别注意：如果您使用流(TextReader) 的方式进行解析，那么 Swifter.Json 将会读取流的全部内容，但是只解析其第一个(根)对象，之后的内容会被忽略，并且您不能在流中读取到任何内容！

#### 此时找到 ',' 字符，然后跳过空白字符，然后又来到 键 的解析处，解析出 "Name" 还是一样的找到 ':' 然后跳过空白字符来到 "Dogwei" 的索引处。

#### 再次执行 dataWriter.OnWriteValue("Name", this); 操作来通知对象写入器去读取该值赋给 "Name" 属性。

#### 此时会来到 JsonDeserializer 的 ReadString 方法；该方法同样会检查当前索引处的 JsonValueType 是否为 String，如果不是将引发异常或者进行类型转换。

#### 然后解析器将读取第一个字符('"')作为该字符串的引号，意味着字符串的开始和结束字符。

#### 然后解析器会查找下一个该字符('"')，期间计数 '\' 的数量；当出现 '\' 字符时，判断下一个字符是否为 'u' 如果是，则跳过包含 '\' 在内的 6 个字符("\uAAAA")，如果不是则跳过两个字符("\n")。 

#### 遍历完成之后将判断出否出现 '\' 转义符，如果没有则直接返回这部分字符串的内容；如果有，则创建一个遍历内容长度减去转义内容的长度的空白字符串，该字符串长度刚好等于结果字符串，然后再次循环填充该字符串。
###### 该方式在没有 '\' 转义符时性能极佳，但是如果在有转义符时性能较低，处于中游水平；但内存分配始终时最小的。

#### 现在回到 dataWriter.OnWriteValue 方法里将该值赋予 Context.Name。然会返回解析器。

#### 解析器继续解析会解析到 '}'，然会返回到 FastObjectInterface&lt;Demo&gt;.ReadValue，该方法返回 fastObjectRW.Context 给 JsonFormatter.DeserializeObject。

#### JsonFormatter.DeserializeObject 再返回对象给调用者，解析就完成了。

## 这里还有几个关于 Swifter.Json 的注意事项：

#### 1：Swifter.Json 解析器支持 "\uAAAA" 这样的格式，但序列化时永远也不会将中文字符或其它多字节字符序列化为 "\uAAAA" 格式，我希望这事由编码去做。
#### 2：Swifter.Json 解析器支持没有引号的键，但是序列化时绝对不会出现没有引号的键，因为这不是 JSON 标准。
#### 3：Swifter.Json 可能会将一个错误的 JSON 正常处理(如:{Id:123,})，但绝对不会将一个正确的 JSON 错误处理！

## 更新历史

### 1.2.2 更新:

#### 1：增加了异步方法，JsonFormatter 中以 Async 结尾的方法均为异步方法。
#### 2：修改 Swifter.Extensions.AspNetCore 的扩展使用异步方法。

### 1.2.1 更新:

#### 1：再度提高性能 (主要原理是对不常见行为禁止内联，提高常见行为的内联成功率)。
#### 2：解决枚举序列化出错，ValueInterface&lt;T&gt;.SetInterface() 不起作用等 BUG。
#### 3：增加特性定义 (反)序列化行为 ([RWFormat], [RWField], [RWObject] 等特性)。
#### 4：增加 AspNetCore 的扩展方法 ConfigureJsonFormatter(this IServiceCollection services)。现在可以很方便将 Swifter.Json 配置到 MVC 了。
#### 5：新增 JsonValue 类，此类可以表示 JSON 反序列化时的任何值（包括对象和数组）。
