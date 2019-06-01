using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    internal static class InternalConvert<TSource, TDestination>
    {
        public static readonly IXConvert<TSource, TDestination> Instance;

        static InternalConvert()
        {
            foreach (var item in GetImpls())
            {
                if (item is IXConvert<TSource, TDestination> instance)
                {
                    Instance = instance;

                    return;
                }
            }

            throw new NotSupportedException();
        }

        private static IEnumerable<object> GetImpls()
        {
            yield return BasicConvert.Instance;

            if (typeof(TSource) == typeof(TDestination))
            {
                yield return new EqualsConvert<TSource>();
            }

            if (Nullable.GetUnderlyingType(typeof(TDestination)) == typeof(TSource))
            {
                yield return Activator.CreateInstance(typeof(StructToNullableConvert<>).MakeGenericType(typeof(TSource)));
            }

            if (Nullable.GetUnderlyingType(typeof(TSource)) == typeof(TDestination))
            {
                yield return Activator.CreateInstance(typeof(NullableToStructConvert<>).MakeGenericType(typeof(TDestination)));
            }

            if (typeof(TDestination) == typeof(string))
            {
                yield return new ToStringConvert<TSource>();
            }

            if (typeof(TDestination).IsAssignableFrom(typeof(TSource)))
            {
                yield return Activator.CreateInstance(typeof(AssignableConvert<,>).MakeGenericType(typeof(TSource), typeof(TDestination)));
            }

            if (typeof(TSource) == typeof(DBNull))
            {
                var underlyingType = Nullable.GetUnderlyingType(typeof(TDestination));

                if (underlyingType != null && underlyingType != typeof(TDestination))
                {
                    yield return Activator.CreateInstance(typeof(DBNullToNullableConvert<>).MakeGenericType(underlyingType));
                }

                if (typeof(TDestination).IsClass)
                {
                    yield return Activator.CreateInstance(typeof(DBNullToClassConvert<>).MakeGenericType(typeof(TDestination)));
                }
            }

            if (typeof(TDestination) == typeof(DBNull))
            {
                yield return new ToDBNullConvert<TSource>();
            }

            if (typeof(TSource) == typeof(string) && typeof(TDestination).IsEnum)
            {
                yield return Activator.CreateInstance(typeof(ParseEnum<>).MakeGenericType(typeof(TDestination)));
            }

            if (typeof(TSource).IsVisible && typeof(TDestination).IsVisible)
            {
                if (ImplicitConvert.TryGetMathod(typeof(TSource), typeof(TDestination), out var method))
                {
                    yield return BaseDynamicConvert.CreateInstanceByIL<TSource, TDestination>(method);
                }

                if (ExplicitConvert.TryGetMathod(typeof(TSource), typeof(TDestination), out method))
                {
                    yield return BaseDynamicConvert.CreateInstanceByIL<TSource, TDestination>(method);
                }

                if (ParseConvert.TryGetMathod(typeof(TSource), typeof(TDestination), out method))
                {
                    yield return BaseDynamicConvert.CreateInstanceByIL<TSource, TDestination>(method);
                }

                if (ToConvert.TryGetMathod(typeof(TSource), typeof(TDestination), out method))
                {
                    yield return BaseDynamicConvert.CreateInstanceByIL<TSource, TDestination>(method);
                }

                if (ConstructorConvert.TryGetMathod(typeof(TSource), typeof(TDestination), out var constructor))
                {
                    yield return BaseDynamicConvert.CreateInstanceByIL<TSource, TDestination>(constructor);
                }
            }

            if (typeof(TSource).IsAssignableFrom(typeof(TDestination)))
            {
                yield return Activator.CreateInstance(typeof(BaseConvert<,>).MakeGenericType(typeof(TSource), typeof(TDestination)));
            }

            yield return new ForceConvert<TSource, TDestination>();
        }
    }

    internal sealed class ToStringConvert<T> : IXConvert<T, string>
    {
        public string Convert(T value) => value?.ToString();
    }

    internal sealed class ParseEnum<TDestination> : IXConvert<string, TDestination> where TDestination:struct
    {
        public TDestination Convert(string value)
        {
#if NET20 || NET30 || NET35
            return (TDestination)Enum.Parse(typeof(TDestination), value);
#else
            if (Enum.TryParse<TDestination>(value, out var result))
            {
                return result;
            }

            return (TDestination)Enum.Parse(typeof(TDestination), value);
#endif
        }
    }

    internal sealed class EqualsConvert<T> : IXConvert<T, T>
    {
        public T Convert(T value) => value;
    }

    internal sealed class DBNullToNullableConvert<T> : IXConvert<DBNull, T?> where T : struct
    {
        public T? Convert(DBNull value) => null;
    }

    internal sealed class DBNullToClassConvert<T> : IXConvert<DBNull, T> where T : class
    {
        public T Convert(DBNull value) => null;
    }

    internal sealed class ToDBNullConvert<T> : IXConvert<T, DBNull>
    {
        public DBNull Convert(T value) => value == null ? DBNull.Value : throw new InvalidOperationException("Unable convert a value to DBNull.");
    }

    internal sealed class StructToNullableConvert<T> : IXConvert<T, T?> where T : struct
    {
        public T? Convert(T value) => value;
    }

    internal sealed class NullableToStructConvert<T> : IXConvert<T?, T> where T : struct
    {
        public T Convert(T? value) => value.Value;
    }

    internal sealed class AssignableConvert<T, TBase> : IXConvert<T, TBase> where T : TBase
    {
        public TBase Convert(T value) => value;
    }

    internal sealed class BaseConvert<TBase, T> : IXConvert<TBase, T> where T : TBase
    {
        public T Convert(TBase value) => (T)value;
    }

    internal sealed class ForceConvert<TSource, TDestination> : IXConvert<TSource, TDestination>
    {
        public TDestination Convert(TSource value) => (TDestination)(object)value;
    }

    internal abstract class BaseDynamicConvert
    {
        public static bool FirstEqualsSpecifiedType(MethodBase method, Type type)
        {
            var @params = method.GetParameters();

            return @params.Length == 1 && @params[0].ParameterType == type;
        }

        public static object CreateInstanceByIL<TSource, TDestination>(MethodBase method)
        {
            if (VersionDifferences.IsSupportEmit)
            {
                return null;
            }

            var typeBuilder = DynamicAssembly.DefineType(
                $"{typeof(TSource).Name}_To_{typeof(TDestination).Name}_{Guid.NewGuid().ToString("N")}",
                TypeAttributes.Public | TypeAttributes.Sealed);

            typeBuilder.AddInterfaceImplementation(typeof(IXConvert<TSource, TDestination>));

            var methodBuilder = typeBuilder.DefineMethod(
                nameof(IXConvert<TSource, TDestination>.Convert),
                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.Final,
                CallingConventions.HasThis,
                typeof(TDestination),
                new Type[] { typeof(TSource) });

            var ilGen = methodBuilder.GetILGenerator();
            
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

            var type = typeBuilder.CreateTypeInfo();

            return Activator.CreateInstance(type);
        }
    }

    internal sealed class ImplicitConvert : BaseDynamicConvert
    {
        public const BindingFlags ImplicitFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static readonly string ImplicitName =
            typeof(ImplicitConvert).GetMethods(ImplicitFlags)
            .First(item => item.ReturnType == typeof(ImplicitConvert) && FirstEqualsSpecifiedType(item, typeof(int)))
            .Name;

        public static bool TryGetMathod(Type tSource, Type tDestination, out MethodInfo method)
        {
            foreach (var item in tSource.GetMethods(ImplicitFlags))
            {
                if (item.Name == ImplicitName &&
                    item.ReturnType == tDestination &&
                    FirstEqualsSpecifiedType(item, tSource) &&
                    item.DeclaringType.IsVisible)
                {
                    method = item;

                    return true;
                }
            }

            method = tDestination.GetMethod(ImplicitName, ImplicitFlags, Type.DefaultBinder, new Type[] { tSource }, null);

            return method != null;
        }

        public static implicit operator ImplicitConvert(int value) => default;
    }

    internal sealed class ExplicitConvert : BaseDynamicConvert
    {
        public const BindingFlags ExplicitFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static readonly string ExplicitName =
            typeof(ExplicitConvert).GetMethods(ExplicitFlags)
            .First(item => item.ReturnType == typeof(ExplicitConvert) && FirstEqualsSpecifiedType(item, typeof(int)))
            .Name;

        public static bool TryGetMathod(Type tSource, Type tDestination, out MethodInfo method)
        {
            foreach (var item in tSource.GetMethods(ExplicitFlags))
            {
                if (item.Name == ExplicitName &&
                    item.ReturnType == tDestination &&
                    FirstEqualsSpecifiedType(item, tSource) &&
                    item.DeclaringType.IsVisible)
                {
                    method = item;

                    return true;
                }
            }

            method = tDestination.GetMethod(ExplicitName, ExplicitFlags, Type.DefaultBinder, new Type[] { tSource }, null);

            return method != null;

        }

        public static explicit operator ExplicitConvert(int value) => default;
    }

    internal sealed class ParseConvert : BaseDynamicConvert
    {
        public const BindingFlags ParseFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static readonly string[] ParseNames = ArrayHelper.Filter(
                typeof(ParseConvert).GetMethods(ParseFlags),
                item => item.ReturnType == typeof(ParseConvert) && FirstEqualsSpecifiedType(item, typeof(string)),
                item => item.Name);

        public static bool TryGetMathod(Type tSource, Type tDestination, out MethodInfo method)
        {
            var types = new Type[] { tSource };

            foreach (var item in ParseNames)
            {
                method = tDestination.GetMethod(item, ParseFlags, Type.DefaultBinder, types, null);

                if (method == null)
                {
                    continue;
                }

                if (method.IsGenericMethodDefinition)
                {
                    var gArgs = method.GetGenericArguments();

                    if (gArgs.Length == 1 && method.ReturnType == gArgs[0])
                    {
                        method = method.MakeGenericMethod(tDestination);
                    }
                    else
                    {
                        method = null;
                    }
                }

                if (method != null)
                {
                    return true;
                }
            }

            method = null;

            return false;
        }

        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            while (type != null)
            {
                yield return type;

                type = type.BaseType;
            }
        }

        private static IEnumerable<Type[]> GetParametersTypes(Type tSource, Type tDestination)
        {
            yield return new Type[] { tSource };
            yield return new Type[] { typeof(Type), tSource };
            yield return new Type[] { tSource, typeof(Type) };
        }

        public static ParseConvert Parse(string value) => default;

        public static ParseConvert ValueOf(string value) => default;
    }

    internal sealed class ToConvert : BaseDynamicConvert
    {
        public const BindingFlags ToFlags = BindingFlags.Public | BindingFlags.Instance;

        public static readonly NameInfo[] ToNames =
            typeof(ToConvert).GetMethods(ToFlags)
            .Where(item => item.ReturnType != typeof(void) && item.GetParameters().Length == 0 && CreateNameInfo(item) != null)
            .Select(item => CreateNameInfo(item))
            .Distinct()
            .ToArray();

        public static NameInfo CreateNameInfo(MethodInfo item)
        {
            var split = item.Name.Split(new string[] { item.ReturnType.Name }, StringSplitOptions.None);

            if (split.Length == 2)
            {
                return new NameInfo { Before = split[0], After = split[1] };
            }

            return null;
        }

        public static IEnumerable<string> GetDestinationName(Type tDestination)
        {
            if (tDestination.IsArray)
            {
                yield return nameof(Array);

                foreach (var item in GetDestinationName(tDestination.GetElementType()))
                {
                    yield return item + nameof(Array);
                    yield return item + "s";
                    yield return item + "es";

                    if (item.EndsWith("y")) yield return item.Substring(0, item.Length - 1) + "ies";
                    if (item.EndsWith("f")) yield return item.Substring(0, item.Length - 1) + "ves";
                    if (item.EndsWith("fe")) yield return item.Substring(0, item.Length - 2) + "ves";
                }
            }

            yield return tDestination.Name;
        }

        public static bool TryGetMathod(Type tSource, Type tDestination, out MethodInfo method)
        {
            foreach (var item in ToNames)
            {
                foreach (var dName in GetDestinationName(tDestination))
                {
                    method = tSource.GetMethod(
                        item.Before + dName + item.After,
                        ToFlags,
                        Type.DefaultBinder,
                        Type.EmptyTypes,
                        null);

                    if (method != null)
                    {
                        return true;
                    }
                }
            }

            method = null;

            return false;
        }

        public int ToInt32() => default;

        public int Int32 => default;

        public long Int64() => default;

        public sealed class NameInfo : IEquatable<NameInfo>
        {
            public string Before;

            public string After;

            public bool Equals(NameInfo other) => other != null && Before == other.Before && After == other.After;

            public override bool Equals(object obj) => obj is NameInfo other && Equals(other);

            public override int GetHashCode() => Before.GetHashCode() ^ After.GetHashCode();
        }
    }

    internal sealed class ConstructorConvert : BaseDynamicConvert
    {
        public static bool TryGetMathod(Type tSource, Type tDestination, out ConstructorInfo method)
        {
            method = tDestination.GetConstructor(new Type[] { tSource });

            return method != null;
        }
    }
}