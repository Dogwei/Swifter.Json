


namespace Swifter.RW
{
    /// <summary>
    /// 提供某一类型在 IValueReader 中读取值和在 IValueWriter 写入值的方法。
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public interface IValueInterface<T>
    {
        /// <summary>
        /// 在 IValueReader 中读取该类型的值。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的值</returns>
        T ReadValue(IValueReader valueReader);

        /// <summary>
        /// 在 IValueWriter 中写入该类型的值。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">该类型的值</param>
        void WriteValue(IValueWriter valueWriter, T value);
    }
}