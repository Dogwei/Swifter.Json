namespace Swifter.Reflection
{
    enum SerializationType
    {
        /// <summary>
        /// 基元类型 基础数据类型/String/Decimal/Enum/Type
        /// </summary>
        Primitive,
        /// <summary>
        /// 数组
        /// </summary>
        Array,
        /// <summary>
        /// 可序列化对象
        /// </summary>
        Serializable,
        /// <summary>
        /// 普通对象
        /// </summary>
        Object
    }
}