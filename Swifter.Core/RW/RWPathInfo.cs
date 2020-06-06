using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    /// <summary>
    /// 表示对象路径信息。
    /// </summary>
    public abstract class RWPathInfo : IEquatable<RWPathInfo>
    {
        /// <summary>
        /// 表示根路径对象。
        /// </summary>
        public static readonly RWPathInfo Root = new Impl<string>(null, null);

        /// <summary>
        /// 根路径名称。
        /// </summary>
        public static string RootToken = "#";

        /// <summary>
        /// 路径分隔符。
        /// </summary>
        public static string PathSeparator = "/";

        /// <summary>
        /// 创建一个对象路径信息。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="parent">父级路径</param>
        /// <returns>返回一个新的路径信息</returns>
        public static RWPathInfo Create<TKey>(TKey key, RWPathInfo parent = null) => new Impl<TKey>(key, parent ?? Root);

        /// <summary>
        /// 设置路径键。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <param name="path">要设置的路径</param>
        /// <param name="key">键</param>
        public static void SetPath<TKey>(in RWPathInfo path, TKey key)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path is Impl<TKey> impl)
            {
                impl.Key = key;
            }
            else
            {
                Underlying.AsRef(path) = Create(key, path.Parent);
            }
        }

        /// <summary>
        /// 父级路径。
        /// </summary>
        public readonly RWPathInfo Parent;

        RWPathInfo(RWPathInfo parent)
        {
            if (parent is null && Root != null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            Parent = parent;
        }

        /// <summary>
        /// 是否为根路径。
        /// </summary>
        public bool IsRoot => this == Root;

        /// <summary>
        /// 获取键。
        /// </summary>
        /// <returns></returns>
        public abstract object GetKey();

        /// <summary>
        /// 获取一个数据读取器该路径的值读取器。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>返回一个值读取器</returns>
        public abstract IValueReader GetValueReader(IDataReader dataReader);

        /// <summary>
        /// 设置一个数据写入器该路径的值写入器。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <returns>返回一个值写入器</returns>
        public abstract IValueWriter GetValueWriter(IDataWriter dataWriter);
        
        /// <summary>
        /// 让数据读取器读取该路径的值到写入器中。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="valueWriter">值写入器</param>
        /// <returns>返回是否到达</returns>
        public abstract bool OnReadValue(IDataReader dataReader, IValueWriter valueWriter);

        /// <summary>
        /// 让数据写入器写入读取器中的值到该路径的值中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回是否到达</returns>
        public abstract bool OnWriteValue(IDataWriter dataWriter, IValueReader valueReader);

        /// <summary>
        /// 获取数据读取器的值的数据读取器。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>返回数据读取器</returns>
        protected abstract IDataReader GetDataReader(IDataReader dataReader);

        /// <summary>
        /// 获取数据写入器的值的数据写入器。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <returns>返回数据读取器</returns>
        protected abstract IDataWriter GetDataWriter(IDataWriter dataWriter);

        /// <summary>
        /// 创建副本。
        /// </summary>
        /// <returns>返回克隆实例</returns>
        public abstract RWPathInfo Clone();

        /// <summary>
        /// 比较当前路径是否与指定的路径相同。
        /// </summary>
        /// <param name="other">指定的路径</param>
        /// <returns>返回一个 bool 值</returns>
        public abstract bool Equals(RWPathInfo other);

        sealed class Impl<TKey> : RWPathInfo, IEquatable<Impl<TKey>>
        {
            public static readonly IEqualityComparer<TKey> EqualityComparer = EqualityComparer<TKey>.Default;

            public TKey Key;

            public Impl(TKey key, RWPathInfo parent) : base(parent)
            {
                Key = key;
            }

            public override RWPathInfo Clone()
            {
                if (IsRoot)
                {
                    return this;
                }

                return new Impl<TKey>(Key, Parent.Clone());
            }

            public bool Equals(Impl<TKey> other) => other != null && EqualityComparer.Equals(Key, other.Key) && Equals(Parent, other.Parent);

            public override bool Equals(object obj) => Equals(obj as Impl<TKey>);

            public override bool Equals(RWPathInfo other) => Equals(other as Impl<TKey>);

            protected override IDataReader GetDataReader(IDataReader dataReader)
            {
                if (IsRoot)
                {
                    return dataReader;
                }

                dataReader = Parent.GetDataReader(dataReader);

                return RWHelper.CreateItemReader(dataReader.As<TKey>(), Key, false);
            }

            protected override IDataWriter GetDataWriter(IDataWriter dataWriter)
            {
                if (IsRoot)
                {
                    return dataWriter;
                }

                var dataReader = (dataWriter as IDataReader) ?? RWHelper.CreateReader(dataWriter.Content, false);

                if (dataReader is null)
                {
                    return null;
                }

                dataReader = Parent.GetDataReader(dataReader);

                if (dataReader is null)
                {
                    return null;
                }

                return (dataReader as IDataWriter) ?? RWHelper.CreateWriter(dataReader.Content, false);
            }

            public override int GetHashCode() => IsRoot ? base.GetHashCode() : Parent.GetHashCode() ^ EqualityComparer.GetHashCode(Key);

            public override object GetKey() => Key;

            public override IValueReader GetValueReader(IDataReader dataReader)
            {
                if (IsRoot)
                {
                    return new ContentValueRW(dataReader);
                }

                dataReader = Parent.GetDataReader(dataReader);

                if (dataReader is null)
                {
                    return null;
                }

                return dataReader.As<TKey>()[Key];
            }

            public override IValueWriter GetValueWriter(IDataWriter dataWriter)
            {
                if (IsRoot)
                {
                    return new ContentValueRW(dataWriter);
                }

                dataWriter = Parent.GetDataWriter(dataWriter);

                if (dataWriter is null)
                {
                    return null;
                }

                return dataWriter.As<TKey>()[Key];
            }

            public override bool OnReadValue(IDataReader dataReader, IValueWriter valueWriter)
            {
                if (IsRoot)
                {
                    ValueInterface.WriteValue(valueWriter, dataReader.Content);

                    return true;
                }

                dataReader = Parent.GetDataReader(dataReader);

                if (dataReader is null)
                {
                    return false;
                }

                dataReader.As<TKey>().OnReadValue(Key, valueWriter);

                return true;
            }

            public override bool OnWriteValue(IDataWriter dataWriter, IValueReader valueReader)
            {
                if (IsRoot)
                {
                    dataWriter.Content = ValueInterface.ReadValue(valueReader, dataWriter.ContentType);

                    return true;
                }

                dataWriter = Parent.GetDataWriter(dataWriter);

                if (dataWriter is null)
                {
                    return false;
                }

                dataWriter.As<TKey>().OnWriteValue(Key, valueReader);

                return true;
            }

            public override string ToString() => IsRoot ? RootToken : Parent + PathSeparator + Key;

        }

        sealed class ContentValueRW : BaseDirectRW
        {
            public readonly object rw;

            public ContentValueRW(IDataReader reader)
            {
                rw = reader;
            }

            public ContentValueRW(IDataWriter writer)
            {
                rw = writer;
            }

            public ContentValueRW(IDataRW rw)
            {
                this.rw = rw;
            }

            public override object DirectRead()
            {
                return (rw as IDataReader)?.Content ?? Underlying.As<IDataWriter>(rw).Content;
            }

            public override void DirectWrite(object value)
            {
                if (rw is IDataReader reader)
                {
                    reader.Content = value;
                }
                else
                {
                    Underlying.As<IDataWriter>(rw).Content = value;
                }
            }
        }
    }
}