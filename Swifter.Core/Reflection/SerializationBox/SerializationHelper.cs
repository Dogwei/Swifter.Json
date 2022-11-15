using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Swifter.Reflection
{
    /// <summary>
    /// 提供对象序列化的工具方法。
    /// </summary>
    public static class SerializationHelper
    {
        internal const string TypeFieldName = "$type";
        internal const string ReferenceFieldName = "$ref";
        internal const string ValueFieldName = "$value";
        internal const string LowerBoundsFieldName = "$lowerBounds";
        internal const string LengthFieldName = "$length";
        internal const string LengthsFieldName = "$lengths";

        [ThreadStatic]
        static SerializationContext? ThreadSerializationContext;

        [ThreadStatic]
        static DeserializationContext? ThreadDeserializationContext;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="valueWriter"></param>
        /// <param name="value"></param>
        public static void WriteValue<TValue>(IValueWriter valueWriter, TValue? value)
        {
            // TODO: 优化性能
            WriteValue(valueWriter, typeof(TValue), value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="valueReader"></param>
        /// <returns></returns>
        public static TValue? ReadValue<TValue>(IValueReader valueReader)
        {
            // TODO: 优化性能
            return (TValue?)ReadValue(valueReader, typeof(TValue));
        }

        internal static void WriteValue(IValueWriter valueWriter, Type? declaringType, object? value)
        {
            var isRoot = EntrySerializationContext();

            try
            {
                if (declaringType is not null && typeof(Type).IsAssignableFrom(declaringType))
                {
                    ValueInterface<Type>.WriteValue(valueWriter, (Type?)value);

                    return;
                }

                if (declaringType?.IsValueType != true && TryWriteSerializationReference(valueWriter, declaringType, value))
                {
                    return;
                }

                if (value is null)
                {
                    valueWriter.DirectWrite(null);

                    return;
                }

                if (declaringType is not null)
                {
                    // TODO: DateTimeOffset, TimeSpan, Guid
                    switch (Type.GetTypeCode(declaringType))
                    {
                        case TypeCode.Boolean:
                            valueWriter.WriteBoolean((bool)value);
                            return;
                        case TypeCode.Char:
                            valueWriter.WriteChar((char)value);
                            return;
                        case TypeCode.SByte:
                            valueWriter.WriteSByte((sbyte)value);
                            return;
                        case TypeCode.Byte:
                            valueWriter.WriteByte((byte)value);
                            return;
                        case TypeCode.Int16:
                            valueWriter.WriteInt16((short)value);
                            return;
                        case TypeCode.UInt16:
                            valueWriter.WriteUInt16((ushort)value);
                            return;
                        case TypeCode.Int32:
                            valueWriter.WriteInt32((int)value);
                            return;
                        case TypeCode.UInt32:
                            valueWriter.WriteUInt32((uint)value);
                            return;
                        case TypeCode.Int64:
                            valueWriter.WriteInt64((long)value);
                            return;
                        case TypeCode.UInt64:
                            valueWriter.WriteUInt64((ulong)value);
                            return;
                        case TypeCode.Single:
                            valueWriter.WriteSingle((float)value);
                            return;
                        case TypeCode.Double:
                            valueWriter.WriteDouble((double)value);
                            return;
                        case TypeCode.Decimal:
                            valueWriter.WriteDecimal((decimal)value);
                            return;
                        case TypeCode.String:
                            valueWriter.WriteString((string)value);
                            return;
                    }
                }

                valueWriter.WriteObject(new SerializationReader(declaringType, value));
            }
            finally
            {
                LeaveSerializationContext(isRoot);
            }
        }

        internal static object? ReadValue(IValueReader valueReader, Type? declaringType)
        {
            var isRoot = EntryDeserializationContext();

            try
            {
                if (declaringType is not null)
                {
                    if (typeof(Type).IsAssignableFrom(declaringType))
                    {
                        return ValueInterface<Type>.ReadValue(valueReader);
                    }

                    // TODO: DateTimeOffset, TimeSpan, Guid
                    switch (Type.GetTypeCode(declaringType))
                    {
                        case TypeCode.Boolean:
                            return valueReader.ReadBoolean();
                        case TypeCode.Char:
                            return valueReader.ReadChar();
                        case TypeCode.SByte:
                            return valueReader.ReadSByte();
                        case TypeCode.Byte:
                            return valueReader.ReadByte();
                        case TypeCode.Int16:
                            return valueReader.ReadInt16();
                        case TypeCode.UInt16:
                            return valueReader.ReadUInt16();
                        case TypeCode.Int32:
                            return valueReader.ReadInt32();
                        case TypeCode.UInt32:
                            return valueReader.ReadUInt32();
                        case TypeCode.Int64:
                            return valueReader.ReadInt64();
                        case TypeCode.UInt64:
                            return valueReader.ReadUInt64();
                        case TypeCode.Single:
                            return valueReader.ReadSingle();
                        case TypeCode.Double:
                            return valueReader.ReadDouble();
                        case TypeCode.Decimal:
                            return valueReader.ReadDecimal();
                        case TypeCode.String:

                            if (valueReader.ValueType is Type valueType && Type.GetTypeCode(valueType) is TypeCode.String)
                            {
                                var value = valueReader.ReadString();

                                StoreDeserializationReferenceContent(value);

                                return value;
                            }

                            break;
                    }
                }

                var serializationWriter = new SerializationWriter(declaringType);

                if (declaringType?.IsValueType != true)
                {
                    StoreDeserializationReferenceWriter(serializationWriter);
                }

                valueReader.ReadObject(serializationWriter);

                return serializationWriter.Content;
            }
            finally
            {
                LeaveDeserializationContext(isRoot);
            }
        }

        internal static void WriteSerializationItemValue(IValueWriter valueWriter, object? value)
        {
            if (TryWriteSerializationReference(valueWriter, null, value))
            {
                return;
            }

            if (value is null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (value is IConvertible convertible)
            {
                // TODO: DateTimeOffset, TimeSpan, Guid
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        valueWriter.WriteBoolean((bool)value);
                        return;
                    case TypeCode.Char:
                        valueWriter.WriteChar((char)value);
                        return;
                    case TypeCode.SByte:
                        valueWriter.WriteSByte((sbyte)value);
                        return;
                    case TypeCode.Byte:
                        valueWriter.WriteByte((byte)value);
                        return;
                    case TypeCode.Int16:
                        valueWriter.WriteInt16((short)value);
                        return;
                    case TypeCode.UInt16:
                        valueWriter.WriteUInt16((ushort)value);
                        return;
                    case TypeCode.Int32:
                        valueWriter.WriteInt32((int)value);
                        return;
                    case TypeCode.UInt32:
                        valueWriter.WriteUInt32((uint)value);
                        return;
                    case TypeCode.Int64:
                        valueWriter.WriteInt64((long)value);
                        return;
                    case TypeCode.UInt64:
                        valueWriter.WriteUInt64((ulong)value);
                        return;
                    case TypeCode.Single:
                        valueWriter.WriteSingle((float)value);
                        return;
                    case TypeCode.Double:
                        valueWriter.WriteDouble((double)value);
                        return;
                    case TypeCode.Decimal:
                        valueWriter.WriteDecimal((decimal)value);
                        return;
                    case TypeCode.String:
                        valueWriter.WriteString((string)value);
                        return;
                }
            }

            valueWriter.WriteObject(new SerializationReader(null, value));
        }

        internal static object? ReadSerializationItemValue(IValueReader valueReader)
        {
            var serializationWriter = new SerializationWriter(null);

            StoreDeserializationReferenceWriter(serializationWriter);

            valueReader.ReadObject(serializationWriter);

            return serializationWriter.Content;
        }

        internal static SerializationType GetSerializationType(Type type)
        {
            if (typeof(Type).IsAssignableFrom(type))
            {
                return SerializationType.Primitive;
            }


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
                case TypeCode.Decimal:
                case TypeCode.String:
                    return SerializationType.Primitive;
            }

            if (type.IsArray)
            {
                return SerializationType.Array;
            }

            if (typeof(ISerializable).IsAssignableFrom(type))
            {
                return SerializationType.Serializable;
            }

            return SerializationType.Object;
        }

        internal static bool EntrySerializationContext()
        {
            ref var serializationContext = ref ThreadSerializationContext;

            if (serializationContext is null)
            {
                serializationContext = new SerializationContext();

                return true;
            }

            return false;
        }

        internal static void LeaveSerializationContext(bool isRoot)
        {
            ref var serializationContext = ref ThreadSerializationContext;

            if (serializationContext is null)
            {
                throw new NotSupportedException();
            }

            if (isRoot)
            {
                serializationContext = null;
            }
        }

        internal static bool EntryDeserializationContext()
        {
            ref var deserializationContext = ref ThreadDeserializationContext;

            if (deserializationContext is null)
            {
                deserializationContext = new DeserializationContext();

                return true;
            }

            return false;
        }

        internal static void LeaveDeserializationContext(bool isRoot)
        {
            ref var deserializationContext = ref ThreadDeserializationContext;

            if (deserializationContext is null)
            {
                throw new NotSupportedException();
            }

            if (isRoot)
            {
                while (deserializationContext.DeserializationCallbacks != null)
                {
                    deserializationContext.DeserializationCallbacks.Value.OnDeserialization(deserializationContext.StreamingContext);

                    deserializationContext.DeserializationCallbacks = deserializationContext.DeserializationCallbacks.Next;
                }

                deserializationContext = null;
            }
        }

        internal static void AddDeserializationCallback(IDeserializationCallback deserializationCallback)
        {
            ref var deserializationContext = ref ThreadDeserializationContext;

            if (deserializationContext is null)
            {
                throw new NotSupportedException();
            }

            deserializationContext.DeserializationCallbacks = new SinglyLinkedNode<IDeserializationCallback>(deserializationCallback, deserializationContext.DeserializationCallbacks);
        }

        internal static bool TryWriteSerializationReference(IValueWriter valueWriter, Type? declaringType, object? value)
        {
            var serializationContext = ThreadSerializationContext;

            if (serializationContext is null)
            {
                throw new NotSupportedException();
            }

            if (value is null)
            {
                ++serializationContext.ReferenceOffset;

                return false;
            }
            else if (serializationContext.ReferenceMap.TryGetValue(value, out var targetIndex))
            {
                ++serializationContext.ReferenceOffset;

                valueWriter.WriteObject(new SerializationReader(declaringType, value, new SerializationReferenceInfo(targetIndex)));

                return true;
            }
            else
            {
                serializationContext.ReferenceMap.Add(value, serializationContext.ReferenceOffset);

                ++serializationContext.ReferenceOffset;

                return false;
            }
        }

        internal static object GetDeserializationReferenceTargetObject(SerializationReferenceInfo info)
        {
            var deserializationContext = ThreadDeserializationContext;

            if (deserializationContext is null)
            {
                throw new NotSupportedException();
            }

            var result = deserializationContext.ReferenceMap[info.TargetIndex];

            if (result is SerializationWriter serializationWriter)
            {
                result = serializationWriter.Content;
            }

            if (result is null)
            {
                throw new NullReferenceException();
            }

            return result;
        }

        internal static void StoreDeserializationReferenceContent(object? value)
        {
            var deserializationContext = ThreadDeserializationContext;

            if (deserializationContext is null)
            {
                throw new NotSupportedException();
            }

            deserializationContext.ReferenceMap.Add(value);
        }

        internal static void StoreDeserializationReferenceWriter(SerializationWriter value)
        {
            StoreDeserializationReferenceContent(value);
        }

        internal static StreamingContext CurrentSerializationStreamingContext => ThreadSerializationContext?.StreamingContext ?? throw new NotSupportedException();

        internal static StreamingContext CurrentDeserializationStreamingContext => ThreadDeserializationContext?.StreamingContext ?? throw new NotSupportedException();

        sealed class SerializationContext
        {
            public readonly Dictionary<object, int> ReferenceMap = new (ReferenceEqualityComparer.Instance);
            public int ReferenceOffset;

            StreamingContext? streamingContext;

            public StreamingContext StreamingContext => streamingContext ??= new StreamingContext(StreamingContextStates.All);
        }

        sealed class DeserializationContext
        {
            public readonly List<object?> ReferenceMap = new();

            StreamingContext? streamingContext;

            public SinglyLinkedNode<IDeserializationCallback>? DeserializationCallbacks;

            public StreamingContext StreamingContext => streamingContext ??= new StreamingContext(StreamingContextStates.All);
        }
    }
}