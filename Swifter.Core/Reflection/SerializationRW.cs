using Swifter.RW;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

using static Swifter.Reflection.SerializationBox;

namespace Swifter.Reflection
{
    sealed class SerializationRW : IDataRW<string>
    {
        /// <summary>
        /// 定义类型。
        /// </summary>
        readonly Type declaringType;

        /// <summary>
        /// 实际类型。
        /// </summary>
        Type actualType;

        /// <summary>
        /// 是否初始化过。
        /// </summary>
        bool isInitialized;

        /// <summary>
        /// 当前对象。
        /// </summary>
        object obj;

        /// <summary>
        /// 当前序列化信息。
        /// </summary>
        SerializationInfo serializationInfo;

        /// <summary>
        /// 对象的类型信息。
        /// </summary>
        XTypeInfo xTypeInfo;

        public SerializationRW(Type declaringType)
        {
            this.declaringType = declaringType;
        }

        public SerializationRW(object obj, Type declaringType)
        {
            this.declaringType = declaringType;
            this.obj = obj;

            isInitialized = true;
            actualType = obj.GetType();

            if (IsSerializable(actualType))
            {
                serializationInfo = CreateSerializationInfo(obj);


#if FullParse
                if (serializationInfo.ObjectType != null && serializationInfo.ObjectType != actualType)
                {
                    actualType = serializationInfo.ObjectType;
                }

#else
                if (serializationInfo.AssemblyName != null && serializationInfo.FullTypeName != null)
                {
                    if (Assembly.Load(serializationInfo.AssemblyName) is Assembly assembly && assembly.GetType(serializationInfo.FullTypeName) is Type objectType)
                    {
                        if (objectType != actualType)
                        {
                            actualType = objectType;
                        }
                    }
                }
#endif
            }
        }

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];

        public IEnumerable<string> Keys => null;

        public int Count => -1;

        public Type ContentType => actualType ?? declaringType;

        public object Content
        {
            get => GetContent(false);
            set
            {
                if (value != null)
                {
                    if (!ContentType.IsInstanceOfType(value))
                    {
                        throw new InvalidCastException(nameof(value));
                    }

                    obj = value;

                    isInitialized = true;
                    actualType = value.GetType();
                }
            }
        }

        public IValueRW this[string key] => new ValueCopyer<string>(this, key);

        public SerializationInfo GetOrCreateSerializationInfo()
        {
            return serializationInfo ?? (serializationInfo = CreateSerializationInfo(ContentType));
        }

        public XTypeInfo GetOrCreateXTypeInfo()
        {
            return xTypeInfo ?? (xTypeInfo = XTypeInfo.Create(ContentType,
                XBindingFlags.Public |
                XBindingFlags.NonPublic |
                XBindingFlags.Instance |
                XBindingFlags.Field));
        }

        public object GetOrCreateContent()
        {
            return obj ?? (obj = FormatterServices.GetUninitializedObject(ContentType));
        }

        public object GetContent(bool isFinish)
        {
            if (!isInitialized)
            {
                return null;
            }

            if (IsPrimitive(ContentType))
            {
                if (obj is null)
                {
                    throw new InvalidOperationException($"Must to set {"m_value"} first.");
                }

                return obj;
            }
            else if (IsArray(ContentType))
            {
                if (obj is null)
                {
                    throw new InvalidOperationException($"Must to set {"m_value"} first.");
                }

                return obj;
            }

            if (isFinish)
            {
                if (IsSerializable(ContentType) && obj is null)
                {
                    obj = CallConstructor(GetOrCreateContent(), GetOrCreateSerializationInfo());
                }

                if (IsDeserializationCallback(ContentType))
                {
                    Underlying.As<IDeserializationCallback>(GetOrCreateContent()).OnDeserialization(this);
                }
            }

            return GetOrCreateContent();
        }

        public void Initialize()
        {
            isInitialized = true;
        }

        public void Initialize(int capacity)
        {
            Initialize();
        }

        public void OnWriteAll(IDataReader<string> dataReader)
        {
            actualType = ValueInterface<Type>.ReadValue(dataReader["$type"]);

            if (IsPrimitive(ContentType))
            {
                obj = ReadPrimitive(dataReader["m_value"], ContentType);
            }
            else if (IsArray(ContentType))
            {
                obj = ReadArray(dataReader["m_value"], ContentType);
            }
            else if (IsSerializable(ContentType))
            {
                dataReader.OnReadAll(this);
            }
            else
            {
                var fields = GetOrCreateXTypeInfo().fields;

                for (int i = 0; i < fields.Count; i++)
                {
                    var field = fields[i].Value;
                    var name = field.Name;

                    StoreField(dataReader[name], GetOrCreateContent(), field);
                }
            }
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            if (!isInitialized)
            {
                throw new NullReferenceException();
            }

            if (key == "$type" && actualType is null)
            {
                actualType = ValueInterface<Type>.ReadValue(valueReader);

                return;
            }

            if (key == "m_value")
            {
                if (IsPrimitive(ContentType))
                {
                    obj = ReadPrimitive(valueReader, ContentType);

                    return;
                }
                else if (IsArray(ContentType))
                {
                    obj = ReadArray(valueReader, ContentType);

                    return;
                }
            }

            if (IsSerializable(ContentType))
            {
                GetOrCreateSerializationInfo().AddValue(key, ReadSerializableItem(valueReader));
            }
            else
            {
                StoreField(valueReader, GetOrCreateContent(), GetOrCreateXTypeInfo().GetField(key));
            }
        }

        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            if (obj != null && actualType != null)
            {
                if (key == "$type" && actualType != declaringType)
                {
                    ValueInterface.WriteValue(valueWriter, actualType);

                    return;
                }

                if (key == "m_value")
                {
                    if (IsPrimitive(actualType))
                    {
                        WritePrimitive(valueWriter, obj, actualType);

                        return;
                    }
                    else if (IsArray(actualType))
                    {
                        WriteArray(valueWriter, obj, actualType);

                        return;
                    }
                }

                if (IsSerializable(actualType))
                {
                    WriteSerializableItem(valueWriter, GetOrCreateSerializationInfo().GetValue(key, typeof(object)));
                }
                else
                {
                    LoadField(valueWriter, obj, GetOrCreateXTypeInfo().GetField(key));
                }
            }
        }

        public void OnReadAll(IDataWriter<string> dataWriter)
        {
            if (obj != null && actualType != null)
            {
                if (actualType != declaringType)
                {
                    ValueInterface.WriteValue(dataWriter["$type"], actualType);
                }

                if (IsPrimitive(actualType))
                {
                    WritePrimitive(dataWriter["m_value"], obj, actualType);
                }
                else if (IsArray(actualType))
                {
                    WriteArray(dataWriter["m_value"], obj, actualType);
                }
                else if (IsSerializable(actualType))
                {
                    foreach (var item in GetOrCreateSerializationInfo())
                    {
                        WriteSerializableItem(dataWriter[item.Name], item.Value);
                    }
                }
                else
                {
                    var fields = GetOrCreateXTypeInfo().fields;

                    for (int i = 0; i < fields.Count; i++)
                    {
                        var field = fields[i].Value;
                        var name = field.Name;

                        LoadField(dataWriter[name], obj, field);
                    }
                }
            }


        }
    }
}