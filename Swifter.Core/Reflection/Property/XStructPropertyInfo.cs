
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

#pragma warning disable

namespace Swifter.Reflection
{
    /// <summary>
    /// 表示一个结构类型中的实例属性的信息。
    /// </summary>
    /// <typeparam name="TStruct"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed unsafe class XStructPropertyInfo<TStruct, TValue> : XPropertyInfo, IXFieldRW where TStruct : struct
    {
        delegate ref TValue RefValueHandler(ref TStruct obj);
        delegate TValue GetValueHandler(ref TStruct obj);
        delegate void SetValueHandler(ref TStruct obj, TValue value);

        GetValueHandler _get;
        SetValueHandler _set;

        XStructPropertyInfo()
        {
        }

        private protected override void InitializeByValue(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByValue(propertyInfo, flags);

            if ((flags & XBindingFlags.RWAutoPropertyDirectRW) != 0/* || !VersionDifferences.IsSupportEmit */)
            {
                if (TypeHelper.IsAutoProperty(propertyInfo, out var fieldInfo) && fieldInfo != null)
                {
                    try
                    {
                        var offset = TypeHelper.OffsetOf(fieldInfo);

                        _get = (ref TStruct obj) => Underlying.AddByteOffset(ref Underlying.As<TStruct, TValue>(ref obj), offset);
                        _set = (ref TStruct obj, TValue value) => Underlying.AddByteOffset(ref Underlying.As<TStruct, TValue>(ref obj), offset) = value;

                        return;
                    }
                    catch
                    {
                    }
                }
            }

            if (_get is null)
            {
                var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (getMethod != null)
                {
                    var __get = MethodHelper.CreateDelegate<GetValueHandler>(getMethod);

                    _get = VersionDifferences.IsSupportEmit ? __get : AOTCheck;

                    TValue AOTCheck(ref TStruct obj)
                    {
                        try
                        {
                            return __get(ref obj);
                        }
                        catch (ExecutionEngineException)
                        {
                            __get = AOT;

                            return AOT(ref obj);
                        }
                        finally
                        {
                            _get = __get;
                        }
                    }

                    TValue AOT(ref TStruct obj)
                    {
                        fixed (byte* ptr = &Underlying.As<TStruct, byte>(ref obj))
                        {
                            return (TValue)getMethod.Invoke(TypeHelper.CamouflageBox(ptr), null);
                        }
                    }
                }
            }

            if (_set is null)
            {
                var setMethod = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (setMethod != null)
                {
                    var __set = (SetValueHandler)Delegate.CreateDelegate(typeof(SetValueHandler), setMethod);

                    _set = VersionDifferences.IsSupportEmit ? __set : AOTCheck;

                    void AOTCheck(ref TStruct obj, TValue value)
                    {
                        try
                        {
                            __set(ref obj, value);
                        }
                        catch (ExecutionEngineException)
                        {
                            __set = AOT;

                            AOT(ref obj, value);
                        }
                        finally
                        {
                            _set = __set;
                        }
                    }

                    void AOT(ref TStruct obj, TValue value)
                    {
                        fixed (byte* ptr = &Underlying.As<TStruct, byte>(ref obj))
                        {
                            setMethod.Invoke(TypeHelper.CamouflageBox(ptr), new object[] { value });
                        }
                    }
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
                    var _ref = (RefValueHandler)Delegate.CreateDelegate(typeof(RefValueHandler), getMethod);

                    _get = (ref TStruct obj) =>
                    {
                        return _ref(ref obj);
                    };

                    _set = (ref TStruct obj, TValue value) =>
                    {
                        _ref(ref obj) = value;
                    };

                    // TODO: AOTCheck
                }
            }
        }

        /// <summary>
        /// 获取一个值，表示属性能否读取。
        /// </summary>
        public bool CanRead
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => _get != null; }

        /// <summary>
        /// 获取一个值，表示属性能否写入。
        /// </summary>
        public bool CanWrite
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => _set != null; }

        /// <summary>
        /// 获取属性的值。
        /// </summary>
        /// <param name="obj">结构引用</param>
        /// <returns>返回属性的值</returns>
        /// <exception cref="MissingMethodException"><see cref="CanRead"/> 为 False</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public TValue GetValue(ref TStruct obj)
        {
            Assert(CanRead, "get");

            return _get(ref obj);
        }

        /// <summary>
        /// 设置属性的值。
        /// </summary>
        /// <param name="obj">结构引用</param>
        /// <param name="value">值</param>
        /// <exception cref="MissingMethodException"><see cref="CanWrite"/> 为 False</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SetValue(ref TStruct obj, TValue value)
        {
            Assert(CanWrite, "set");

            _set(ref obj, value);
        }

        /// <summary>
        /// 获取属性的值。
        /// </summary>
        /// <param name="obj">结构已装箱的实例</param>
        /// <returns>返回属性的值</returns>
        /// <exception cref="InvalidCastException">对象不是字段的定义类的类型</exception>
        /// <exception cref="MissingMethodException"><see cref="CanRead"/> 为 False</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override object GetValue(object obj)
        {
            Assert(CanRead, "get");

            return _get(ref Underlying.Unbox<TStruct>(obj));
        }

        /// <summary>
        /// 设置属性的值。
        /// </summary>
        /// <param name="obj">结构已装箱的实例</param>
        /// <param name="value">值</param>
        /// <exception cref="InvalidCastException">对象不是字段的定义类的类型</exception>
        /// <exception cref="MissingMethodException"><see cref="CanWrite"/> 为 False</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override void SetValue(object obj, object value)
        {
            Assert(CanWrite, "set");

            _set(ref Underlying.Unbox<TStruct>(obj), (TValue)value);
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
            ValueInterface<TValue>.WriteValue(
                valueWriter, 
                GetValue(ref TypeHelper.Unbox<TStruct>(obj))
                );
        }

        void IXFieldRW.OnWriteValue(object obj, IValueReader valueReader)
        {
            SetValue(
                ref TypeHelper.Unbox<TStruct>(obj), 
                ValueInterface<TValue>.ReadValue(valueReader)
                );
        }

        T IXFieldRW.ReadValue<T>(object obj)
        {
            return XConvert<T>.Convert(
                GetValue(ref TypeHelper.Unbox<TStruct>(obj))
                );
        }

        void IXFieldRW.WriteValue<T>(object obj, T value)
        {
            SetValue(
                ref TypeHelper.Unbox<TStruct>(obj),
                XConvert<TValue>.Convert(value)
                );
        }
    }
}