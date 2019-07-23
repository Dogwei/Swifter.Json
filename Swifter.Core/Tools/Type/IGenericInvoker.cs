namespace Swifter.Tools
{
    /// <summary>
    /// 表示一个泛型执行器。
    /// </summary>
    public interface IGenericInvoker
    {
        /// <summary>
        /// 泛型执行方法。
        /// </summary>
        /// <typeparam name="TKey">泛型</typeparam>
        void Invoke<TKey>();
    }

    /// <summary>
    /// 带返回值的泛型执行器。
    /// </summary>
    /// <typeparam name="TResult">返回值类型</typeparam>
    public interface IGenericInvoker<TResult>
    {
        /// <summary>
        /// 泛型执行方法。
        /// </summary>
        /// <typeparam name="TKey">泛型</typeparam>
        /// <returns>返回一个泛型值</returns>
        TResult Invoke<TKey>();
    }
}