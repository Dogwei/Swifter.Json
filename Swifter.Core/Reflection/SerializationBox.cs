using Swifter.RW;
using Swifter.Tools;
using System;
using System.Reflection;
using System.Runtime.Serialization;
namespace Swifter.Reflection
{
    /// <summary>
    /// 提供对象序列化的工具方法。
    /// </summary>
    public static class SerializationBox
    {
        internal static readonly IFormatterConverter DefaultFormatterConverter = new ValueInterfaceFormatterConverter();

        internal static SerializationInfo CreateSerializationInfo(object obj)
        {
            var serializationInfo = new SerializationInfo(obj.GetType(), DefaultFormatterConverter);
            var streamingContext = new StreamingContext(StreamingContextStates.All);

            Underlying.As<ISerializable>(obj).GetObjectData(serializationInfo, streamingContext);

            return serializationInfo;
        }

        internal static SerializationInfo CreateSerializationInfo(Type type)
        {
            return new SerializationInfo(type, DefaultFormatterConverter);
        }

        internal static object CallConstructor(object obj, SerializationInfo serializationInfo)
        {
            var type = obj.GetType();

            var constructor = type.GetConstructor(
                BindingFlags.Public | 
                BindingFlags.NonPublic | 
                BindingFlags.Instance, 
                Type.DefaultBinder, 
                new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }, 
                null);

            if (constructor is null)
            {
                throw new NotSupportedException("Missing deserialization constructor function.");
            }

            var pFunc = constructor.MethodHandle.GetFunctionPointer();

            var streamingContext = new StreamingContext(StreamingContextStates.All);

            if (type.IsValueType)
            {
                unsafe
                {
                    fixed(byte* ptr = &TypeHelper.Unbox<byte>(obj))
                    {
                        Underlying.Call((IntPtr)ptr, serializationInfo, streamingContext, pFunc);
                    }
                }
            }
            else
            {
                Underlying.Call(obj, serializationInfo, streamingContext, pFunc);
            }

            if (obj is IObjectReference objRef)
            {
                obj = objRef.GetRealObject(streamingContext);
            }

            return obj;
        }

        internal static Type GetArrayValueType(Type type)
        {
            var elementType = type.GetElementType();

            var rank = type.GetArrayRank();

            var isMultDimArray = rank != 1 || elementType.MakeArrayType() != type;

            var itemType = typeof(SerializationBox<>).MakeGenericType(elementType);

            if (isMultDimArray)
            {
                return itemType.MakeArrayType(rank);
            }
            else
            {
                return itemType.MakeArrayType();
            }
        }

        internal static bool IsArray(Type type)
        {
            return type.IsArray;
        }

        internal static void WriteArray(IValueWriter valueWriter, object value, Type type)
        {
            var items = Underlying.As<Array>(value).Clone();

            Underlying.GetMethodTablePointer(items) = TypeHelper.GetMethodTablePointer(GetArrayValueType(type));

            ValueInterface.WriteValue(valueWriter, items);
        }

        internal static object ReadArray(IValueReader valueReader, Type type)
        {
            var items = ValueInterface.ReadValue(valueReader, GetArrayValueType(type));

            Underlying.GetMethodTablePointer(items) = TypeHelper.GetMethodTablePointer(type);

            return items;
        }

        internal static bool IsPrimitive(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.String:
                    return true;
            }

            if (type.IsPointer)
            {
                return true;
            }

            return false;
        }

        internal static void WritePrimitive(IValueWriter valueWriter, object value, Type type)
        {
            ValueInterface.WriteValue(valueWriter, value);
        }

        internal static object ReadPrimitive(IValueReader valueReader, Type type)
        {
            return ValueInterface.ReadValue(valueReader, type);
        }

        internal static bool IsPointer(Type type)
        {
            return /*type == typeof(IntPtr) || type == typeof(UIntPtr) || */type.IsPointer;
        }

        internal static bool IsSerializable(Type type)
        {
            return typeof(ISerializable).IsAssignableFrom(type);
        }

        internal static bool IsDeserializationCallback(Type type)
        {
            return typeof(IDeserializationCallback).IsAssignableFrom(type);
        }

        internal static void LoadField(IValueWriter valueWriter, object obj, XFieldInfo field)
        {
            var value = field.GetValue(obj);
            var type = field.FieldInfo.FieldType;

            Write(valueWriter, value, type);
        }

        internal static void StoreField(IValueReader valueReader, object obj, XFieldInfo field)
        {
            var type = field.FieldInfo.FieldType;
            var value = Read(valueReader, type);

            field.SetValue(obj, value);
        }

        internal static void WriteSerializableItem(IValueWriter valueWriter, object value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var actualType = value.GetType();

            if (IsPrimitive(actualType))
            {
                WritePrimitive(valueWriter, value, actualType);
            }
            else
            {
                Write(valueWriter, value, typeof(object));
            }

        }

        internal static object ReadSerializableItem(IValueReader valueReader)
        {
            return Read(valueReader, typeof(object));
        }

        /// <summary>
        /// 写入入口。
        /// </summary>
        internal static void Write(IValueWriter valueWriter, object value, Type declaringType)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var actualType = value.GetType();

            if (IsPointer(actualType))
            {
                throw new NotSupportedException("Pointer are not supported");
            }

            if (declaringType == actualType)
            {
                if (IsPrimitive(actualType))
                {
                    WritePrimitive(valueWriter, value, actualType);

                    return;
                }

                if (IsArray(actualType))
                {
                    WriteArray(valueWriter, value, actualType);

                    return;
                }
            }

            valueWriter.WriteObject(new SerializationRW(value, declaringType));
        }

        /// <summary>
        /// 读取入口。
        /// </summary>
        internal static object Read(IValueReader valueReader, Type declaringType)
        {
            if (IsPointer(declaringType))
            {
                throw new NotSupportedException("Pointer are not supported");
            }

            if (IsPrimitive(declaringType))
            {
                return ReadPrimitive(valueReader, declaringType);
            }

            if (IsArray(declaringType))
            {
                return ReadArray(valueReader, declaringType);
            }

            var writer = new SerializationRW(declaringType);

            valueReader.ReadObject(writer);

            return writer.GetContent(true);
        }
    }
}