
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// 表示静态属性的信息。
    /// </summary>
    /// <typeparam name="TValue">属性类型</typeparam>
    public sealed class XStaticPropertyInfo<TValue> : XPropertyInfo, IXFieldRW
    {
        XStaticGetValueHandler<TValue> _get;
        XStaticSetValueHandler<TValue> _set;

        /// <summary>
        /// 获取或设置属性的值。
        /// </summary>
        public TValue Value
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => _get();
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            set => _set(value);
        }

        XStaticPropertyInfo()
        {

        }

        private protected override void InitializeByValue(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByValue(propertyInfo, flags);

            // TODO: RWAutoPropertyDirectRW

            if (_get is null)
            {
                var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (getMethod != null)
                {
                    _get = MethodHelper.CreateDelegate<XStaticGetValueHandler<TValue>>(getMethod, false);
                }
            }

            if (_set is null)
            {
                var setMethod = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (setMethod != null)
                {
                    _set = MethodHelper.CreateDelegate<XStaticSetValueHandler<TValue>>(setMethod, false);
                }
            }
        }

        private protected override void InitializeByRef(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByRef(propertyInfo, flags);

            if (_get is null || _set is null)
            {
                var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (getMethod != null)
                {
                    var _ref = MethodHelper.CreateDelegate<XStaticRefValueHandler<TValue>>(getMethod, false);

                    _get = () =>
                    {
                        return _ref();
                    };

                    _set = (value) =>
                    {
                        _ref() = value;
                    };
                }
            }
        }


        /// <summary>
        /// 获取属性的值。
        /// </summary>
        /// <returns>返回属性的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override object GetValue()
        {
            Assert(CanRead, "get");

            return _get();
        }

        /// <summary>
        /// 设置属性的值。
        /// </summary>
        /// <param name="value">值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override void SetValue(object value)
        {
            Assert(CanWrite, "set");

            _set((TValue)value);
        }

        /// <summary>
        /// 获取一个值，表示属性能否读取。
        /// </summary>
        public bool CanRead
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => _get != null;
        }

        /// <summary>
        /// 获取一个值，表示属性能否写入。
        /// </summary>
        public bool CanWrite
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => _set != null;
        }


        int IObjectField.Order => RWFieldAttribute.DefaultOrder;

        Type IObjectField.BeforeType => typeof(TValue);

        Type IObjectField.AfterType => typeof(TValue);

        bool IObjectField.IsPublic => (PropertyInfo.GetGetMethod(true) ?? PropertyInfo.GetSetMethod(true))?.IsPublic ?? false;

        bool IObjectField.IsStatic => true;

        object IObjectField.Original => PropertyInfo;

        bool IObjectField.SkipDefaultValue => (flags & XBindingFlags.RWSkipDefaultValue) != 0;

        bool IObjectField.CannotGetException => (flags & XBindingFlags.RWCannotGetException) != 0;

        bool IObjectField.CannotSetException => (flags & XBindingFlags.RWCannotSetException) != 0;


        void IXFieldRW.OnReadValue(object obj, IValueWriter valueWriter)
        {
            Assert(CanRead, "get");

            ValueInterface<TValue>.WriteValue(valueWriter, Value);
        }

        void IXFieldRW.OnWriteValue(object obj, IValueReader valueReader)
        {
            Assert(CanWrite, "set");

            Value = ValueInterface<TValue>.ReadValue(valueReader);
        }

        T IXFieldRW.ReadValue<T>(object obj)
        {
            Assert(CanRead, "get");

            return XConvert<T>.Convert(Value);
        }

        void IXFieldRW.WriteValue<T>(object obj, T value)
        {
            Assert(CanWrite, "set");

            Value = XConvert<TValue>.Convert(value);
        }
    }
}