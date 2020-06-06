using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swifter.Tools
{
    /// <summary>
    /// 基础转换函数实现类
    /// </summary>
    public sealed partial class BasicConvert
    {
        /// <summary>
        /// 获取基础转换函数实现类的实例。
        /// </summary>
        public static readonly BasicConvert Instance = new BasicConvert();

        private BasicConvert()
        {

        }

        /// <summary>
        /// 获取所有可用的类型转换工具类。
        /// </summary>
        /// <returns>返回类型集合</returns>
        public static IEnumerable<Type> GetConvertTypes()
        {
            yield return typeof(ConvertAdd);
            yield return typeof(Convert);
        }

        /// <summary>
        /// 获取所有可用类型转换函数。
        /// </summary>
        /// <returns>返回类型转换函数集合</returns>
        public static List<MethodInfo> GetConvertMethods()
        {
            var methods = new List<MethodInfo>();

            foreach (var convertType in GetConvertTypes())
            {
                methods.AddRange(convertType.GetMethods(BindingFlags.Static | BindingFlags.Public));
            }

            methods = Filter(methods.Distinct(ConvertMethodComparer.Instance)).ToList();

            methods.Sort((x, y) => x.ReturnType.Name.CompareTo(y.ReturnType.Name));

            return methods;

            static IEnumerable<MethodInfo> Filter(IEnumerable<MethodInfo> methods)
            {
                foreach (var method in methods)
                {
                    if (method.ReturnType != typeof(void) && method.GetParameters().Length == 1)
                    {
                        if (method.ReturnType.IsByRef)
                        {
                            continue;
                        }

                        if (method.GetParameters()[0].ParameterType.IsByRef)
                        {
                            continue;
                        }

                        yield return method;
                    }
                }
            }
        }

        /// <summary>
        /// 获取一个类型在 C# 代码中的标识符。
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns>返回一个 stirng 标识符</returns>
        public static string GetCodeName(Type type)
        {
            return type switch
            {
                _ when type == typeof(byte) => "byte",
                _ when type == typeof(sbyte) => "sbyte",
                _ when type == typeof(short) => "short",
                _ when type == typeof(ushort) => "ushort",
                _ when type == typeof(int) => "int",
                _ when type == typeof(uint) => "uint",
                _ when type == typeof(long) => "long",
                _ when type == typeof(ulong) => "ulong",
                _ when type == typeof(float) => "float",
                _ when type == typeof(double) => "double",
                _ when type == typeof(bool) => "bool",
                _ when type == typeof(char) => "char",
                _ when type == typeof(string) => "string",
                _ when type == typeof(decimal) => "decimal",
                _ when type == typeof(object) => "object",
                _ when type.IsArray => $"{GetCodeName(type.GetElementType())}[]",
                _ when type.IsPointer => $"{GetCodeName(type)}*",
                _ when type.Namespace == "System" => type.Name,
                _ when type.Namespace == "Swifter.Tools" => type.Name,
                _ when type.Namespace == "Swifter" => type.Name,
                _ => type.FullName,
            };
        }
    }
}
