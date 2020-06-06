using Swifter.Tools;
using System;
using System.Reflection;

using static Swifter.Reflection.ThrowHelpers;

namespace Swifter.Reflection
{
    /// <summary>
    /// XEventInfo 事件信息
    /// </summary>
    public sealed unsafe class XEventInfo
    {
        const int _type_static = 1;
        const int _type_struct = 2;
        const int _type_class = 3;

        /// <summary>
        /// 创建 XEventInfo 事件信息。
        /// </summary>
        /// <param name="eventInfo">.Net 自带的 EventInfo 事件信息</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回 XEventInfo 事件信息。</returns>
        public static XEventInfo Create(EventInfo eventInfo, XBindingFlags flags = XBindingFlags.Event)
        {
            return new XEventInfo(eventInfo, flags);
        }


        readonly IntPtr _add;
        readonly IntPtr _remove;
        readonly Type _declaring_type;
        readonly Type _handler_type;
        readonly int _type;

        readonly XBindingFlags Flags;

        /// <summary>
        /// 获取 .Net 自带的 EventInfo 事件信息。
        /// </summary>
        public readonly EventInfo EventInfo;

        /// <summary>
        /// 获取此事件名称。
        /// </summary>
        public string Name => EventInfo.Name;

        XEventInfo(EventInfo eventInfo, XBindingFlags flags)
        {
            Flags = flags;
            EventInfo = eventInfo;

            if (EventInfo.GetAddMethod((Flags & XBindingFlags.NonPublic) != 0) is var addMethod)
            {
                _add = addMethod.GetFunctionPointer();
            }

            if (EventInfo.GetRemoveMethod((Flags & XBindingFlags.NonPublic) != 0) is var removeMethod)
            {
                _remove = removeMethod.GetFunctionPointer();
            }

            _handler_type = eventInfo.EventHandlerType;
            _declaring_type = eventInfo.DeclaringType;

            if ((addMethod ?? removeMethod).IsStatic)
            {
                _type = _type_static;
            }
            else if (_declaring_type.IsValueType)
            {
                _type = _type_struct;
            }
            else
            {
                _type = _type_class;
            }
        }

        /// <summary>
        /// 添加该实例事件的处理器。
        /// </summary>
        /// <param name="obj">类型的实例</param>
        /// <param name="delegate">事件处理器</param>
        public void AddEventHandler(object obj, Delegate @delegate)
        {
            if (@delegate != null && !_handler_type.IsInstanceOfType(@delegate))
            {
                ThrowTargetException(nameof(@delegate), _handler_type);
            }

            if (_add == default)
            {
                ThrowMissingMethodException("Event", EventInfo.DeclaringType, EventInfo, "add");
            }

            switch (_type)
            {
                case _type_static:

                    ThrowInvalidOperationException("event", "instance");

                    break;

                case _type_struct:

                    if (!_declaring_type.IsInstanceOfType(obj))
                    {
                        ThrowTargetException(nameof(obj), _declaring_type);
                    }

                    fixed (byte* ptr = &TypeHelper.Unbox<byte>(obj))
                        Underlying.Call<IntPtr, Delegate>((IntPtr)ptr, @delegate, _add);

                    break;

                case _type_class:

                    if (!_declaring_type.IsInstanceOfType(obj))
                    {
                        ThrowTargetException(nameof(obj), _declaring_type);
                    }

                    Underlying.Call<object, Delegate>(obj, @delegate, _add);

                    break;
            }
        }

        /// <summary>
        /// 添加该实例事件的处理器。
        /// </summary>
        /// <param name="reference">类型的实例引用</param>
        /// <param name="delegate">事件处理器</param>
        public void AddEventHandler(TypedReference reference, Delegate @delegate)
        {
            if (@delegate != null && !_handler_type.IsInstanceOfType(@delegate))
            {
                ThrowTargetException(nameof(@delegate), _handler_type);
            }

            if (_add == default)
            {
                ThrowMissingMethodException("Event", EventInfo.DeclaringType, EventInfo, "add");
            }

            switch (_type)
            {
                case _type_static:

                    ThrowInvalidOperationException("event", "instance");

                    break;

                case _type_struct:

                    if (!_declaring_type.IsSubclassOf(__reftype(reference)))
                    {
                        ThrowTargetException(nameof(reference), _declaring_type);
                    }

                    fixed (byte* ptr = &TypeHelper.RefValue<byte>(reference)) 
                        Underlying.Call<IntPtr, Delegate>((IntPtr)ptr, @delegate, _add);

                    break;

                case _type_class:

                    var obj = TypeHelper.RefValue<object>(reference);

                    if (!_declaring_type.IsInstanceOfType(obj))
                    {
                        ThrowTargetException(nameof(reference), _declaring_type);
                    }

                    Underlying.Call<object, Delegate>(obj, @delegate, _add);

                    break;
            }
        }

