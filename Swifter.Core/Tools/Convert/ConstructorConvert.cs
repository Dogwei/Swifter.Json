using System;
using System.Reflection;

namespace Swifter.Tools
{
    internal sealed class ConstructorConvert : BaseDynamicConvert
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        public static bool TryGetMathod(Type tSource, Type tDestination, out ConstructorInfo method)
        {
            method = null;

            var constructors = tDestination.GetConstructors(Flags);

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length == 1 && tSource == parameters[0].ParameterType)
                {
                    method = constructor;

                    return true;
                }
            }

            // 构造函数参数允许隐式转换。
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length == 1 && InternalConvert.IsImplicitConvert(tSource, parameters[0].ParameterType))
                {
                    method = constructor;

                    return true;
                }
            }


            return false;

            //if (tDestination.GetConstructor(new Type[] { tSource }) is ConstructorInfo constructorInfo && OneParamsAndEqual(constructorInfo, tSource))
            //{
            //    method = constructorInfo;

            //    return true;
            //}

            //method = null;

            //return false;
        }
    }
}