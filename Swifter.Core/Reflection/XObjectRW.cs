
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// XObjectRW 一个强大，高效，内存小的对象读写器。
    /// </summary>
    public sealed class XObjectRW : IObjectRW
    {
        /// <summary>
        /// 读取或设置默认的绑定标识。
        /// </summary>
        public static XBindingFlags DefaultBindingFlags { get; set; } =
            XBindingFlags.Property |
            XBindingFlags.Field |
            XBindingFlags.Public |
            XBindingFlags.Instance |
            XBindingFlags.InheritedMembers |
            //XBindingFlags.RWCannotGetException |
            //XBindingFlags.RWCannotSetException |
            //XBindingFlags.RWNotFoundException |
            XBindingFlags.RWIgnoreCase;


        internal static XBindingFlags GetDefaultBindingFlags(Type type)
        {
            if (type.IsRecordType() || type.IsTupleType())
            {
                return DefaultBindingFlags | XBindingFlags.RWAllocate | XBindingFlags.RWAutoPropertyDirectRW;
            }

            return DefaultBindingFlags;
        }

        /// <summary>
        /// 创建 XObjectRW 对象读写器。
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回 XObjectRW 对象读写器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static XObjectRW Create(Type type, XBindingFlags flags = XBindingFlags.UseDefault)
        {
            return new XObjectRW(XTypeInfo.Create(type, flags | XTypeInfo.Flags_RW));
        }

        /// <summary>
        /// 读取或设置默认的绑定标识。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回 XObjectRW 对象读写器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static XObjectRW Create<T>(XBindingFlags flags = XBindingFlags.UseDefault)
        {
            return new XObjectRW(XTypeInfo.Create<T>(flags | XTypeInfo.Flags_RW));
        }

        /// <summary>
        /// 获取对象读写器所使用的 XTypeInfo 实例。
        /// </summary>
        public readonly XTypeInfo XTypeInfo;

        internal object? content;
        
        internal XObjectRW(XTypeInfo xTypeInfo)
        {
            XTypeInfo = xTypeInfo;
        }

        /// <summary>
        /// 获取指定成员名称的成员值的读写器。
        /// </summary>
        /// <param name="key">成员名称</param>
        /// <returns>返回值的读写器</returns>
        public IValueRW this[string key]
        {
            get
            {
                var fields = XTypeInfo.rwFields;

                var index = fields.FindIndex(key);

                if (index >= 0)
                {
                    return fields[index].Value.CreateValueRW(this);
                }

                if (XTypeInfo.flags.On(XBindingFlags.RWNotFoundException))
                {
                    throw new MissingMemberException(XTypeInfo.type.Name, key);
                }

                return RWHelper.DefaultValueRW;
            }
        }

        /// <summary>
        /// 将指定成员名称的值写入到值写入器中。
        /// </summary>
        /// <param name="key">成员的名称</param>
        /// <param name="valueWriter">值写入器</param>
        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var index = XTypeInfo.rwFields.FindIndex(key);

            if (index >= 0)
            {
                XTypeInfo.rwFields[index].Value.OnReadValue(content, valueWriter);
            }
            else if (XTypeInfo.flags.On(XBindingFlags.RWNotFoundException))
            {
                throw new MissingMemberException(XTypeInfo.type.Name, key);
            }
            else
            {
                valueWriter.DirectWrite(null);
            }
        }

        /// <summary>
        /// 将数据源中的所有成员的名称和值写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="stopToken">停止令牌</param>
        public void OnReadAll(IDataWriter<string> dataWriter, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var fields = XTypeInfo.rwFields;

            int i = 0;

            if (stopToken.CanBeStopped)
            {
                if (stopToken.PopState() is int index)
                {
                    i = index;
                }

                for (; i < fields.Count; i++)
                {
                    if (stopToken.IsStopRequested)
                    {
                        stopToken.SetState(i);

                        return;
                    }

                    fields[i].Value.OnReadAll(content, dataWriter);
                }
            }
            else
            {
                for (; i < fields.Count; i++)
                {
                    fields[i].Value.OnReadAll(content, dataWriter);
                }
            }
        }

        /// <summary>
        /// 将数据读取器中的值设置到指定名称的成员中。
        /// </summary>
        /// <param name="key">成员的名称</param>
        /// <param name="valueReader">值读取器</param>
        public void OnWriteValue(string key, IValueReader valueReader)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var index = XTypeInfo.rwFields.FindIndex(key);

            if (index >= 0)
            {
                XTypeInfo.rwFields[index].Value.OnWriteValue(content, valueReader);
            }
            else if (XTypeInfo.flags.On(XBindingFlags.RWNotFoundException))
            {
                throw new MissingMemberException(XTypeInfo.type.Name, key);
            }
            else
            {
                valueReader.Pop();
            }
        }

        /// <summary>
        /// 从数据读取器中读取所有数据源字段到数据源的值
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="stopToken">停止令牌</param>
        public void OnWriteAll(IDataReader<string> dataReader, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var fields = XTypeInfo.rwFields;

            int i = 0;

            if (stopToken.CanBeStopped)
            {
                if (stopToken.PopState() is int index)
                {
                    i = index;
                }

                for (; i < fields.Count; i++)
                {
                    if (stopToken.IsStopRequested)
                    {
                        stopToken.SetState(i);

                        return;
                    }

                    fields[i].Value.OnWriteAll(content, dataReader);
                }
            }
            else
            {
                for (; i < fields.Count; i++)
                {
                    fields[i].Value.OnWriteAll(content, dataReader);
                }
            }
        }

        /// <summary>
        /// 调用默认构造函数初始化数据源对象。
        /// </summary>
        public void Initialize()
        {
            if (XTypeInfo.flags.On(XBindingFlags.RWAllocate))
            {
                content = TypeHelper.Allocate(XTypeInfo.type);
            }
            else
            {
                content = Activator.CreateInstance(XTypeInfo.type);
            }
        }

        /// <summary>
        /// 调用默认构造函数初始化数据源对象。
        /// </summary>
        /// <param name="capacity">不处理此参数</param>
        public void Initialize(int capacity)
        {
            Initialize();
        }

        /// <summary>
        /// 获取指定索引处的字段名称。
        /// </summary>
        /// <param name="ordinal">指定索引</param>
        /// <returns>返回字段名称</returns>
        public string GetKey(int ordinal)
        {
            return XTypeInfo.rwFields[ordinal].Key;
        }

        /// <summary>
        /// 获取指定字段名称的序号。
        /// </summary>
        /// <param name="key">指定字段名称</param>
        /// <returns>返回序号</returns>
        public int GetOrdinal(string key)
        {
            return XTypeInfo.rwFields.FindIndex(key);
        }

        /// <summary>
        /// 获取该对象读写器的成员名称的数量
        /// </summary>
        public int Count => XTypeInfo.rwFields.Count;

        /// <summary>
        /// 获取数据源。
        /// </summary>
        public object? Content
        {
            get => content;
            set => content
                = value is null || ContentType.IsInstanceOfType(value)
                ? value
                : throw new ArgumentException($"The value is not instance of '{ContentType}'.", nameof(value));
        }

        /// <summary>
        /// 获取数据源类型。
        /// </summary>
        public Type ContentType => XTypeInfo.type;

        /// <summary>
        /// 此值对此读写器无效。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Type? ValueType => null;

        /// <summary>
        /// 成员名称集合。
        /// </summary>
        public IEnumerable<string> Keys => XTypeInfo.rwKeys;

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];
    }
}