        /// <summary>
        /// 移除该实例事件的处理器。
        /// </summary>
        /// <param name="obj">类型的实例</param>
        /// <param name="delegate">事件处理器</param>
        public void RemoveEventHandler(object obj, Delegate @delegate)
        {
            if (@delegate != null && !_handler_type.IsInstanceOfType(@delegate))
            {
                ThrowTargetException(nameof(@delegate), _handler_type);
            }

            if (_remove == default)
            {
                ThrowMissingMethodException("Event", EventInfo.DeclaringType, EventInfo, "remove");
            }

            switch (_type)
            {
                case _type_static:

                    ThrowInvalidOperationException("event", "instance");

                    break;

                case _type_struct:

                    if (!_declaring_type.IsInstanceOfType(obj))
                    {
                        ThrowTargetException(nameof(obj), _declaring_type);
                    }

                    fixed (byte* ptr = &TypeHelper.Unbox<byte>(obj))
                        Underlying.Call<IntPtr, Delegate>((IntPtr)ptr, @delegate, _remove);

                    break;

                case _type_class:

                    if (!_declaring_type.IsInstanceOfType(obj))
                    {
                        ThrowTargetException(nameof(obj), _declaring_type);
                    }

                    Underlying.Call<object, Delegate>(obj, @delegate, _remove);

                    break;
            }
        }

        /// <summary>
        /// 移除该实例事件的处理器。
        /// </summary>
        /// <param name="reference">类型的实例引用</param>
        /// <param name="delegate">事件处理器</param>
        public void RemoveEventHandler(TypedReference reference, Delegate @delegate)
        {
            if (@delegate != null && !_handler_type.IsInstanceOfType(@delegate))
            {
                ThrowTargetException(nameof(@delegate), _handler_type);
            }

            if (_remove == default)
            {
                ThrowMissingMethodException("Event", EventInfo.DeclaringType, EventInfo, "remove");
            }

            switch (_type)
            {
                case _type_static:

                    ThrowInvalidOperationException("event", "instance");

                    break;

                case _type_struct:

                    if (!_declaring_type.IsSubclassOf(__reftype(reference)))
                    {
                        ThrowTargetException(nameof(reference), _declaring_type);
                    }

                    fixed (byte* ptr = &TypeHelper.RefValue<byte>(reference))
                        Underlying.Call<IntPtr, Delegate>((IntPtr)ptr, @delegate, _remove);

                    break;

                case _type_class:

                    var obj = TypeHelper.RefValue<object>(reference);

                    if (!_declaring_type.IsInstanceOfType(obj))
                    {
                        ThrowTargetException(nameof(reference), _declaring_type);
                    }

                    Underlying.Call<object, Delegate>(obj, @delegate, _remove);

                    break;
            }
        }

        /// <summary>
        /// 添加该静态事件的处理器。
        /// </summary>
        /// <param name="delegate">事件处理器</param>
        public void AddEventHandler(Delegate @delegate)
        {
            if (@delegate != null && !_handler_type.IsInstanceOfType(@delegate))
            {
                ThrowTargetException(nameof(@delegate), _handler_type);
            }

            if (_add == default)
            {
                ThrowMissingMethodException("Event", EventInfo.DeclaringType, EventInfo, "add");
            }

            switch (_type)
            {
                case _type_static:

                    Underlying.Call<Delegate>(@delegate, _add);

                    break;

                case _type_struct:
                case _type_class:

                    ThrowInvalidOperationException("event", "static");

                    break;
            }
        }

        /// <summary>
        /// 移除该静态事件的处理器。
        /// </summary>
        /// <param name="delegate">事件处理器</param>
        public void RemoveEventHandler(Delegate @delegate)
        {
            if (@delegate != null && !_handler_type.IsInstanceOfType(@delegate))
            {
                ThrowTargetException(nameof(@delegate), _handler_type);
            }

            if (_remove == default)
            {
                ThrowMissingMethodException("Event", EventInfo.DeclaringType, EventInfo, "remove");
            }

            switch (_type)
            {
                case _type_static:

                    Underlying.Call<Delegate>(@delegate, _remove);

                    break;

                case _type_struct:
                case _type_class:

                    ThrowInvalidOperationException("event", "static");

                    break;
            }
        }
    }
}