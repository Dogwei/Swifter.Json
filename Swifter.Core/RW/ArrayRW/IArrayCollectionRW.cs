using Swifter.Tools;

namespace Swifter.RW
{
    /// <summary>
    /// 标识数组读写器的接口。
    /// </summary>
    public interface IArrayCollectionRW
    {
        /// <summary>
        /// 执行元素类型。
        /// </summary>
        /// <param name="invoker">泛型执行器</param>
        void InvokeElementType(IGenericInvoker invoker);
    }
}
