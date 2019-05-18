namespace Swifter.RW
{
    /// <summary>
    /// 表示允许指定类型的数据源初始化的数据读写器
    /// </summary>
    /// <typeparam name="T">指定类型</typeparam>
    public interface IInitialize<T>
    {
        /// <summary>
        /// 初始化数据读写器
        /// </summary>
        /// <param name="obj">数据源</param>
        void Initialize(T obj);

        /// <summary>
        /// 获取数据源
        /// </summary>
        T Content { get; }
    }
}
