using System;

namespace Swifter.Tools
{
    internal sealed class ToDBNullConvert<T> : IXConverter<T, DBNull>
    {
        public DBNull Convert(T value) => value is null ? DBNull.Value : throw new InvalidOperationException("Unable convert a value to DBNull.");
    }
}