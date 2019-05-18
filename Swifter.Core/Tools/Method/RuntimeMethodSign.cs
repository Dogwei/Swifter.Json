using System;

namespace Swifter.Tools
{
    /// <summary>
    /// 运行时函数参数签名标识
    /// </summary>
    public sealed class RuntimeMethodSign
    {
        private RuntimeMethodSign(string name, object[] parameters)
        {
            this.name = name;
            this.parameters = parameters;
            hashCode = name.GetHashCode() ^ (46104728 * parameters.Length);

            isInputParams = true;
        }

        private RuntimeMethodSign(string name, Type[] types) : this(name, (object[])types)
        {
            isInputParams = false;
        }

        private readonly bool isInputParams;
        private readonly int hashCode;
        private readonly string name;
        private readonly object[] parameters;

        /// <summary>
        /// 返回此方法签名 HashCode。此值考虑方法名和参数生成。
        /// </summary>
        /// <returns>一个 HashCode 值。</returns>
        public override int GetHashCode()
        {
            return hashCode;
        }


        /// <summary>
        /// 比较一个对象的实例是否为 RuntimeMethodSign 类型，并且和当前实例的签名相同。
        /// </summary>
        /// <param name="obj">对象的实例</param>
        /// <returns>返回一个 bool 值</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is RuntimeMethodSign))
            {
                return false;
            }

            var Object = (RuntimeMethodSign)obj;

            if (name == Object.name && parameters.Length == Object.parameters.Length)
            {
                if (isInputParams)
                {
                    return TypeHelper.ParametersCompares((Type[])Object.parameters, parameters);
                }
                else if (Object.isInputParams)
                {
                    return TypeHelper.ParametersCompares((Type[])parameters, Object.parameters);
                }
                else
                {
                    return TypeHelper.ParametersCompares((Type[])Object.parameters, (Type[])parameters);
                }
            }

            return false;


        }
        
        /// <summary>
        /// 构造运行时函数参数签名标识
        /// </summary>
        /// <param name="methodSign">包含函数名称和函数参数</param>
        public static implicit operator RuntimeMethodSign((string name, object[] parameters) methodSign) => new RuntimeMethodSign(methodSign.name, methodSign.parameters);

        /// <summary>
        /// 构造运行时函数参数签名标识
        /// </summary>
        /// <param name="methodSign">包含函数名称和函数参数类型</param>
        public static implicit operator RuntimeMethodSign((string name, Type[] types) methodSign) => new RuntimeMethodSign(methodSign.name, methodSign.types);
    }
}