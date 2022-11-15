using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    sealed class SystemConvertFactory : IInternalXConverterFactory
    {
        const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.Static;

        static IEnumerable<Type> GetConvertTypes()
        {
            yield return typeof(ConvertAdd);
            yield return typeof(System.Convert);
        }

        static IEnumerable<MethodInfo> GetMethods()
        {
            return GetConvertTypes().SelectMany(type => type.GetMethods(BindingFlag));
        }

        public static void GetParametersTypes(MethodBase method, out Type[] parameterTypes, out Type returnType)
        {
            if (method is ConstructorInfo constructorInfo)
            {
                parameterTypes = constructorInfo.GetParameters().Map(parameter => parameter.ParameterType);

                returnType = constructorInfo.DeclaringType!;
            }
            else
            {
                MethodHelper.GetSignature(method, out parameterTypes, out returnType);
            }
        }

        public static XConvertFunc<TParameter, TReturn> CreateDelegate<TParameter, TReturn>(MethodBase method)
        {
            if (method is ConstructorInfo constructorInfo)
            {
                if (default(TReturn) is not null)
                {
                    var Action = MethodHelper.CreateDelegate<Action<IntPtr, TParameter?>>(method, false) ?? throw new NotSupportedException();

                    unsafe TReturn? CreateInstance(TParameter? value)
                    {
                        var Instance = default(TReturn)!;

                        Action((IntPtr)Unsafe.AsPointer(ref Instance), value);

                        return Instance;
                    }

                    return CreateInstance;
                }
                else
                {
                    var Action = MethodHelper.CreateDelegate<Action<TReturn, TParameter?>>(method, false) ?? throw new NotSupportedException();

                    TReturn? CreateInstance(TParameter? value)
                    {
                        var Instance = (TReturn)TypeHelper.Allocate(typeof(TReturn));

                        Action(Instance, value);

                        return Instance;
                    }

                    return CreateInstance;
                }
            }
            else
            {
                return MethodHelper.CreateDelegate<XConvertFunc<TParameter, TReturn>>(method, false) ?? throw new NotSupportedException();
            }
        }

        public static int GetComparison(MethodBase method, Type sourceType, Type destinationType)
        {
            GetParametersTypes(method, out var parameterTypes, out var returnType);

            if (parameterTypes.Length != 1)
            {
                return 999;
            }

            var parameterType = parameterTypes[0];

            if (returnType == typeof(void))
            {
                return 999;
            }

            int comparison = 0;

            if (parameterType != sourceType)
            {
                ++comparison;

                if (!CovariantFactory.CanConvert(sourceType, parameterType) && !BasicImplicitFactory.CanConvert(sourceType, parameterType))
                {
                    return 999;
                }

                if (parameterType == typeof(object) && !typeof(IConvertible).IsAssignableFrom(sourceType))
                {
                    return 999;
                }
            }

            if (returnType != destinationType)
            {
                ++comparison;

                if (!CovariantFactory.CanConvert(returnType, destinationType) && !BasicImplicitFactory.CanConvert(returnType, destinationType))
                {
                    return 999;
                }
            }

            return comparison;
        }

        public static MethodInfo? GetMethod(Type sourceType, Type destinationType)
        {
            foreach (var convertType in GetConvertTypes())
            {
                foreach (var convertMethod in convertType.GetMethods(BindingFlag))
                {
                    if (convertMethod.ReturnType == destinationType
                        && convertMethod.GetParameters() is var parameters
                        && parameters.Length == 1
                        && parameters[0].ParameterType == sourceType)
                    {
                        return convertMethod;
                    }
                }
            }

            return null;
        }

        public XConvertMode Mode => XConvertMode.Extended;

        public MethodBase? GetConverter<TSource, TDestination>()
        {
            return GetMethods()
                .Where(method => GetComparison(method, typeof(TSource), typeof(TDestination)) <= 2)
                .OrderBy(method => GetComparison(method, typeof(TSource), typeof(TDestination)))
                .FirstOrDefault();
        }

        static class ConvertAdd
        {
            public static string ToString(Guid value)
            {
                return value.ToString();
            }

            public static Guid ToGuid(string value)
            {
                return new Guid(value);
            }
        }
    }
}