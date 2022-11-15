namespace Swifter.Tools
{
    /// <summary>
    /// 类型转换方式。
    /// </summary>
    public enum XConvertMode
    {
        /// <summary>
        /// 基础类型的隐式转换。
        /// </summary>
        BasicImplicit,
        /// <summary>
        /// 实现类到基类或接口的转换。
        /// </summary>
        Covariant,
        /// <summary>
        /// implicit 函数实现的转换。
        /// </summary>
        Implicit,
        /// <summary>
        /// 基础类型的显式转换。
        /// </summary>
        BasicExplicit,
        /// <summary>
        /// explicit 函数实现的转换。
        /// </summary>
        Explicit,
        /// <summary>
        /// 从子类或接口到实现类的转换。
        /// </summary>
        Inverter,
        /// <summary>
        /// 扩展类型转换。
        /// </summary>
        Extended,
        /// <summary>
        /// 用户自定义的转换。这会覆盖其他转换函数，但不影响其他能否类型转换判断的结果。
        /// </summary>
        Custom
    }
}