namespace Swifter.RW
{
    /// <summary>
    /// 提供类型与 IValueInterface 的匹配器。
    /// 实现它，并使用 ValueInterface.AddMaper 添加它的实例即可自定义类型的读写方法。
    /// </summary>
    public interface IValueInterfaceMaper
    {
        /// <summary>
        /// 类型与 IValueInterface 的匹配方法。
        /// 匹配成功则返回实例，不成功则返回 Null。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回一个 IValueInterface/<T/> 实例</returns>
        IValueInterface<T> TryMap<T>();
    }
}