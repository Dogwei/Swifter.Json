namespace Swifter.RW
{
    /// <summary>
    /// 链式值读写器接口。
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    public abstract class ChainedValueInterfaceBase<T> : IValueInterface<T>
    {
        /// <summary>
        /// 上一个值读写器接口。
        /// </summary>
        internal protected IValueInterface<T>? PreviousValueInterface;

        /// <summary>
        /// 在 IValueReader 中读取该类型的值。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的值</returns>
        public abstract T? ReadValue(IValueReader valueReader);

        /// <summary>
        /// 在 IValueWriter 中写入该类型的值。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">该类型的值</param>
        public abstract void WriteValue(IValueWriter valueWriter, T? value);
    }
}