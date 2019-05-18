using Swifter.Writers;

namespace Swifter.Readers
{
    /// <summary>
    /// 提供将值读取器中的值填充到数据写入器的接口。
    /// </summary>
    /// <typeparam name="Key">键的类型</typeparam>
    public interface IValueFiller<Key>
    {
        /// <summary>
        /// 填充该数据写入器。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        void FillValue(IDataWriter<Key> dataWriter);
    }
}
