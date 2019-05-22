# Swifter.Json

#### .Net 平台上的一个功能强大，简单易用，稳定又不失高性能的 JSON 序列化和反序列化工具。

## 特性

#### 1: 支持 .Net 上绝大多是的数据类型，且轻松扩展；包括但不限于：实体，字典，集合，迭代器，DbDataReader和 DataTable。

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

##### 平台兼容性     ✓：兼容大多数平台的大多数版本；乄：兼容部分平台，且版本要求较高；✗：只能在单一平台上运行。
##### 稳定性         ✓：在大多数测试中未出现 BUG；乄：一些不常见操作会出现 BUG；✗：常见操作会出现 BUG。
##### 功能性         ✓：支持大多数的数据类型和方法；乄：支持常用的数据类型和方法；✗：部分常用数据类型和方法不支持。
##### 扩展性         ✓：高度允许自定义格式和处理方式；乄：支持常用的格式设置；✗：不能自定义格式。
##### 高性能         ✓：相比 Newtonsoft 平均快 4x 以上；乄：相比 Newtonsoft 平均快 2x 以上；✗：相比 Newtonsoft 差不多或者更慢。
##### 小分配(内存)   ✓：执行过程中分配的内存极少；乄：必要的内存占用较少；✗：执行过程中分配的大量的临时内存。
##### 大小(文件)     ✓：小于 100KB；乄：大于 100KB 小于 500 KB；✗：大于 500 KB。

## 部分 .Net 现有的 JSON 工具性能对比

#### .Net Core 3.0 Previews running results.

![.Net Core 3.0 Previews running results](benchmark.png)

#### .Net Framework 4.7.1 Previews running results.

![.Net Framework 4.7.1 Previews running results](benckmark_for_framework_4.7.1.png)

##### 图中