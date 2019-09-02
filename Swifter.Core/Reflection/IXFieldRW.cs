
using Swifter.RW;


namespace Swifter.Reflection
{
    interface IXFieldRW : IObjectField
    {
        void OnReadValue(object obj, IValueWriter valueWriter);

        void OnWriteValue(object obj, IValueReader valueReader);

        T ReadValue<T>(object obj);

        void WriteValue<T>(object obj, T value);
    }
}