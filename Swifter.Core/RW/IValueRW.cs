using Swifter.Readers;
using Swifter.Writers;

namespace Swifter.RW
{
    /// <summary>
    /// 基础类型的值读写器
    /// </summary>
    public interface IValueRW : IValueReader, IValueWriter
    {
    }

    /// <summary>
    /// 自定义类型的值读写器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValueRW<T> :IValueReader<T>, IValueWriter<T>
    {
    }
}