using System;
using System.ComponentModel;

namespace Swifter.Json
{
    /// <summary>
    /// JSON 格式化器配置项。
    /// </summary>
    [Flags]
    public enum JsonFormatterOptions : int
    {
        /// <summary>
        /// 默认配置项。
        /// </summary>
        Default = OutOfDepthException,
        
        /// <summary>
        /// 在序列化时出现循环引用的对象时将发生异常。该选项不能和其他引用配置复用。
        /// </summary>
        LoopReferencingException = 0x1,

        /// <summary>
        /// 在序列化时出现循环引用的对象时将用 Null 表示。该选项不能和其他引用配置复用。
        /// </summary>
        LoopReferencingNull = 0x2,

        /// <summary>
        /// 在序列化时出现已序列化的对象时将用 Null 表示。该选项不能和其他引用配置复用。
        /// </summary>
        MultiReferencingNull = 0x4,

        /// <summary>
        /// 在序列化和反序列化时允许使用 $ref 写法表示重复引用的对象。该选项不能和其他引用配置复用。
        /// </summary>
        MultiReferencingReference = 0x8,

        /// <summary>
        /// 此配置是 <see cref="MultiReferencingNull"/> 和 <see cref="MultiReferencingReference"/> 的可选配置。<br/>
        /// 此配置将会优化结构布局，使被引用对象始终为最浅层的引用（而不是第一次出现的引用）。<br/>
        /// 此配置会将浅层对象或数组先遍历保存引用，然再执行序列化和引用检查，性能损耗较大。<br/>
        /// </summary>
        MultiReferencingOptimizeLayout = 0x10,

        /// <summary>
        /// 此配置是 <see cref="MultiReferencingNull"/> 和 <see cref="MultiReferencingReference"/> 的可选配置。<br/>
        /// 开启此配置将字符串也纳入多引用范畴。<br/>
        /// </summary>
        MultiReferencingAlsoString = 0x20,

        /// <summary>
        /// 在反序列化对象时，将执行假定字段顺序对应的解析。<br/>
        /// 如果启用此配置；那么当 Json 对象与目标对象的字段顺序相同时效率更快，反之则变慢。<br/>
        /// 原理时在反序列化时，将循环一次目标对象的所有字段；<br/>
        /// 当目标对象的字段名与当前正在解析的 Json 字段名一致时（忽略大小写），则读取 Json 字段对应的值到目标对象的字段对应的值中；<br/>
        /// 如果字段名称不匹配，则跳过读取，并继续循环目标对象的字段。<br/>
        /// 当满足顺序要求时，如果目标对象的字段数多于或等于 Json 对象中的字段数，这个假定有序的解析会成功。<br/>
        /// 但如果目标对象的字段数少于 Json 对象中的字段数那么假定有序的解析不成功，会降低性能。<br/>
        /// </summary>
        AsOrderedObjectDeserialize = 0x40,

        /// <summary>
        /// 执行紧凑（无多余空格）且标准的 JSON 反序列化，此配置有效提高反序列化性能。
        /// </summary>
        DeflateDeserialize = 0x80,

        /// <summary>
        /// 执行标准的 JSON 反序列化（即 不执行部分验证）。
        /// </summary>
        StandardDeserialize = 0x100,

        /// <summary>
        /// 执行完全验证的 JSON 反序列化（这是默认行为）。
        /// </summary>
        VerifiedDeserialize = 0x0,

        /// <summary>
        /// 反序列化的配置项，当反序列化除字符串和通用类型外的可空类型时，如果 Json 值是 0 长度的字符串，则解析为 Null。
        /// </summary>
        EmptyStringAsNull = 0x400,

        /// <summary>
        /// 反序列化的配置项，当反序列化非字符串和通用类型时，如果 Json 值是 0 长度的字符串，则解析为 Default。
        /// </summary>
        EmptyStringAsDefault = 0x800,

#if WhiteSpaceStringAsNull

        /// <summary>
        /// 反序列化的配置项，当反序列化除字符串和通用类型外的可空类型时，如果 Json 值是空白字符串（包括空格换行符等），则解析为 Null。
        /// </summary>
        WhiteSpaceStringAsNull = EmptyStringAsNull | 0x1000,

        /// <summary>
        /// 反序列化的配置项，当反序列化非字符串和通用类型时，如果 Json 值是空白字符串（包括空格换行符等），则解析为 Default。
        /// </summary>
        WhiteSpaceStringAsDefault = EmptyStringAsDefault | 0x1000,

#endif

        /// <summary>
        /// 反序列化的内部配置项，传入反序列化器的原始内容可以被作为缓存区而修改。<br/>
        /// 此配置会提高解析字符串的性能，但是原始内容会被修改而不能再使用。<br/>
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        ModifiableOriginal = 0x2000,

        /// <summary>
        /// 序列化时对 JSON 进行缩进美化。
        /// </summary>
        Indented = 0x4000,

        /// <summary>
        /// 如果启用，在序列化或反序列化时当 Json 结构超出深度限制（<see cref="JsonFormatter.MaxDepth"/>）则抛出异常。<br/>
        /// 如果不启用，超出深度限制部分将不会被序列化或反序列化。<br/>
        /// </summary>
        OutOfDepthException = 0x8000,

        /// <summary>
        /// 如果启用，则在序列化和反序列化时，使用 System 命名空间里的浮点数算法，浮点数是 <see cref="float"/> 和 <see cref="double"/>。<br/>
        /// 此配置能让浮点数的格式化和解析的结果始终和 ToString 和 Parse 的结果保持一致；<br/>
        /// 缺点是性能变低，尤其是在早期 .NET 版本为 40 倍性能差，在 Core3.0 下缩减为 4 倍。<br/>
        /// 默认情况下，Swifter 的浮点数算法在浮点数特别大或特别小时，得出的结果可能和 System 里的算法得出的结果不一致；<br/>
        /// 这是浮点数的特点之一，在不同的系统，平台，CPU或算法下都会产生这个问题，这是正常的。<br/>
        /// 由于早期 .NET 版本的浮点数算法缺陷，导致开启此配置后在浮点数特别大或特别小时，反序列化可能会引发 <see cref="OverflowException"/> 异常。<br/>
        /// </summary>
        UseSystemFloatingPointsMethods = 0x10000,

        /// <summary>
        /// 序列化对象时，字段名使用驼峰命名法。即：如果字段名首字母为大写，则将首字母写入为小写字母。
        /// </summary>
        CamelCaseWhenSerialize = 0x20000,

        /// <summary>
        /// 启用对象字段和值的筛选。<br/>
        /// 此配置与 <see cref="JsonFormatter.ObjectFiltering"/> 事件相互影响。<br/>
        /// </summary>
        OnFilter = 0x40000,

        /// <summary>
        /// 在序列化或反序列化时忽略 Null 值。
        /// </summary>
        IgnoreNull = 0x80000,

        /// <summary>
        /// 在序列化或反序列化时忽略 0 值。
        /// </summary>
        IgnoreZero = 0x100000,

        /// <summary>
        /// 在序列化时忽略 ""(空字符串) 值。
        /// </summary>
        IgnoreEmptyString = 0x200000,

        /// <summary>
        /// 启用数组元素的筛选。<br/>
        /// 此配置与 <see cref="JsonFormatter.ArrayFiltering"/> 事件相互影响。<br/>
        /// </summary>
        ArrayOnFilter = 0x400000,
    }
}