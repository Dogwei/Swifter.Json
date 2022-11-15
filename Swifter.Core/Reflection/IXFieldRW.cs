
using Swifter.RW;
using Swifter.Tools;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.Reflection
{
    interface IXFieldRW : IObjectField
    {
        void OnReadValue(object obj, IValueWriter valueWriter);

        void OnWriteValue(object obj, IValueReader valueReader);

        void OnReadAll(object obj, IDataWriter<string> dataWriter);

        void OnWriteAll(object obj, IDataReader<string> dataReader);

        IValueRW CreateValueRW(XObjectRW baseRW);
    }
}