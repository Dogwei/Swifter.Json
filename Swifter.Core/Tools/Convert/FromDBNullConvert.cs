using System;

namespace Swifter.Tools
{
    internal sealed class FromDBNullConvert<T> : IXConverter<DBNull, T>
    {
        public T Convert(DBNull value)
        {
            return default;
        }
    }
}