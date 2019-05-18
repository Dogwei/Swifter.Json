using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.Formatters
{
    sealed class AuxiliaryInterface<TKey> : IValueInterface<AuxiliaryWriter<TKey>>
    {
        static readonly bool IsArray;

        static AuxiliaryInterface()
        {
            var tKey = typeof(TKey);
            
            switch (Type.GetTypeCode(tKey))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    IsArray = true && !tKey.IsEnum;
                    break;
            }
        }

        public AuxiliaryWriter<TKey> ReadValue(IValueReader valueReader)
        {
            var auxiliaryWriter = new AuxiliaryWriter<TKey>();

            if (valueReader is IValueFiller<TKey> tReader)
            {
                tReader.FillValue(auxiliaryWriter);
            }
            else if (IsArray)
            {
                valueReader.ReadArray(auxiliaryWriter.As<int>());
            }
            else
            {
                valueReader.ReadObject(auxiliaryWriter.As<string>());
            }

            return auxiliaryWriter;
        }

        public void WriteValue(IValueWriter valueWriter, AuxiliaryWriter<TKey> value)
        {
            throw new NotSupportedException("Unable write a writer.");
        }
    }
}