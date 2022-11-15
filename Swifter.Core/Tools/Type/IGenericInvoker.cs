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
        /// <typeparam name="T">泛型</typeparam>
        void Invoke<T>();
    }
}