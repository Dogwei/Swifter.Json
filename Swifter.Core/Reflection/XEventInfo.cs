using InlineIL;
using Swifter.Tools;
using System;
using System.Reflection;

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

            if (EventInfo.GetAddMethod(Flags.On(XBindingFlags.NonPublic)) is var addMethod && addMethod!=null)
            {
                _add = addMethod.GetFunctionPointer();
            }

            if (EventInfo.GetRemoveMethod(Flags.On(XBindingFlags.NonPublic)) is var removeMethod && removeMethod != null)
            {
                _remove = removeMethod.GetFunctionPointer();
            }

            _handler_type = eventInfo.EventHandlerType!;
            _declaring_type = eventInfo.DeclaringType!;

            if (eventInfo.IsStatic())
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
            if (!_handler_type.IsInstanceOfType(@delegate))
            {
                throw new TargetException(nameof(@delegate));
            }

            if (_add == default)
            {
                EventInfo.AddEventHandler(obj, @delegate);

                return;
            }

            switch (_type)
            {
                case _type_static:

                    EventInfo.AddEventHandler(obj, @delegate);

                    break;

                case _type_struct:

                    if (!_declaring_type.IsInstanceOfType(obj))
                    {
                        throw new TargetException(nameof(obj));
                    }

                    IL.Push(obj);
                    IL.Emit.Call(MethodRef.Method(typeof(TypeHelper), nameof(TypeHelper.Unbox), 1, typeof(object)).MakeGenericMethod(typeof(byte)));
                    IL.Push(@delegate);
                    IL.Push(_add);
                    IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void), typeof(Delegate)));

                    break;

                case _type_class:

                    if (!_declaring_type.IsInstanceOfType(obj))
                    {
                        throw new TargetException(nameof(obj));
                    }

                    IL.Push(obj);
                    IL.Push(@delegate);
                    IL.Push(_add);
                    IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void), typeof(Delegate)));

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
            if (!_handler_type.IsInstanceOfType(@delegate))
            {
                throw new TargetException(nameof(@delegate));
            }

            if (_remove == default)
            {
                EventInfo.RemoveEventHandler(obj, @delegate);

                return;
            }

            switch (_type)
            {
                case _type_static:

                    EventInfo.RemoveEventHandler(obj, @delegate);

                    break;

                case _type_struct:

                    if (!_declaring_type.IsInstanceOfType(obj))
                    {
                        throw new TargetException(nameof(obj));
                    }

                    IL.Push(obj);
                    IL.Emit.Call(MethodRef.Method(typeof(TypeHelper), nameof(TypeHelper.Unbox), 1, typeof(object)).MakeGenericMethod(typeof(byte)));
                    IL.Push(@delegate);
                    IL.Push(_remove);
                    IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void), typeof(Delegate)));

                    break;

                case _type_class:

                    if (!_declaring_type.IsInstanceOfType(obj))
                    {
                        throw new TargetException(nameof(obj));
                    }

                    IL.Push(obj);
                    IL.Push(@delegate);
                    IL.Push(_remove);
                    IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void), typeof(Delegate)));

                    break;
            }
        }

        /// <summary>
        /// 添加该静态事件的处理器。
        /// </summary>
        /// <param name="delegate">事件处理器</param>
        public void AddEventHandler(Delegate @delegate)
        {
            if (!_handler_type.IsInstanceOfType(@delegate))
            {
                throw new TargetException(nameof(@delegate));
            }

            if (_add == default)
            {
                EventInfo.AddEventHandler(null, @delegate);

                return;
            }

            switch (_type)
            {
                case _type_static:

                    ((delegate*<Delegate, void>)_add)(@delegate);

                    break;

                case _type_struct:
                case _type_class:

                    EventInfo.AddEventHandler(null, @delegate);

                    break;
            }
        }

        /// <summary>
        /// 移除该静态事件的处理器。
        /// </summary>
        /// <param name="delegate">事件处理器</param>
        public void RemoveEventHandler(Delegate @delegate)
        {
            if (!_handler_type.IsInstanceOfType(@delegate))
            {
                throw new TargetException(nameof(@delegate));
            }

            if (_remove == default)
            {
                EventInfo.RemoveEventHandler(null, @delegate);

                return;
            }

            switch (_type)
            {
                case _type_static:

                    ((delegate*<Delegate, void>)_remove)(@delegate);

                    break;

                case _type_struct:
                case _type_class:

                    EventInfo.RemoveEventHandler(null, @delegate);

                    break;
            }
        }
    }
}