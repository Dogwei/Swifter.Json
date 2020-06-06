
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// XObjectRW 一个强大，高效，内存小的对象读写器。
    /// </summary>
    public sealed class XObjectRW : IDataRW<string>
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


        private static XBindingFlags GetDefaultBindingFlags(Type type)
        {
            // 对匿名类型进行 Allocate 和 AutoPropertyDirectRW
            if (type.IsDefined(typeof(CompilerGeneratedAttribute), false))
            {
                return DefaultBindingFlags | XBindingFlags.RWAllocate | XBindingFlags.RWAutoPropertyDirectRW;
            }

            // 对元组类型进行 Allocate 和 AutoPropertyDirectRW
            if (type.IsClass && type.IsGenericType && type.Name.Contains("Tuple") && type.GetConstructor(Type.EmptyTypes) is null)
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
            if (flags == XBindingFlags.UseDefault)
            {
                flags = GetDefaultBindingFlags(type);
            }

            return new XObjectRW(XTypeInfo.Create(type, flags | XTypeInfo.RW));
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
            if (flags == XBindingFlags.UseDefault)
            {
                flags = GetDefaultBindingFlags(typeof(T));
            }

            return new XObjectRW(XTypeInfo.Create<T>(flags | XTypeInfo.RW));
        }

        /// <summary>
        /// 获取对象读写器所使用的 XTypeInfo 实例。
        /// </summary>
        public readonly XTypeInfo xTypeInfo;

        internal object obj;
        
        internal XObjectRW(XTypeInfo xTypeInfo)
        {
            this.xTypeInfo = xTypeInfo;
        }

        /// <summary>
        /// 获取指定成员名称的成员值的读写器。
        /// </summary>
        /// <param name="key">成员名称</param>
        /// <returns>返回值的读写器</returns>
        public XFieldValueRW this[string key]
        {
            get
            {
                if (xTypeInfo.rwFields.TryGetValue(key, out var field))
                {
                    return new XFieldValueRW(obj, field);
                }

                if ((xTypeInfo.flags & XBindingFlags.RWNotFoundException) != 0)
                {
                    throw new MissingMemberException(xTypeInfo.type.Name, key);
                }

                return null;
            }
        }

        /// <summary>
        /// 将指定成员名称的值写入到值写入器中。
        /// </summary>
        /// <param name="key">成员的名称</param>
        /// <param name="valueWriter">值写入器</param>
        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            if (xTypeInfo.rwFields.TryGetValue(key, out var field))
            {
                if (field.CanRead)
                {
                    field.OnReadValue(obj, valueWriter);
                }
                else if (field.CannotGetException)
                {
                    throw new MissingMethodException($"Property '{xTypeInfo.type.Name}.{key}' No define '{"get"}' method or cannot access.");
                }
            }
            else if ((xTypeInfo.flags & XBindingFlags.RWNotFoundException) != 0)
            {
                throw new MissingMemberException(xTypeInfo.type.Name, key);
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
        public void OnReadAll(IDataWriter<string> dataWriter)
        {
            if ((xTypeInfo.flags & XBindingFlags.RWMembersOptIn) != 0)
            {
                MembersOptIn();
            }
            else
            {
                None();
            }

            void MembersOptIn()
            {
                var vc = new ValueCopyer();
                var fields = xTypeInfo.rwFields;

                for (int i = 0; i < fields.Count; i++)
                {
                    if (fields[i].Value is XAttributedFieldRW fieldRW && fieldRW.CanRead)
                    {
                        if (fieldRW.SkipDefaultValue)
                        {
                            fieldRW.OnReadValue(obj, vc);

                            if (!vc.IsEmptyValue())
                            {
                                vc.WriteTo(dataWriter[fieldRW.Name]);
                            }
                        }
                        else
                        {
                            fieldRW.OnReadValue(obj, dataWriter[fieldRW.Name]);
                        }
                    }
                }
            }

            void None()
            {
                var vc = new ValueCopyer();
                var fields = xTypeInfo.rwFields;

                for (int i = 0; i < fields.Count; i++)
                {
                    var item = fields[i].Value;

                    if (item.CanRead)
                    {
                        if (item.SkipDefaultValue)
                        {
                            item.OnReadValue(obj, vc);

                            if (!vc.IsEmptyValue())
                            {
                                vc.WriteTo(dataWriter[item.Name]);
                            }
                        }
                        else
                        {
                            item.OnReadValue(obj, dataWriter[item.Name]);
                        }
                    }
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
            if (xTypeInfo.rwFields.TryGetValue(key, out var field))
            {
                if (field.CanWrite)
                {
                    field.OnWriteValue(obj, valueReader);
                }
                else if (field.CannotSetException)
                {
                    throw new MissingMethodException($"Property '{xTypeInfo.type.Name}.{key}' No define '{"set"}' method or cannot access.");
                }
            }
            else if ((xTypeInfo.flags & XBindingFlags.RWNotFoundException) != 0)
            {
                throw new MissingMemberException(xTypeInfo.type.Name, key);
            }
            else
            {
                valueReader.DirectRead();
            }
        }

        /// <summary>
        /// 调用默认构造函数初始化数据源对象。
        /// </summary>
        public void Initialize()
        {
            if ((xTypeInfo.flags & XBindingFlags.RWAllocate) != 0)
            {
                obj = TypeHelper.Allocate(xTypeInfo.type);
            }
            else
            {
                obj = Activator.CreateInstance(xTypeInfo.type);
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
        /// 初始化数据源。
        /// </summary>
        /// <param name="obj">数据源。</param>
        public void Initialize(object obj)
        {
            if (obj != null && !xTypeInfo.type.IsInstanceOfType(obj))
            {
                throw new ArgumentException(nameof(obj));
            }

            this.obj = obj;
        }

        /// <summary>
        /// 从数据读取器中读取所有数据源字段到数据源的值
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        public void OnWriteAll(IDataReader<string> dataReader)
        {
            var fields = xTypeInfo.rwFields;

            for (int i = 0; i < fields.Count; i++)
            {
                var item = fields[i].Value;

                if (item.CanWrite)
                {
                    item.OnWriteValue(obj, dataReader[item.Name]);
                }
            }
        }

        /// <summary>
        /// 获取指定索引处的字段名称。
        /// </summary>
        /// <param name="ordinal">指定索引</param>
        /// <returns>返回字段名称</returns>
        public string GetKey(int ordinal)
        {
            return xTypeInfo.rwFields[ordinal].Key;
        }

        /// <summary>
        /// 获取指定字段名称的序号。
        /// </summary>
        /// <param name="key">指定字段名称</param>
        /// <returns>返回序号</returns>
        public int GetOrdinal(string key)
        {
            return xTypeInfo.rwFields.FindIndex(key);
        }


        /// <summary>
        /// 获取该对象读写器的成员名称集合。
        /// </summary>
        public IEnumerable<string> Keys
        {
            get
            {
                var fields = xTypeInfo.rwFields;

                for (int i = 0; i < fields.Count; i++)
                {
                    yield return fields[i].Key;
                }
            }
        }

        /// <summary>
        /// 获取该对象读写器的成员名称的数量
        /// </summary>
        public int Count => xTypeInfo.rwFields.Count;

        /// <summary>
        /// 获取数据源。
        /// </summary>
        public object Content
        {
            get => obj;
            set => obj 
                = ContentType.IsInstanceOfType(value)
                ? value
                : throw new ArgumentException($"The value is not instance of '{ContentType}'.", nameof(value));
        }

        

        /// <summary>
        /// 获取数据源类型。
        /// </summary>
        public Type ContentType => xTypeInfo.type;

        IValueRW IDataRW<string>.this[string key] => this[key];

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];
    }
}