

using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    /// <summary>
    /// 表示对象路径信息。
    /// </summary>
    public abstract class RWPathInfo
    {
        /// <summary>
        /// 表示根路径对象。
        /// </summary>
        public static readonly RWPathInfo Root = new RWPathInfo<string>(null, null);

        /// <summary>
        /// 创建一个对象路径信息。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="parent">父级路径</param>
        /// <returns>返回一个新的路径信息</returns>
        public static RWPathInfo Create<TKey>(TKey key, RWPathInfo parent = null) => new RWPathInfo<TKey>(key, parent ?? Root);

        /// <summary>
        /// 父级路径。
        /// </summary>
        public readonly RWPathInfo Parent;


        internal RWPathInfo(RWPathInfo parent)
        {
            if (parent == null && Root != null)
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
        /// 获取一个数据读取器该路径的值。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>返回一个值</returns>
        public abstract object GetValue(IDataReader dataReader);

        /// <summary>
        /// 设置一个数据写入器该路径的值。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="value">值</param>
        public abstract void SetValue(IDataWriter dataWriter, object value);

        /// <summary>
        /// 获取数据读取器的值的数据读取器。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>返回数据读取器</returns>
        public abstract IDataReader GetDataReader(IDataReader dataReader);
    }

    sealed class RWPathInfo<TKey> : RWPathInfo, IEquatable<RWPathInfo<TKey>>
    {
        public static readonly IEqualityComparer<TKey> EqualityComparer = EqualityComparer<TKey>.Default;

        public readonly TKey Key;

        public RWPathInfo(TKey key, RWPathInfo parent) : base(parent)
        {
            Key = key;
        }

        public bool Equals(RWPathInfo<TKey> other) => other != null && EqualityComparer.Equals(Key, other.Key) && Equals(Parent, other.Parent);

        public override bool Equals(object obj) => Equals(obj as RWPathInfo<TKey>);

        public override IDataReader GetDataReader(IDataReader dataReader)
        {
            if (IsRoot)
            {
                return dataReader;
            }

            dataReader = Parent.GetDataReader(dataReader);

            return RWHelper.CreateItemReader(dataReader.As<TKey>(), Key);
        }

        public override int GetHashCode() => IsRoot ? base.GetHashCode() : Parent.GetHashCode() ^ EqualityComparer.GetHashCode(Key);

        public override object GetKey() => Key;

        public override object GetValue(IDataReader dataReader)
        {
            if (IsRoot)
            {
                return RWHelper.GetContent<object>(dataReader);
            }

            dataReader = Parent.GetDataReader(dataReader);

            if (typeof(TKey) == typeof(long) && dataReader is IId64DataRW id64DataRW && !(dataReader is IDataReader<long>))
            {
                var valueCopyer = new ValueCopyer();

                id64DataRW.OnReadValue(Unsafe.As<TKey, long>(ref Unsafe.AsRef(Key)), valueCopyer);

                return valueCopyer.DirectRead();
            }

            return dataReader.As<TKey>()[Key].DirectRead();
        }

        public override void SetValue(IDataWriter dataWriter, object value)
        {
            if (IsRoot)
            {
                RWHelper.SetContent(dataWriter, value);

                return;
            }

            if (!Parent.IsRoot)
            {
                if (!(dataWriter is IDataReader dataReader))
                {
                    dataReader = RWHelper.CreateReader(RWHelper.GetContent<object>(dataWriter));
                }

                dataReader = Parent.GetDataReader(dataReader);

                dataWriter = dataReader as IDataWriter;

                if (dataWriter == null)
                {
                    dataWriter = RWHelper.CreateRW(RWHelper.GetContent<object>(dataReader));
                }
            }

            if (typeof(TKey) == typeof(long) && dataWriter is IId64DataRW id64DataRW && !(dataWriter is IDataWriter<long>))
            {
                var valueCopyer = new ValueCopyer();

                valueCopyer.DirectWrite(value);

                id64DataRW.OnWriteValue(Unsafe.As<TKey, long>(ref Unsafe.AsRef(Key)), valueCopyer);

                return;
            }

            dataWriter.As<TKey>()[Key].DirectWrite(value);
        }

        public override string ToString() => IsRoot ? "#" : Parent + "/" + Key;
    }
}
