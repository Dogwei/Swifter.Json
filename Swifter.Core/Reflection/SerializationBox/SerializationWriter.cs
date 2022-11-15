using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Swifter.Reflection
{
    sealed class SerializationWriter : IDataWriter<string>
    {
        readonly Type? declaringType;

        object? content;
        object? info;
        bool isInitialized;

        public SerializationWriter(Type? declaringType)
        {
            this.declaringType = declaringType;
        }

        public IValueWriter this[string key] => throw new NotImplementedException();

        public int Count => -1;

        public Type? ContentType => null;

        public Type? ValueType => null;

        public object? Content
        {
            get
            {
                if (info is SerializationInfo serializationInfo && content is null)
                {
                    var constructor = serializationInfo
                        .GetObjectType()
                        .GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);

                    if (constructor is null)
                    {
                        throw new NotSupportedException(/* 可序列化对象未提供序列化构造函数 */);
                    }

                    content = constructor.Invoke(new object[] { serializationInfo, SerializationHelper.CurrentDeserializationStreamingContext });

                    if (content is IObjectReference)
                    {
                        content = Unsafe.As<IObjectReference>(content).GetRealObject(SerializationHelper.CurrentDeserializationStreamingContext);
                    }

                    if (content is IDeserializationCallback)
                    {
                        SerializationHelper.AddDeserializationCallback(Unsafe.As<IDeserializationCallback>(content));
                    }
                }

                if (info is SerializationArrayInfo arrayInfo && content is null)
                {
                    content = arrayInfo.CreateInstance();
                }

                return content;
            }
            set
            {
                content = value;
            }
        }

        public void Initialize()
        {
            content = null;
            info = null;
            isInitialized = true;
        }

        public void Initialize(int capacity)
        {
            Initialize();
        }

        public void OnWriteAll(IDataReader<string> dataReader, RWStopToken stopToken = default)
        {
            throw new NotImplementedException();
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            if (!isInitialized)
            {
                throw new NullReferenceException(nameof(Content));
            }

            switch (key)
            {
                case SerializationHelper.TypeFieldName:

                    if (info is null)
                    {
                        var type = SerializationHelper.ReadValue<Type>(valueReader);

                        type ??= declaringType ?? throw new InvalidOperationException(/* 无法得知反序列化的类型 */);

                        switch (SerializationHelper.GetSerializationType(type))
                        {
                            case SerializationType.Primitive:
                                info = type;
                                break;
                            case SerializationType.Array:
                                info = new SerializationArrayInfo(type);
                                break;
                            case SerializationType.Serializable:
                                info = new SerializationInfo(type, ValueInterfaceFormatterConverter.Instance);
                                break;
                            case SerializationType.Object:
                                info = XTypeInfo.Create(type, XBindingFlags.Public | XBindingFlags.NonPublic | XBindingFlags.Instance | XBindingFlags.Field | XBindingFlags.InheritedMembers);
                                content = FormatterServices.GetUninitializedObject(type);
                                break;
                        }

                        return;
                    }

                    goto default;

                case SerializationHelper.ReferenceFieldName:

                    if (info is null)
                    {
                        var targetIndex = ValueInterface.ReadValue<int>(valueReader);

                        info = new SerializationReferenceInfo(targetIndex);
                        content = SerializationHelper.GetDeserializationReferenceTargetObject(Unsafe.As<SerializationReferenceInfo>(info));

                        return;
                    }

                    goto default;

                case SerializationHelper.LengthFieldName:

                    if (info is null && declaringType is not null && SerializationHelper.GetSerializationType(declaringType) is SerializationType.Array)
                    {
                        info = new SerializationArrayInfo(declaringType);
                    }

                    if (info is SerializationArrayInfo)
                    {
                        Unsafe.As<SerializationArrayInfo>(info).Length = ValueInterface.ReadValue<int>(valueReader);

                        return;
                    }

                    goto default;

                case SerializationHelper.LengthsFieldName:

                    if (info is null && declaringType is not null && SerializationHelper.GetSerializationType(declaringType) is SerializationType.Array)
                    {
                        info = new SerializationArrayInfo(declaringType);
                    }

                    if (info is SerializationArrayInfo)
                    {
                        Unsafe.As<SerializationArrayInfo>(info).Lengths = ValueInterface.ReadValue<int[]>(valueReader);

                        return;
                    }

                    goto default;

                case SerializationHelper.LowerBoundsFieldName:

                    if (info is null && declaringType is not null && SerializationHelper.GetSerializationType(declaringType) is SerializationType.Array)
                    {
                        info = new SerializationArrayInfo(declaringType);
                    }

                    if (info is SerializationArrayInfo)
                    {
                        Unsafe.As<SerializationArrayInfo>(info).LowerBounds = ValueInterface.ReadValue<int[]>(valueReader);

                        return;
                    }

                    goto default;

                case SerializationHelper.ValueFieldName:

                    if (info is Type)
                    {
                        content = SerializationHelper.ReadValue(valueReader, Unsafe.As<Type>(info));

                        return;
                    }

                    if (info is SerializationArrayInfo multiDimArrayInfo)
                    {
                        var array = multiDimArrayInfo.CreateInstance();

                        var arrayWriter = SerializationArrayRWHelper.CreateWriter(array);

                        valueReader.ReadArray(arrayWriter);

                        content = array;

                        return;
                    }

                    goto default;
                default:

                    if (info is null && declaringType is not null)
                    {
                        switch (SerializationHelper.GetSerializationType(declaringType))
                        {
                            case SerializationType.Serializable:
                                info = new SerializationInfo(declaringType, ValueInterfaceFormatterConverter.Instance);
                                break;
                            case SerializationType.Object:
                                info = XTypeInfo.Create(declaringType, XBindingFlags.Public | XBindingFlags.NonPublic | XBindingFlags.Instance | XBindingFlags.Field | XBindingFlags.InheritedMembers);
                                content = FormatterServices.GetUninitializedObject(declaringType);
                                break;
                        }
                    }

                    if (info is XTypeInfo typeInfo)
                    {
                        var fieldInfo = typeInfo.GetField(key);

                        if (fieldInfo is null)
                        {
                            goto KeyNotFound;
                        }

                        fieldInfo.SetValue(content, SerializationHelper.ReadValue(valueReader, fieldInfo.FieldInfo.FieldType));

                        return;
                    }

                    if (info is SerializationInfo serializationInfo)
                    {
                        var value = SerializationHelper.ReadSerializationItemValue(valueReader);

                        serializationInfo.AddValue(key, value);

                        return;
                    }

                KeyNotFound:
                    throw new KeyNotFoundException();
            }
        }
    }
}