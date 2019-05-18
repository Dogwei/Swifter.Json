using Swifter.RW;

namespace Swifter.Readers
{
    /// <summary>
    /// 允许填充一个以 Id64 的数据读写器的值读取器。
    /// </summary>
    /// <typeparam name="TSymbol">集合元素的类型</typeparam>
    public interface IId64Filler<TSymbol>
    {
        /// <summary>
        /// 填充一个以 Id64 的数据读写器。
        /// </summary>
        /// <typeparam name="TDataWriter">要求同时实现 IDataRW 和 IId64DataRW&lt;TSymbol&gt;</typeparam>
        /// <param name="dataWriter">目标数据读写器类型</param>
        void FillValue<TDataWriter>(TDataWriter dataWriter) where TDataWriter : IDataRW, IId64DataRW<TSymbol>;
    }
}
