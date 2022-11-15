namespace Swifter.RW
{
    /// <summary>
    /// 提供类型与 IValueInterface 的匹配器。
    /// 实现它，并使用 <see cref="ValueInterface.AddMaper(IValueInterfaceMaper)"/> 添加它的实例即可自定义类型的读写方法。
    /// </summary>
    public interface IValueInterfaceMaper
    {
        /// <summary>
        /// 类型与 IValueInterface 的匹配方法。
        /// 匹配成功则返回实例，不成功则返回 <see langword="null"/> 以流转到下一个匹配器。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回一个 <see cref="IValueInterface{T}"/> 实例</returns>
        IValueInterface<T>? TryMap<T>();
    }
}