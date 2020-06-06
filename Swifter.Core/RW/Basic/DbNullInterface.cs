

using System;

namespace Swifter.RW
{
    internal sealed class DbNullInterface : IValueInterface<DBNull>
    {
        public DBNull ReadValue(IValueReader valueReader)
        {
            var value = valueReader.DirectRead();

            if (value is null || value == DBNull.Value)
            {
                return DBNull.Value;
            }

            throw new NotSupportedException("Unable convert value to DbNull.");
        }

        public void WriteValue(IValueWriter valueWriter, DBNull value)
        {
            valueWriter.DirectWrite(null);
        }
    }
}