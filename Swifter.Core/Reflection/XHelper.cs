using Swifter.RW;
using Swifter.Tools;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    static class XHelper
    {
        const int WriteValueMethodValueArgIndex = 1;

        public static bool TryGetValueInterface(
            object? firstArgument,
            MethodInfo? readValueMethod,
            MethodInfo? writeValueMethod,
            [NotNullWhen(true)] out object? valueInterface,
            [NotNullWhen(true)] out Type? valueType)
        {
            if (firstArgument is not null
                && readValueMethod is not null
                && writeValueMethod is not null
                && readValueMethod.ReturnType == writeValueMethod.GetParameters()[WriteValueMethodValueArgIndex].ParameterType)
            {
                valueType = readValueMethod.ReturnType;

                foreach (var item in firstArgument.GetType().GetInterfaces())
                {
                    if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IValueInterface<>) && item.GetGenericArguments()[0] == valueType)
                    {
                        valueInterface = firstArgument;

                        return true;
                    }
                }
            }

            valueInterface = null;
            valueType = null;

            return false;
        }

        public static object GetValueInterface(
            Type fieldType,
            object? firstArgument,
            MethodInfo? readValueMethod, 
            MethodInfo? writeValueMethod)
        {
            if (TryGetValueInterface(firstArgument, readValueMethod, writeValueMethod, out var valueInterface, out var valueType))
            {
                if (valueType == fieldType)
                {
                    return valueInterface;
                }
                else
                {
                    return typeof(XConvertValueInterface<,>)
                        .MakeGenericType(fieldType, valueType)
                        .GetConstructors()
                        .First()
                        .Invoke(new object?[] { valueInterface });
                }
            }
            else
            {
                var readType = readValueMethod?.ReturnType ?? fieldType;
                var writeType = writeValueMethod?.GetParameters()[WriteValueMethodValueArgIndex].ParameterType ?? fieldType;

                return typeof(XDelegateValueInterface<,,>)
                    .MakeGenericType(fieldType, readType, writeType)
                    .GetConstructors()
                    .First()
                    .Invoke(new object?[] { firstArgument, readValueMethod, writeValueMethod });
            }
        }

        public static ValueInterface MakeValueInterface(object valueInterface)
        {
            foreach (var item in valueInterface.GetType().GetInterfaces())
            {
                if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IValueInterface<>))
                {
                    var valueType = item.GetGenericArguments()[0];

                    return (ValueInterface)typeof(XValueInterface<>)
                        .MakeGenericType(valueType)
                        .GetConstructors()
                        .First()
                        .Invoke(new object?[] { valueInterface });
                }
            }

            throw new ArgumentException("Does not implement IValueInterface<T>.", nameof(valueInterface));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OnCannotReadValue(IXFieldRW fieldRW, IValueWriter valueWriter)
        {
            if (fieldRW.CannotGetException)
            {
                throw new MemberAccessException($@"Member : ""{fieldRW.MemberInfo.DeclaringType!.FullName}.{fieldRW.MemberInfo.Name}"" is not {"readable"}.");
            }
            else
            {
                valueWriter.DirectWrite(null);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OnCannotWriteValue(IXFieldRW fieldRW, IValueReader valueReader)
        {
            if (fieldRW.CannotGetException)
            {
                throw new MemberAccessException($@"Member : ""{fieldRW.MemberInfo.DeclaringType!.FullName}.{fieldRW.MemberInfo.Name}"" is not {"writable"}.");
            }
            else
            {
                valueReader.Pop();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static object? CannotReadValue(IXFieldRW fieldRW)
        {
            if (fieldRW.CannotGetException)
            {
                throw new MemberAccessException($@"Member : ""{fieldRW.MemberInfo.DeclaringType!.FullName}.{fieldRW.MemberInfo.Name}"" is not {"readable"}.");
            }
            else
            {
                return null;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T? CannotReadValue<T>(IXFieldRW fieldRW)
        {
            if (fieldRW.CannotGetException)
            {
                throw new MemberAccessException($@"Member : ""{fieldRW.MemberInfo.DeclaringType!.FullName}.{fieldRW.MemberInfo.Name}"" is not  {"readable"} .");
            }
            else
            {
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CannotWriteValue(IXFieldRW fieldRW)
        {
            if (fieldRW.CannotGetException)
            {
                throw new MemberAccessException($@"Member : ""{fieldRW.MemberInfo.DeclaringType!.FullName}.{fieldRW.MemberInfo.Name}"" is not {"writable"}.");
            }
        }


        sealed class XValueInterface<TValue> : ValueInterface
        {
            readonly IValueInterface<TValue> valueInterface;

            public XValueInterface(IValueInterface<TValue> valueInterface)
            {
                this.valueInterface = valueInterface;
            }

            public override bool IsDefaultBehaviorInternal => false;

            public override object Interface => valueInterface;

            public override Type Type => typeof(TValue);

            public override object? Read(IValueReader valueReader)
            {
                return valueInterface.ReadValue(valueReader);
            }

            public override void Write(IValueWriter valueWriter, object? value)
            {
                valueInterface.WriteValue(valueWriter, (TValue?)value);
            }
        }

        sealed class XConvertValueInterface<TSource, TDestination> : IValueInterface<TSource>
        {
            readonly IValueInterface<TDestination> valueInterface;

            public XConvertValueInterface(IValueInterface<TDestination> valueInterface)
            {
                this.valueInterface = valueInterface;
            }

            public TSource? ReadValue(IValueReader valueReader)
            {
                return XConvert.Convert<TDestination, TSource>(valueInterface.ReadValue(valueReader));
            }

            public void WriteValue(IValueWriter valueWriter, TSource? value)
            {
                valueInterface.WriteValue(valueWriter, XConvert.Convert<TSource, TDestination>(value));
            }
        }

        sealed class XDelegateValueInterface<TValue, TReadType, TWriteType> : IValueInterface<TValue>
        {
            readonly Func<IValueReader, TReadType?>? readValue;
            readonly Action<IValueWriter, TWriteType?>? writeValue;

            public XDelegateValueInterface(object? firstArgument, MethodInfo? readValueMethod, MethodInfo? writeValueMethod)
            {
                if (readValueMethod is not null)
                {
                    if (firstArgument is not null)
                    {
                        readValue = (Func<IValueReader, TReadType?>)Delegate.CreateDelegate(typeof(Func<IValueReader, TReadType?>), firstArgument, readValueMethod);
                    }
                    else
                    {
                        readValue = (Func<IValueReader, TReadType?>)Delegate.CreateDelegate(typeof(Func<IValueReader, TReadType?>), readValueMethod);
                    }
                }

                if (writeValueMethod is not null)
                {
                    if (firstArgument is not null)
                    {
                        writeValue = (Action<IValueWriter, TWriteType?>)Delegate.CreateDelegate(typeof(Action<IValueWriter, TWriteType?>), firstArgument, writeValueMethod);
                    }
                    else
                    {
                        writeValue = (Action<IValueWriter, TWriteType?>)Delegate.CreateDelegate(typeof(Action<IValueWriter, TWriteType?>), writeValueMethod);
                    }
                }
            }

            public TValue? ReadValue(IValueReader valueReader)
            {
                if (readValue is not null)
                {
                    return XConvert.Convert<TReadType, TValue>(readValue(valueReader));
                }
                else
                {
                    return ValueInterface.ReadValue<TValue>(valueReader);
                }
            }

            public void WriteValue(IValueWriter valueWriter, TValue? value)
            {
                if (writeValue is not null)
                {
                    writeValue(valueWriter, XConvert.Convert<TValue, TWriteType>(value));
                }
                else
                {
                    ValueInterface.WriteValue<TValue>(valueWriter, value);
                }
            }
        }
    }
}