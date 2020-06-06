
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// 表示一个类类型中的实例属性信息。
    /// </summary>
    /// <typeparam name="TClass">类类型</typeparam>
    /// <typeparam name="TValue">属性类型</typeparam>
    public sealed class XClassPropertyInfo<TClass, TValue> : XPropertyInfo, IXFieldRW where TClass : class
    {
        XClassGetValueHandler<TClass, TValue> _get;
        XClassSetValueHandler<TClass, TValue> _set;

        XClassPropertyInfo()
        {

        }

        private protected override void InitializeByValue(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByValue(propertyInfo, flags);

            if ((flags & XBindingFlags.RWAutoPropertyDirectRW) != 0 && TypeHelper.IsAutoProperty(propertyInfo, out var fieldInfo) && fieldInfo != null)
            {
                try
                {
                    var offset = TypeHelper.OffsetOf(fieldInfo);

                    _get = (obj) => Underlying.AddByteOffset(ref TypeHelper.Unbox<TValue>(obj), offset);
                    _set = (obj, value) => Underlying.AddByteOffset(ref TypeHelper.Unbox<TValue>(obj), offset) = value;

                    return;
                }
                catch
                {
                }
            }

            if (_get is null)
            {
                var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (getMethod != null)
                {
                    _get = MethodHelper.CreateDelegate<XClassGetValueHandler<TClass, TValue>>(getMethod, false);
                }
            }

            if (_set is null)
            {
                var setMethod = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (setMethod != null)
                {
                    _set = MethodHelper.CreateDelegate<XClassSetValueHandler<TClass, TValue>>(setMethod, false);
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
                    var _ref = MethodHelper.CreateDelegate<XClassRefValueHandler<TClass, TValue>>(getMethod, false);

                    _get = (obj) =>
                    {
                        return _ref(obj);
                    };

                    _set = (obj, value) =>
                    {
                        _ref(obj) = value;
                    };
                }
            }
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

        /// <summary>
        /// 获取属性的值。
        /// </summary>
        /// <param name="obj">对象实例</param>
        /// <returns>返回属性的值</returns>
        /// <exception cref="InvalidCastException">对象实例不是字段的定义类的类型</exception>
        /// <exception cref="MissingMethodException"><see cref="CanRead"/> 为 False</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override object GetValue(object obj)
        {
            return GetValue((TClass)obj);
        }

        /// <summary>
        /// 设置属性的值。
        /// </summary>
        /// <param name="obj">对象实例</param>
        /// <param name="value">值</param>
        /// <exception cref="InvalidCastException">对象实例不是字段的定义类的类型或值的类型错误</exception>
        /// <exception cref="MissingMethodException"><see cref="CanWrite"/> 为 False</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override void SetValue(object obj, object value)
        {
            SetValue((TClass)obj, (TValue)value);
        }

        /// <summary>
        /// 获取属性的值。
        /// </summary>
        /// <param name="obj">对象实例</param>
        /// <returns>返回属性的值</returns>
        /// <exception cref="MissingMethodException"><see cref="CanRead"/> 为 False</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public TValue GetValue(TClass obj)
        {
            Assert(CanRead, "get");

            return _get(obj);
        }

        /// <summary>
        /// 设置属性的值。
        /// </summary>
        /// <param name="obj">对象实例</param>
        /// <param name="value">值</param>
        /// <exception cref="MissingMethodException"><see cref="CanWrite"/> 为 False</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SetValue(TClass obj, TValue value)
        {
            Assert(CanWrite, "set");

            _set(obj, value);
        }


        int IObjectField.Order => RWFieldAttribute.DefaultOrder;

        Type IObjectField.BeforeType => typeof(TValue);

        Type IObjectField.AfterType => typeof(TValue);

        bool IObjectField.IsPublic => (PropertyInfo.GetGetMethod(true) ?? PropertyInfo.GetSetMethod(true))?.IsPublic ?? false;

        bool IObjectField.IsStatic => false;

        object IObjectField.Original => PropertyInfo;

        bool IObjectField.SkipDefaultValue => (flags & XBindingFlags.RWSkipDefaultValue) != 0;

        bool IObjectField.CannotGetException => (flags & XBindingFlags.RWCannotGetException) != 0;

        bool IObjectField.CannotSetException => (flags & XBindingFlags.RWCannotSetException) != 0;

        void IXFieldRW.OnReadValue(object obj, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.WriteValue(valueWriter, GetValue(Underlying.As<TClass>(obj)));
        }

        void IXFieldRW.OnWriteValue(object obj, IValueReader valueReader)
        {
            SetValue(Underlying.As<TClass>(obj), ValueInterface<TValue>.ReadValue(valueReader));
        }

        T IXFieldRW.ReadValue<T>(object obj)
        {
            return XConvert<T>.Convert(GetValue(Underlying.As<TClass>(obj)));
        }

        void IXFieldRW.WriteValue<T>(object obj, T value)
        {
            SetValue(Underlying.As<TClass>(obj), XConvert<TValue>.Convert(value));
        }
    }
}