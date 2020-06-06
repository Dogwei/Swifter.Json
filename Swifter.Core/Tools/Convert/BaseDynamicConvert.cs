using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swifter.Tools
{
    internal abstract class BaseDynamicConvert
    {
        public static bool OneParamsAndEqual(MethodBase method, Type type)
        {
            var @params = method.GetParameters();

            return @params.Length == 1 && @params[0].ParameterType == type;
        }

        public static object CreateInstanceByIL<TSource, TDestination>(MethodBase method)
        {
            if (!VersionDifferences.IsSupportEmit)
            {
                return null;
            }

            return Activator.CreateInstance(DynamicAssembly.DefineType(
                $"{typeof(TSource).Name}_To_{typeof(TDestination).Name}_{Guid.NewGuid().ToString("N")}",
                TypeAttributes.Public | TypeAttributes.Sealed,
                typeBuilder =>
                {
                    typeBuilder.AddInterfaceImplementation(typeof(IXConverter<TSource, TDestination>));

                    typeBuilder.DefineMethod(
                        nameof(IXConverter<TSource, TDestination>.Convert),
                        MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.Final,
                        CallingConventions.HasThis,
                        typeof(TDestination),
                        new Type[] { typeof(TSource) }, 
                        (methodBuilder, ilGen) =>
                        {
                            List<Type> argsTypes = new List<Type>();
                            Type returnType;

                            // Get args types and return type.
                            {
                                if (method is ConstructorInfo constructor)
                                {
                                    returnType = constructor.DeclaringType;
                                }
                                else if (method is MethodInfo methodInfo)
                                {
                                    if (!method.IsStatic)
                                    {
                                        var thisType = methodInfo.DeclaringType; ;

                                        if (thisType.IsValueType)
                                        {
                                            thisType = thisType.MakeByRefType();
                                        }

                                        argsTypes.Add(thisType);
                                    }

                                    returnType = methodInfo.ReturnType;
                                }
                                else
                                {
                                    throw new NotSupportedException(nameof(method));
                                }

                                foreach (var item in method.GetParameters())
                                {
                                    argsTypes.Add(item.ParameterType);
                                }
                            }

                            // Load args
                            {
                                foreach (var item in argsTypes)
                                {
                                    // 如果需要 Type 参数，可以认为它需要的是目标的类型信息。
                                    if (item == typeof(Type))
                                    {
                                        ilGen.LoadType(typeof(TDestination));
                                    }
                                    else if (item == typeof(TSource))
                                    {
                                        ilGen.LoadArgument(1);
                                    }
                                    else if (item == typeof(TSource).MakeByRefType())
                                    {
                                        ilGen.LoadArgumentAddress(1);
                                    }
                                    else if (typeof(TSource).IsValueType && item.IsAssignableFrom(typeof(TSource)))
                                    {
                                        ilGen.LoadArgument(1);
                                        ilGen.Box(typeof(TSource));
                                    }
                                    else if (item.IsAssignableFrom(typeof(TSource)))
                                    {
                                        ilGen.LoadArgument(1);
                                    }
                                    else
                                    {
                                        throw new NotSupportedException(nameof(method));
                                    }
                                }
                            }

                            // Call
                            {
                                if (method is ConstructorInfo constructor)
                                {
                                    ilGen.NewObject(constructor);
                                }
                                else
                                {
                                    ilGen.Call(method);
                                }
                            }

                            // Return
                            {
                                if (returnType.IsByRef)
                                {
                                    returnType = returnType.GetElementType();

                                    ilGen.LoadValue(returnType);
                                }

                                if (returnType.IsValueType && returnType != typeof(TDestination))
                                {
                                    if (typeof(TDestination).IsAssignableFrom(returnType))
                                    {
                                        // Box
                                        ilGen.Box(typeof(TDestination));
                                    }
                                    else
                                    {
                                        var convertMethod = typeof(XConvert<TDestination>).GetMethod(nameof(XConvert<TDestination>.Convert));

                                        convertMethod = convertMethod.MakeGenericMethod(returnType);

                                        ilGen.Call(convertMethod);
                                    }
                                }
                            }

                            ilGen.Return();

                        });
                }));
        }
    }
}