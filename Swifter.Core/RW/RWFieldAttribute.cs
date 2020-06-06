
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swifter.RW
{
    /// <summary>
    /// 表示对象读取器的一个字段的特性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class RWFieldAttribute : Attribute
    {
        /// <summary>
        /// 默认排序值。
        /// </summary>
        public const int DefaultOrder = 999;

        /// <summary>
        /// 获取与指定类型匹配的值读写接口方法。
        /// </summary>
        /// <param name="interfaceType">实现 IValueInterface 接口的类型</param>
        /// <param name="fieldType">指定类型</param>
        /// <param name="readValueMethod">值读取接口</param>
        /// <param name="writeValueMethod">值写入接口</param>
        protected static void GetBestMatchInterfaceMethod(Type interfaceType, Type fieldType, out MethodInfo readValueMethod, out MethodInfo writeValueMethod)
        {
            readValueMethod = null;
            writeValueMethod = null;

            var interfaces = new List<Type>();

            foreach (var item in interfaceType.GetInterfaces())
            {
                if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IValueInterface<>))
                {
                    interfaces.Add(item);
                }
            }

            Type targetType = null;

            foreach (var item in interfaces)
            {
                if (item.GetGenericArguments()[0] == fieldType)
                {
                    targetType = item;

                    break;
                }
            }

            if (targetType is null)
            {
                foreach (var item in interfaces)
                {
                    if (fieldType.IsAssignableFrom(item.GetGenericArguments()[0]))
                    {
                        targetType = item;

                        break;
                    }
                }
            }

            if (targetType is null)
            {
                foreach (var item in interfaces)
                {
                    if (item.GetGenericArguments()[0].IsAssignableFrom(fieldType))
                    {
                        targetType = item;

                        break;
                    }
                }
            }

            if (targetType is null)
            {
                GetBestMatchRWMethod(interfaceType, fieldType, out readValueMethod, out writeValueMethod);

                return;
            }

            var interfaceMap = interfaceType.GetInterfaceMap(targetType);

            var methods = interfaceMap.TargetMethods;

            if (!interfaceType.IsExternalVisible())
            {
                methods = interfaceMap.InterfaceMethods;
            }

            var genericArgument = targetType.GetGenericArguments()[0];

            foreach (var item in methods)
            {
                var parameters = item.GetParameters();

                if (parameters.Length == 1
                    && item.ReturnType == genericArgument
                    && parameters[0].ParameterType == typeof(IValueReader))
                {
                    readValueMethod = item;
                }
                else if (parameters.Length == 2
                    && item.ReturnType == typeof(void)
                    && parameters[0].ParameterType == typeof(IValueWriter)
                    && parameters[1].ParameterType == genericArgument)
                {
                    writeValueMethod = item;
                }
            }

            if (readValueMethod is null || writeValueMethod is null)
            {
                readValueMethod = null;
                writeValueMethod = null;
            }
        }

        /// <summary>
        /// 获取与指定类型匹配的读写方法。
        /// </summary>
        /// <param name="interfaceType">实现读写接口的类型</param>
        /// <param name="fieldType">指定类型</param>
        /// <param name="readValueMethod">值读取接口</param>
        /// <param name="writeValueMethod">值写入接口</param>
        protected static void GetBestMatchRWMethod(Type interfaceType, Type fieldType, out MethodInfo readValueMethod, out MethodInfo writeValueMethod)
        {
            readValueMethod = null;
            writeValueMethod = null;

            var methods = interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

            var readers = methods.Where(
                item => item.Name == nameof(IValueInterface<object>.ReadValue)
                    && item.GetParameters().Length == 1
                    && item.ReturnType != typeof(void)
                    && item.GetParameters()[0].ParameterType == typeof(IValueReader));

            var writers = methods.Where(
                item => item.Name == nameof(IValueInterface<object>.WriteValue)
                    && item.GetParameters().Length == 2
                    && item.ReturnType == typeof(void)
                    && item.GetParameters()[0].ParameterType == typeof(IValueWriter));

            foreach (var item in readers)
            {
                if (item.ReturnType == fieldType)
                {
                    readValueMethod = item;
                }
            }

            foreach (var item in writers)
            {
                if (item.GetParameters()[1].ParameterType == fieldType)
                {
                    writeValueMethod = item;
                }
            }

            if (readValueMethod is null)
            {
                foreach (var item in readers)
                {
                    if (item.IsGenericMethodDefinition)
                    {
                        var arguments = item.GetGenericArguments();

                        if (arguments.Length == 1 && arguments[0] == item.ReturnType)
                        {
                            readValueMethod = item.MakeGenericMethod(fieldType);
                        }
                    }
                }
            }

            if (writeValueMethod is null)
            {
                foreach (var item in writers)
                {
                    if (item.IsGenericMethodDefinition)
                    {
                        var arguments = item.GetGenericArguments();

                        if (arguments.Length == 1 && arguments[0] == item.GetParameters()[1].ParameterType)
                        {
                            writeValueMethod = item.MakeGenericMethod(fieldType);
                        }
                    }
                }
            }

            if (readValueMethod is null)
            {
                foreach (var item in readers)
                {
                    if (fieldType.IsAssignableFrom(item.ReturnType))
                    {
                        readValueMethod = item;
                    }
                }
            }

            if (writeValueMethod is null)
            {
                foreach (var item in writers)
                {
                    if (item.GetParameters()[1].ParameterType.IsAssignableFrom(fieldType))
                    {
                        writeValueMethod = item;
                    }
                }
            }

            if (readValueMethod is null)
            {
                foreach (var item in readers)
                {
                    if (item.ReturnType.IsAssignableFrom(fieldType))
                    {
                        readValueMethod = item;
                    }
                }
            }

            if (writeValueMethod is null)
            {
                foreach (var item in writers)
                {
                    if (fieldType.IsAssignableFrom(item.GetParameters()[1].ParameterType))
                    {
                        writeValueMethod = item;
                    }
                }
            }
        }

        /// <summary>
        /// 获取与指定类型匹配的值读写接口方法。
        /// </summary>
        /// <param name="fieldType">指定类型</param>
        /// <param name="firstArgument">值读写接口实例</param>
        /// <param name="readValueMethod">值读取接口</param>
        /// <param name="writeValueMethod">值写入接口</param>
        public virtual void GetBestMatchInterfaceMethod(Type fieldType, out object firstArgument, out MethodInfo readValueMethod, out MethodInfo writeValueMethod)
        {
            var interfaceType = GetInterfaceType(fieldType);

            firstArgument = interfaceType == GetType() ? this : Activator.CreateInstance(interfaceType);

            GetBestMatchInterfaceMethod(interfaceType, fieldType, out readValueMethod, out writeValueMethod);

        }

        /// <summary>
        /// 获取当前 Field 类型的 InterfaceType。
        /// </summary>
        /// <param name="fieldType">指定类型</param>
        /// <returns></returns>
        protected virtual Type GetInterfaceType(Type fieldType)
        {
            var interfaceType = InterfaceType;

            if (!interfaceType.ContainsGenericParameters)
            {
                return interfaceType;
            }

            if (interfaceType.GetGenericArguments().Length == 1)
            {
                interfaceType = interfaceType.MakeGenericType(fieldType);

                if (typeof(IValueInterface<>).MakeGenericType(fieldType).IsAssignableFrom(interfaceType))
                {
                    return interfaceType;
                }
            }

            throw new NotSupportedException("Unsupported The InterfaceType.");
        }

        /// <summary>
        /// 获取或设置字段的值读写接口类型。
        /// </summary>
        public virtual Type InterfaceType { get; set; }

        /// <summary>
        /// 初始化对象读取器的一个字段的特性。
        /// </summary>
        public RWFieldAttribute()
        {
            InterfaceType = GetType();
        }

        /// <summary>
        /// 初始化具有指定名称的对象读取器的一个字段的特性。
        /// </summary>
        /// <param name="name">指定名称</param>
        public RWFieldAttribute(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// 此字段的名称。
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// 此字段的排序值。约小越靠前，默认值为最靠后。
        /// </summary>
        public virtual int Order { get; set; } = DefaultOrder;

        /// <summary>
        /// 字段的可访问性。
        /// </summary>
        public virtual RWFieldAccess Access { get; set; } = RWFieldAccess.RW;

        /// <summary>
        /// 是否在 OnReadAll 中跳过当前成员的默认值。
        /// </summary>
        public virtual RWBoolean SkipDefaultValue { get; set; }

        /// <summary>
        /// 是否字段不能读取值时发生异常。
        /// </summary>
        public  virtual RWBoolean CannotGetException { get; set; }

        /// <summary>
        /// 是否字段不能写入值时发生异常。
        /// </summary>
        public virtual RWBoolean CannotSetException { get; set; }
    }
}