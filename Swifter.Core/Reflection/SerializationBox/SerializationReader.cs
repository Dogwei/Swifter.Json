using Swifter.RW;
using Swifter.Tools;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Swifter.Reflection
{
    sealed class SerializationReader : IDataReader<string>
    {
        readonly Type? declaringType;
        readonly object content;

        readonly object? info;

        public SerializationReader(Type? declaringType, object content)
        {
            this.declaringType = declaringType;
            this.content = content;

            switch (SerializationHelper.GetSerializationType(content.GetType()))
            {
                case SerializationType.Serializable:
                    info = new SerializationInfo(content.GetType(), ValueInterfaceFormatterConverter.Instance);
                    Unsafe.As<ISerializable>(content).GetObjectData(Unsafe.As<SerializationInfo>(info), SerializationHelper.CurrentSerializationStreamingContext);
                    break;
                case SerializationType.Object:
                    info = XTypeInfo.Create(content.GetType(), XBindingFlags.Public | XBindingFlags.NonPublic | XBindingFlags.Instance | XBindingFlags.Field | XBindingFlags.InheritedMembers);
                    break;
                case SerializationType.Array:
                    info = new SerializationArrayInfo(Unsafe.As<Array>(content));
                    break;
            }
        }

        public SerializationReader(Type? declaringType, object content, SerializationReferenceInfo referenceInfo)
        {
            this.declaringType = declaringType;
            this.content = content;

            info = referenceInfo;
        }

        public IValueReader this[string key] => throw new NotImplementedException();

        public int Count => -1;

        public Type? ContentType => null;

        public Type? ValueType => null;

        public object? Content
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public void OnReadAll(IDataWriter<string> dataWriter, RWStopToken stopToken = default)
        {
            if (info is SerializationReferenceInfo referenceInfo)
            {
                ValueInterface.WriteValue(dataWriter[SerializationHelper.ReferenceFieldName], referenceInfo.TargetIndex);

                return;
            }

            var contentType =
                info is SerializationInfo ? Unsafe.As<SerializationInfo>(info).GetObjectType() :
                content.GetType();

            if (contentType != declaringType)
            {
                ValueInterface.WriteValue(dataWriter[SerializationHelper.TypeFieldName], contentType);
            }

            if (info is not null)
            {
                if (info is SerializationInfo serializationInfo)
                {
                    foreach (var item in serializationInfo)
                    {
                        var name = item.Name;
                        var value = item.Value;

                        SerializationHelper.WriteSerializationItemValue(dataWriter[name], value);
                    }

                    return;
                }

                if (info is XTypeInfo typeInfo)
                {
                    for (int i = 0; i < typeInfo.FieldsCount; i++)
                    {
                        var fieldInfo = typeInfo.GetField(i);

                        var value = fieldInfo.GetValue(content);

                        if (!TypeHelper.IsEmptyValue(value))
                        {
                            SerializationHelper.WriteValue(dataWriter[fieldInfo.Name], fieldInfo.FieldInfo.FieldType, value);
                        }
                    }

                    return;
                }

                if (info is SerializationArrayInfo multiDimArrayInfo)
                {
                    if (multiDimArrayInfo.Lengths is not null)
                    {
                        ValueInterface.WriteValue(dataWriter[SerializationHelper.LengthsFieldName], multiDimArrayInfo.Lengths);
                    }
                    else
                    {
                        ValueInterface.WriteValue(dataWriter[SerializationHelper.LengthFieldName], multiDimArrayInfo.Length);
                    }

                    if (multiDimArrayInfo.LowerBounds is not null)
                    {
                        ValueInterface.WriteValue(dataWriter[SerializationHelper.LowerBoundsFieldName], multiDimArrayInfo.LowerBounds);
                    }

                    var arrayReader = SerializationArrayRWHelper.CreateReader(Unsafe.As<Array>(content));

                    if (arrayReader.Count > 0)
                    {
                        dataWriter[SerializationHelper.ValueFieldName].WriteArray(arrayReader);
                    }

                    return;
                }
            }

            SerializationHelper.WriteValue(dataWriter[SerializationHelper.ValueFieldName], content.GetType(), content);
        }

        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            throw new NotImplementedException();
        }
    }
}