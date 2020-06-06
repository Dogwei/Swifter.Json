namespace Swifter.RW
{
    /// <summary>
    /// FastObjectRW 创建接口。
    /// 此接口由 Emit 实现。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFastObjectRWCreater<T>
    {
        /// <summary>
        /// 创建该类型的对象读写器。
        /// </summary>
        /// <returns>返回该类型</returns>
        FastObjectRW<T> Create();
    }
}