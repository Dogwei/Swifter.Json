using InlineIL;
using Swifter.Tools;
using System;
using System.Reflection;

namespace Swifter.Reflection
{
    /// <summary>
    /// 构造函数信息。
    /// </summary>
    public sealed class XConstructorInfo
    {
        /// <summary>
        /// 创建构造函数信息。
        /// </summary>
        /// <param name="constructorInfo">.NET 自带的构造函数信息</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回一个构造函数信息</returns>
        public static XConstructorInfo Create(ConstructorInfo constructorInfo, XBindingFlags flags)
        {
            return new XConstructorInfo(constructorInfo, flags);
        }

        /// <summary>
        /// 获取构造函数的定义类。
        /// </summary>
        public readonly Type DeclaringType;

        /// <summary>
        /// 获取 .NET 自带的构造函数信息。
        /// </summary>
        public readonly ConstructorInfo ConstructorInfo;

        /// <summary>
        /// 获取构造函数的参数信息集合。
        /// </summary>
        public readonly XMethodParameters Parameters;

        readonly bool isStruct;
        readonly IntPtr functionPointer;

        XConstructorInfo(ConstructorInfo constructorInfo, XBindingFlags _)
        {
            ConstructorInfo = constructorInfo;
            DeclaringType = constructorInfo.DeclaringType!;
            Parameters = new(constructorInfo.GetParameters());

            isStruct = DeclaringType.IsValueType;
            functionPointer = constructorInfo.MethodHandle.GetFunctionPointer();
        }

        /// <summary>
        /// 创建一个类型实例并执行该构造函数。
        /// </summary>
        /// <param name="parameters">参数集合</param>
        /// <returns>返回类型实例</returns>
        public object Invoke(object?[]? parameters)
        {
            if (Parameters.Count is 0)
            {
                var instance = TypeHelper.Allocate(DeclaringType);

                Invoke(instance, parameters);

                return instance;
            }

            return ConstructorInfo.Invoke(parameters);
        }

        /// <summary>
        /// 执行该构造函数。
        /// </summary>
        /// <param name="instance">类型实例</param>
        /// <param name="parameters">参数集合</param>
        public unsafe void Invoke(object instance, object?[]? parameters)
        {
            if (Parameters.Count is 0)
            {
                if (isStruct)
                {
                    // 值类型的空构造函数无需任何操作。
                    return;
                }
                else if (DeclaringType.IsInstanceOfType(instance))
                {
                    IL.Push(instance);
                    IL.Push(functionPointer);
                    IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void)));

                    return;
                }
            }

            ConstructorInfo.Invoke(instance, parameters);
        }
    }
}