using Swifter.Readers;
using Swifter.Writers;
using System.Collections.Generic;

namespace Swifter.RW
{
    /// <summary>
    /// 提供以某元素类型集合计算 Id64 值为键的数据读写器。
    /// 仅提供 OnReadValue 和 OnWriteValue 接口。
    /// 并且实现此接口的类型，必须具有类型唯一性。
    /// 即一个读写器类型的 Keys 内容必须始终相同，并且同一个键始终获取的 Id64 值都相同。
    /// </summary>
    /// <typeparam name="TSymbol">集合元素的类型</typeparam>
    public interface IId64DataRW<TSymbol>
    {
        /// <summary>
        /// 计算 Id64 值。
        /// </summary>
        /// <param name="firstSymbol">第一个 TSymbol 的引用</param>
        /// <param name="length">TSymbol 集合的长度</param>
        /// <returns>返回 Id64 值</returns>
        long GetId64(ref TSymbol firstSymbol, int length);

        /// <summary>
        /// 计算 Id64 值。
        /// </summary>
        /// <param name="symbols">TSymbol 集合</param>
        /// <returns>返回 Id64 值</returns>
        long GetId64(IEnumerable<TSymbol> symbols);

        /// <summary>
        /// 以 Id64 值为键，从数据读写器中读取一个值到值写入器中。
        /// </summary>
        /// <param name="id64">Id64 值</param>
        /// <param name="valueWriter">值写入器</param>
        void OnReadValue(long id64, IValueWriter valueWriter);

        /// <summary>
        /// 以 Id64 值为键，从值读取器中读取该类型的值写入到数据读写器中的指定字段中。
        /// </summary>
        /// <param name="id64">Id64 值</param>
        /// <param name="valueReader">值读取器</param>
        void OnWriteValue(long id64, IValueReader valueReader);
    }
}