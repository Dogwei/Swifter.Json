using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using static Swifter.RW.StaticFastObjectRW;

namespace Swifter.RW
{
    internal static class StaticFastObjectRW
    {
        public const BindingFlags StaticDeclaredOnly = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static readonly FieldInfo ValueFilterInfoValueCopyerField = typeof(ValueFilterInfo<string>).GetField(nameof(ValueFilterInfo<string>.ValueCopyer));
        public static readonly FieldInfo ValueFilterInfoKeyField = typeof(ValueFilterInfo<string>).GetField(nameof(ValueFilterInfo<string>.Key));
        public static readonly FieldInfo ValueFilterInfoTypeField = typeof(ValueFilterInfo<string>).GetField(nameof(ValueFilterInfo<string>.Type));

        public static readonly MethodInfo CreateInstanceFromTypeMethod = typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new Type[] { typeof(Type) });
        
        public static readonly MethodInfo IValueFilterFilterMethod = typeof(IValueFilter<string>).GetMethod(nameof(IValueFilter<string>.Filter));
        public static readonly MethodInfo ValueCopyerWriteToMethod = typeof(ValueCopyer).GetMethod(nameof(ValueCopyer.WriteTo));
        public static readonly MethodInfo IDataReaderIndexerGetMethod = typeof(IDataReader<string>).GetProperty(new Type[] { typeof(string) }).GetGetMethod(true);
        public static readonly MethodInfo IDataWriterIndexerGetMethod = typeof(IDataWriter<string>).GetProperty(new Type[] { typeof(string) }).GetGetMethod(true);

        public static readonly MethodInfo CharToUpperMethod = typeof(StringHelper).GetMethod(nameof(StringHelper.ToUpper), new Type[] { typeof(char) });

        public static readonly MethodInfo CharsGetHashCode = typeof(StringHelper).GetMethod(nameof(StringHelper.GetHashCode), new Type[] { typeof(char*), typeof(int) });
        public static readonly MethodInfo CharsGetUpperedHashCode = typeof(StringHelper).GetMethod(nameof(StringHelper.GetUpperedHashCode), new Type[] { typeof(char*), typeof(int) });
        public static readonly MethodInfo CharsGetLoweredHashCode = typeof(StringHelper).GetMethod(nameof(StringHelper.GetLoweredHashCode), new Type[] { typeof(char*), typeof(int) });

        public static readonly MethodInfo CharsVsStringEquals = typeof(StringHelper).GetMethod(nameof(StringHelper.Equals), new Type[] { typeof(char*), typeof(int), typeof(string) });
        public static readonly MethodInfo CharsVsStringIgnoreCaseEqualsByUpper = typeof(StringHelper).GetMethod(nameof(StringHelper.IgnoreCaseEqualsByUpper), new Type[] { typeof(char*), typeof(int), typeof(string) });
        public static readonly MethodInfo CharsVsStringIgnoreCaseEqualsByLower = typeof(StringHelper).GetMethod(nameof(StringHelper.IgnoreCaseEqualsByLower), new Type[] { typeof(char*), typeof(int), typeof(string) });

        public static readonly MethodInfo AllocateMethod = typeof(TypeHelper).GetMethod(nameof(TypeHelper.Allocate), new Type[] { typeof(Type) });

        public static readonly MethodInfo GetTypeHandle_Object = typeof(TypeHelper).GetMethod(nameof(TypeHelper.GetTypeHandle), new Type[] { typeof(object) });
        public static readonly MethodInfo GetInterface_Object = typeof(ValueInterface).GetMethod(nameof(ValueInterface.GetInterface), new Type[] { typeof(object) });
        public static readonly MethodInfo Write_IValueWriter_Object = typeof(ValueInterface).GetMethod(nameof(ValueInterface.Write), new Type[] { typeof(IValueWriter), typeof(object) });

        public static readonly ConstructorInfo MemberAccessException_String_Constructor = typeof(MemberAccessException).GetConstructor(new Type[] { typeof(string) });
        public static readonly ConstructorInfo MissingMemberException_String_String_Constructor = typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string), typeof(string) });
        public static readonly ConstructorInfo MissingMemberException_Constructor = typeof(MissingMemberException).GetConstructor(new Type[] { });
        public static readonly ConstructorInfo String_PChar_Length_Constructor = typeof(string).GetConstructor(new Type[] { typeof(char*), typeof(int), typeof(int) });

        public static readonly bool DynamicAssemblyCanAccessNonPublicTypes;
        public static readonly bool DynamicAssemblyCanAccessNonPublicMembers;

        public static readonly Random RandomInstance = new Random();

        static StaticFastObjectRW()
        {
            if (VersionDifferences.DynamicAssemblyCanAccessNonPublicTypes == null)
            {
                try
                {
                    DynamicAssembly.DefineType(nameof(TestClass) + 1, TypeAttributes.Public, typeof(TestClass)).CreateTypeInfo();

                    DynamicAssemblyCanAccessNonPublicTypes = true;
                }
                catch (Exception)
                {
                    DynamicAssemblyCanAccessNonPublicTypes = false;
                }
            }
            else
            {
                DynamicAssemblyCanAccessNonPublicTypes = VersionDifferences.DynamicAssemblyCanAccessNonPublicTypes.Value;
            }

            if (VersionDifferences.DynamicAssemblyCanAccessNonPublicMembers == null)
            {
                try
                {
                    var dynamicMethodName = nameof(TestClass.TestMethod);

                    var typeBuilder = DynamicAssembly.DefineType(nameof(TestClass) + 2, TypeAttributes.Public);

                    var methodBuilder = typeBuilder.DefineMethod(
                        dynamicMethodName,
                        MethodAttributes.Public | MethodAttributes.Static,
                        CallingConventions.Standard,
                        typeof(void),
                        Type.EmptyTypes);

                    var ilGen = methodBuilder.GetILGenerator();

                    ilGen.Call(typeof(TestClass).GetMethod(nameof(TestClass.TestMethod), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));
                    ilGen.Return();

                    var method = typeBuilder.CreateTypeInfo().GetMethod(dynamicMethodName);

                    method.Invoke(null, null);

                    DynamicAssemblyCanAccessNonPublicMembers = true;
                }
                catch (Exception)
                {
                    DynamicAssemblyCanAccessNonPublicMembers = false;
                }
            }
            else
            {
                DynamicAssemblyCanAccessNonPublicMembers = VersionDifferences.DynamicAssemblyCanAccessNonPublicMembers.Value;
            }
        }

        private class TestClass
        {
            internal static void TestMethod()
            {

            }
        }
    }

    internal static partial class StaticFastObjectRW<T>
    {
        public static readonly string[] Keys;

        public static readonly BaseField[] Fields;

        public static readonly IFastObjectRWCreater<T> Creater;

        public static readonly FastObjectRWOptions Options;

        public static readonly FieldInfo ContentField;

        public static readonly MethodInfo GetValueInterfaceInstanceMethod;

        public static readonly bool HaveNonPublicMember;

        public static readonly int LongestKeyLength;

        public static readonly long Mult;

        private static void GetFields(Type type, Dictionary<string, BaseField> dicFields, ref bool haveNonPublicMember)
        {
            if ((Options & FastObjectRWOptions.InheritedMembers) != 0)
            {
                var baseType = type.BaseType;

                if (baseType != null && baseType != typeof(object))
                {
                    GetFields(baseType, dicFields, ref haveNonPublicMember);
                }
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);


            if (fields != null && fields.Length != 0)
            {
                foreach (var item in fields)
                {
                    var attributes = item.GetCustomAttributes(typeof(RWFieldAttribute), true);

                    if (attributes != null && attributes.Length != 0)
                    {
                        foreach (var attribute in attributes)
                        {
                            var attributedField = new FastField(item, (RWFieldAttribute)attribute);

                            if (attributedField.CanRead || attributedField.CanWrite)
                            {
                                dicFields[attributedField.Name] = attributedField;

                                if (!attributedField.IsPublic)
                                {
                                    haveNonPublicMember = true;
                                }
                            }
                        }
                    }
                    else if ((Options & FastObjectRWOptions.Field) != 0 && item.IsPublic)
                    {
                        var field = new FastField(item, null);

                        dicFields[field.Name] = field;
                    }
                }
            }


            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (properties != null && properties.Length != 0)
            {
                foreach (var item in properties)
                {
                    var indexParameters = item.GetIndexParameters();

                    if (indexParameters != null && indexParameters.Length != 0)
                    {
                        /* Ignore Indexer. */
                        continue;
                    }

                    var attributes = item.GetCustomAttributes(typeof(RWFieldAttribute), true);

                    if (attributes != null && attributes.Length != 0)
                    {
                        foreach (var attribute in attributes)
                        {
                            var attributedField = new FastProperty(item, (RWFieldAttribute)attribute);

                            if (attributedField.CanRead || attributedField.CanWrite)
                            {
                                dicFields[attributedField.Name] = attributedField;

                                if (!attributedField.IsPublic)
                                {
                                    haveNonPublicMember = true;
                                }
                            }
                        }
                    }
                    else if ((Options & FastObjectRWOptions.Property) != 0)
                    {
                        var field = new FastProperty(item, null);

                        if (field.CanRead || field.CanWrite)
                        {
                            dicFields[field.Name] = field;
                        }
                    }
                }
            }
        }

        private static void Switch(ref FastObjectRWOptions options, FastObjectRWOptions target, RWBoolean boolean)
        {
            switch (boolean)
            {
                case RWBoolean.None:
                    break;
                case RWBoolean.Yes:
                    options |= target;
                    break;
                case RWBoolean.No:
                    options &= ~target;
                    break;
            }
        }

        private static bool ChackMult()
        {
            var cache = new IdCache<bool>();

            foreach (var key in Keys)
            {
                if (!cache.TryAdd(ComputeId64(key), true))
                {
                    return false;
                }
            }

            return true;
        }

        static StaticFastObjectRW()
        {
            try
            {
                var type = typeof(T);

                var attributes = type.GetDefinedAttributes<RWObjectAttribute>(true);

                #region -- Attribute Options --

                if (attributes.Length != 0)
                {
                    var options = FastObjectRW<T>.CurrentOptions;

                    foreach (var item in attributes)
                    {
                        Switch(ref options, FastObjectRWOptions.IgnoreCase, item.IgnoreCace);

                        Switch(ref options, FastObjectRWOptions.NotFoundException, item.NotFoundException);

                        Switch(ref options, FastObjectRWOptions.CannotGetException, item.CannotGetException);

                        Switch(ref options, FastObjectRWOptions.CannotSetException, item.CannotSetException);

                        Switch(ref options, FastObjectRWOptions.Property, item.IncludeProperties);

                        Switch(ref options, FastObjectRWOptions.Field, item.IncludeFields);

                        Switch(ref options, FastObjectRWOptions.SkipDefaultValue, item.SkipDefaultValue);

                        Switch(ref options, FastObjectRWOptions.MembersOptIn, item.MembersOptIn);
                    }

                    FastObjectRW<T>.CurrentOptions = options;
                }

                #endregion

                Options = FastObjectRW.SetToInitialized(type);

                var fieldsMap = new Dictionary<string, BaseField>();

                GetFields(type, fieldsMap, ref HaveNonPublicMember);

                var fields = fieldsMap.Values.ToList();

                fields.Sort((x, y) =>
                {
                    var com = x.Order.CompareTo(y.Order);

                    if (com != 0)
                    {
                        return com;
                    }

                    return x.Name.CompareTo(y.Name);
                });

                if (attributes.Length != 0)
                {
                    var temp = fields.Cast<IObjectField>().ToList();

                    foreach (var item in attributes)
                    {
                        item.OnCreate(type, ref temp);
                    }

                    fields = temp.Cast<BaseField>().ToList();
                }

                Fields = fields.ToArray();

                Keys = ArrayHelper.Filter(Fields, (Item) => { return true; }, (Item) => { return Item.Name; });

                foreach (var item in Keys) LongestKeyLength = Math.Max(LongestKeyLength, item.Length);

                do Mult = RandomInstance.Next(int.MinValue, int.MaxValue) & long.MaxValue; while (!ChackMult());

                ContentField = typeof(FastObjectRW<T>).GetField(nameof(FastObjectRW<T>.content), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                GetValueInterfaceInstanceMethod = typeof(FastObjectRW<T>).GetMethod(nameof(FastObjectRW<T>.GetValueInterfaceInstance), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                Creater = CreateCreater();
            }
            catch (Exception e)
            {
                Creater = new ErrorFastObjectRWCreater<T>(e);
            }
            finally
            {
                GC.Collect();
            }
        }

        public static long ComputeId64(IEnumerable<char> symbols)
        {
            long r = 0;

            if ((Options & FastObjectRWOptions.IgnoreCase) != 0)
            {
                foreach (var item in symbols)
                {
                    r ^= StringHelper.ToUpper(item) * Mult;
                }
            }
            else
            {
                foreach (var item in symbols)
                {
                    r ^= item * Mult;
                }
            }

            return r;
        }

        public static void LoadContent(ILGenerator ilGen)
        {
            ilGen.LoadArgument(0);

            if (typeof(T).IsValueType)
            {
                ilGen.LoadFieldAddress(ContentField);
            }
            else
            {
                ilGen.LoadField(ContentField);
            }
        }



        public static string GetReadValueMethodName(Type type)
        {
            if ((Options & FastObjectRWOptions.BasicTypeDirectCallMethod) == 0)
            {
                return null;
            }

            if (type.IsEnum)
            {
                return null;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return ValueInterface<bool>.IsNoModify ? nameof(IValueReader.ReadBoolean) : null;
                case TypeCode.Char:
                    return ValueInterface<char>.IsNoModify ? nameof(IValueReader.ReadChar) : null;
                case TypeCode.SByte:
                    return ValueInterface<sbyte>.IsNoModify ? nameof(IValueReader.ReadSByte) : null;
                case TypeCode.Byte:
                    return ValueInterface<byte>.IsNoModify ? nameof(IValueReader.ReadByte) : null;
                case TypeCode.Int16:
                    return ValueInterface<short>.IsNoModify ? nameof(IValueReader.ReadInt16) : null;
                case TypeCode.UInt16:
                    return ValueInterface<ushort>.IsNoModify ? nameof(IValueReader.ReadUInt16) : null;
                case TypeCode.Int32:
                    return ValueInterface<int>.IsNoModify ? nameof(IValueReader.ReadInt32) : null;
                case TypeCode.UInt32:
                    return ValueInterface<uint>.IsNoModify ? nameof(IValueReader.ReadUInt32) : null;
                case TypeCode.Int64:
                    return ValueInterface<long>.IsNoModify ? nameof(IValueReader.ReadInt64) : null;
                case TypeCode.UInt64:
                    return ValueInterface<ulong>.IsNoModify ? nameof(IValueReader.ReadUInt64) : null;
                case TypeCode.Single:
                    return ValueInterface<float>.IsNoModify ? nameof(IValueReader.ReadSingle) : null;
                case TypeCode.Double:
                    return ValueInterface<double>.IsNoModify ? nameof(IValueReader.ReadDouble) : null;
                case TypeCode.Decimal:
                    return ValueInterface<decimal>.IsNoModify ? nameof(IValueReader.ReadDecimal) : null;
                case TypeCode.DateTime:
                    return ValueInterface<DateTime>.IsNoModify ? nameof(IValueReader.ReadDateTime) : null;
                case TypeCode.String:
                    return ValueInterface<string>.IsNoModify ? nameof(IValueReader.ReadString) : null;
            }

            return null;
        }

        public static string GetWriteValueMethodName(Type type)
        {
            if ((Options & FastObjectRWOptions.BasicTypeDirectCallMethod) == 0)
            {
                return null;
            }

            if (type.IsEnum)
            {
                return null;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return ValueInterface<bool>.IsNoModify ? nameof(IValueWriter.WriteBoolean) : null;
                case TypeCode.Char:
                    return ValueInterface<char>.IsNoModify ? nameof(IValueWriter.WriteChar) : null;
                case TypeCode.SByte:
                    return ValueInterface<sbyte>.IsNoModify ? nameof(IValueWriter.WriteSByte) : null;
                case TypeCode.Byte:
                    return ValueInterface<byte>.IsNoModify ? nameof(IValueWriter.WriteByte) : null;
                case TypeCode.Int16:
                    return ValueInterface<short>.IsNoModify ? nameof(IValueWriter.WriteInt16) : null;
                case TypeCode.UInt16:
                    return ValueInterface<ushort>.IsNoModify ? nameof(IValueWriter.WriteUInt16) : null;
                case TypeCode.Int32:
                    return ValueInterface<int>.IsNoModify ? nameof(IValueWriter.WriteInt32) : null;
                case TypeCode.UInt32:
                    return ValueInterface<uint>.IsNoModify ? nameof(IValueWriter.WriteUInt32) : null;
                case TypeCode.Int64:
                    return ValueInterface<long>.IsNoModify ? nameof(IValueWriter.WriteInt64) : null;
                case TypeCode.UInt64:
                    return ValueInterface<ulong>.IsNoModify ? nameof(IValueWriter.WriteUInt64) : null;
                case TypeCode.Single:
                    return ValueInterface<float>.IsNoModify ? nameof(IValueWriter.WriteSingle) : null;
                case TypeCode.Double:
                    return ValueInterface<double>.IsNoModify ? nameof(IValueWriter.WriteDouble) : null;
                case TypeCode.Decimal:
                    return ValueInterface<decimal>.IsNoModify ? nameof(IValueWriter.WriteDecimal) : null;
                case TypeCode.DateTime:
                    return ValueInterface<DateTime>.IsNoModify ? nameof(IValueWriter.WriteDateTime) : null;
                case TypeCode.String:
                    return ValueInterface<string>.IsNoModify ? nameof(IValueWriter.WriteString) : null;
            }

            return null;
        }


        public static IFastObjectRWCreater<T> CreateCreater()
        {
            var typeName = $"{nameof(FastObjectRW)}_{typeof(T).Name}_{Guid.NewGuid().ToString("N")}";

            var typeBuilder = DynamicAssembly.DefineType(
                typeName,
                TypeAttributes.Sealed | TypeAttributes.Public);

            if (typeof(T).IsVisible || DynamicAssemblyCanAccessNonPublicTypes)
            {
                typeBuilder.SetParent(typeof(FastObjectRW<T>));
            }
            else
            {
                /* Use generics to skip visibility check. */

                var genericParameters = typeBuilder.DefineGenericParameters("T");

                var baseType = typeof(FastObjectRW<>).MakeGenericType(genericParameters);

                typeBuilder.SetParent(baseType);
            }

            var callbackEvents = new CallbackEvents();

            ImplInitialize(typeBuilder, callbackEvents);

            ImplOnWriteValue(typeBuilder, callbackEvents);

            ImplOnReadValue(typeBuilder, callbackEvents);

            ImplOnReadAll(typeBuilder, callbackEvents);

            ImplOnWriteAll(typeBuilder, callbackEvents);

            ImplOnReadAllWithFilter(typeBuilder, callbackEvents);

            ImplId64OnReadValue(typeBuilder, callbackEvents);

            ImplId64OnWriteValue(typeBuilder, callbackEvents);

            ImplGetId64(typeBuilder, callbackEvents);



            Type rtType = typeBuilder.CreateTypeInfo();

            if (rtType.IsGenericTypeDefinition)
            {
                rtType = rtType.MakeGenericType(typeof(T));
            }

            callbackEvents.OnTypeCreated(rtType);

            
            if (typeof(T).IsVisible || (DynamicAssemblyCanAccessNonPublicTypes && DynamicAssemblyCanAccessNonPublicMembers))
            {
                var createrBuilder = DynamicAssembly.DefineType(
                    $"{typeName}_Creater",
                    TypeAttributes.Sealed | TypeAttributes.Public
                );

                createrBuilder.AddInterfaceImplementation(typeof(IFastObjectRWCreater<T>));
                createrBuilder.AddInterfaceImplementation(typeof(IValueInterface<T>));

                ImplCreate(createrBuilder, rtType);

                ImplReadValue(createrBuilder, rtType);

                ImplWriteValue(createrBuilder, rtType);

                var createrType = createrBuilder.CreateTypeInfo();

                var creater = Activator.CreateInstance(createrType);

                if (ValueInterface<T>.Content is FastObjectInterface<T>)
                {
                    ValueInterface<T>.SetInterface((IValueInterface<T>)creater);
                }

                return (IFastObjectRWCreater<T>)creater;
            }

            return new NonPublicFastObjectRWCreater<T>(rtType);
        }

        public static void ImplInitialize(TypeBuilder typeBuilder, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.Initialize),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                Type.EmptyTypes);


            if (typeof(T).IsVisible)
            {
                ImplInitialize(methodBuilder.GetILGenerator());
            }
            else
            {
                var delegateType = typeof(Action<IntPtr>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplInitialize) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                if (typeof(T).IsValueType)
                {
                    ilGen.Return();

                    return;
                }

                FixedArg0(ilGen);

                ilGen.LoadField(delegateField);
                ilGen.LoadArgument(0);
                ilGen.Call(delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Return();

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(FastObjectRW<T>.Initialize),
                        typeof(void),
                        new Type[] { typeof(IntPtr) },
                        typeof(FastObjectRW<T>).Module, true);


                    ImplInitialize(dynamicMethod.GetILGenerator());

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplOnWriteValue(TypeBuilder typeBuilder, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnWriteValue),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(string), typeof(IValueReader) });

            if (typeof(T).IsVisible && (!HaveNonPublicMember))
            {
                ImplOnWriteValue(methodBuilder.GetILGenerator());
            }
            else
            {
                var delegateType = typeof(Action<IntPtr, string, IValueReader>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplOnWriteValue) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                FixedArg0(ilGen);

                ilGen.LoadField(delegateField);
                ilGen.LoadArgument(0);
                ilGen.LoadArgument(1);
                ilGen.LoadArgument(2);
                ilGen.Call(delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Return();

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(FastObjectRW<T>.OnWriteValue),
                        typeof(void),
                        new Type[] { typeof(IntPtr), typeof(string), typeof(IValueReader) },
                        typeof(FastObjectRW<T>).Module, true);

                    ImplOnWriteValue(dynamicMethod.GetILGenerator());

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplOnReadAll(TypeBuilder typeBuilder, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadAll),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IDataWriter<string>) });

            if (typeof(T).IsVisible && (!HaveNonPublicMember))
            {
                ImplOnReadAll(methodBuilder.GetILGenerator());
            }
            else
            {
                var delegateType = typeof(Action<IntPtr, IDataWriter<string>>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplOnReadAll) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                FixedArg0(ilGen);

                ilGen.LoadField(delegateField);
                ilGen.LoadArgument(0);
                ilGen.LoadArgument(1);
                ilGen.Call(delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Return();

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(FastObjectRW<T>.OnReadAll),
                        typeof(void),
                        new Type[] { typeof(IntPtr), typeof(IDataWriter<string>) },
                        typeof(FastObjectRW<T>).Module, true);


                    ImplOnReadAll(dynamicMethod.GetILGenerator());

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplOnWriteAll(TypeBuilder typeBuilder, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnWriteAll),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IDataReader<string>) });


            if (typeof(T).IsVisible && (!HaveNonPublicMember))
            {
                ImplOnWriteAll(methodBuilder.GetILGenerator());
            }
            else
            {
                var delegateType = typeof(Action<IntPtr, IDataReader<string>>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplOnWriteAll) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                FixedArg0(ilGen);

                ilGen.LoadField(delegateField);
                ilGen.LoadArgument(0);
                ilGen.LoadArgument(1);
                ilGen.Call(delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Return();

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(FastObjectRW<T>.OnWriteAll),
                        typeof(void),
                        new Type[] { typeof(IntPtr), typeof(IDataReader<string>) },
                        typeof(FastObjectRW<T>).Module, true);


                    ImplOnWriteAll(dynamicMethod.GetILGenerator());

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplOnReadAllWithFilter(TypeBuilder typeBuilder, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadAll),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IDataWriter<string>), typeof(IValueFilter<string>) });

            if (typeof(T).IsVisible && (!HaveNonPublicMember))
            {
                ImplOnReadAllWithFilter(methodBuilder.GetILGenerator());
            }
            else
            {
                var delegateType = typeof(Action<IntPtr, IDataWriter<string>, IValueFilter<string>>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplOnReadAllWithFilter) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                FixedArg0(ilGen);

                ilGen.LoadField(delegateField);
                ilGen.LoadArgument(0);
                ilGen.LoadArgument(1);
                ilGen.LoadArgument(2);
                ilGen.Call(delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Return();

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(ImplOnReadAllWithFilter),
                        typeof(void),
                        new Type[] { typeof(IntPtr), typeof(IDataWriter<string>), typeof(IValueFilter<string>) },
                        typeof(FastObjectRW<T>).Module, true);


                    ImplOnReadAllWithFilter(dynamicMethod.GetILGenerator());

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplOnReadValue(TypeBuilder typeBuilder, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadValue),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(string), typeof(IValueWriter) });

            if (typeof(T).IsVisible && (!HaveNonPublicMember))
            {
                ImplOnReadValue(methodBuilder.GetILGenerator());
            }
            else
            {
                var delegateType = typeof(Action<IntPtr, string, IValueWriter>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplOnReadValue) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                FixedArg0(ilGen);

                ilGen.LoadField(delegateField);
                ilGen.LoadArgument(0);
                ilGen.LoadArgument(1);
                ilGen.LoadArgument(2);
                ilGen.Call(delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Return();

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(ImplOnReadValue),
                        typeof(void),
                        new Type[] { typeof(IntPtr), typeof(string), typeof(IValueWriter) },
                        typeof(FastObjectRW<T>).Module, true);


                    ImplOnReadValue(dynamicMethod.GetILGenerator());

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplId64OnReadValue(TypeBuilder typeBuilder, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadValue),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(long), typeof(IValueWriter) });

            if (typeof(T).IsVisible && (!HaveNonPublicMember))
            {
                ImplId64OnReadValue(methodBuilder.GetILGenerator());
            }
            else
            {
                var delegateType = typeof(Action<IntPtr, long, IValueWriter>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplId64OnReadValue) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                FixedArg0(ilGen);

                ilGen.LoadField(delegateField);
                ilGen.LoadArgument(0);
                ilGen.LoadArgument(1);
                ilGen.LoadArgument(2);
                ilGen.Call(delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Return();

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(ImplOnReadValue),
                        typeof(void),
                        new Type[] { typeof(IntPtr), typeof(long), typeof(IValueWriter) },
                        typeof(FastObjectRW<T>).Module, true);


                    ImplId64OnReadValue(dynamicMethod.GetILGenerator());

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplId64OnWriteValue(TypeBuilder typeBuilder, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnWriteValue),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(long), typeof(IValueReader) });

            if (typeof(T).IsVisible && (!HaveNonPublicMember))
            {
                ImplId64OnWriteValue(methodBuilder.GetILGenerator());
            }
            else
            {
                var delegateType = typeof(Action<IntPtr, long, IValueReader>);

                var delegateField = typeBuilder.DefineField(
                    nameof(ImplId64OnWriteValue) + nameof(Delegate),
                    delegateType,
                    FieldAttributes.Public | FieldAttributes.Static);

                var ilGen = methodBuilder.GetILGenerator();

                FixedArg0(ilGen);

                ilGen.LoadField(delegateField);
                ilGen.LoadArgument(0);
                ilGen.LoadArgument(1);
                ilGen.LoadArgument(2);
                ilGen.Call(delegateType.GetMethod(nameof(Action.Invoke)));
                ilGen.Return();

                callbackEvents.TypeCreated += type =>
                {
                    var dynamicMethod = new DynamicMethod(
                        typeBuilder.Name + "_" + nameof(FastObjectRW<T>.OnWriteValue),
                        typeof(void),
                        new Type[] { typeof(IntPtr), typeof(long), typeof(IValueReader) },
                        typeof(FastObjectRW<T>).Module, true);

                    ImplId64OnWriteValue(dynamicMethod.GetILGenerator());

                    var dynamicDelegate = dynamicMethod.CreateDelegate(delegateType);

                    type.GetField(delegateField.Name).SetValue(null, dynamicDelegate);
                };
            }
        }

        public static void ImplGetId64(TypeBuilder typeBuilder, CallbackEvents callbackEvents)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.GetId64),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(long),
                new Type[] { typeof(char).MakeByRefType(), typeof(int) });

            ImplGetId64(methodBuilder.GetILGenerator());
        }





        public static void FixedArg0(ILGenerator ilGen)
        {
            var arg0 = ilGen.DeclareLocal(typeof(IntPtr).MakeByRefType(), true);
            ilGen.LoadArgument(0);
            ilGen.ConvertPointer();
            ilGen.StoreLocal(arg0);
        }

        public static void ImplInitialize(ILGenerator ilGen)
        {
            if (typeof(T).IsValueType)
            {
                ilGen.Return();
            }
            else if ((Options & FastObjectRWOptions.Allocate) != 0)
            {
                ilGen.LoadArgument(0);

                ilGen.LoadType(typeof(T));

                ilGen.Call(AllocateMethod);

                ilGen.StoreField(ContentField);
                ilGen.Return();
            }
            else
            {
                ilGen.LoadArgument(0);

                var Constructor = typeof(T).GetConstructor(Type.EmptyTypes);

                if (Constructor == null)
                {
                    ilGen.LoadType(typeof(T));
                    ilGen.Call(CreateInstanceFromTypeMethod);
                }
                else
                {
                    ilGen.NewObject(Constructor);
                }

                ilGen.StoreField(ContentField);
                ilGen.Return();
            }
        }

        public static void ImplOnReadAll(ILGenerator ilGen)
        {
            var locals = new TypeCache<LocalBuilder>();

            foreach (var field in Fields)
            {
                if ((Options & FastObjectRWOptions.MembersOptIn) != 0 && field.Attribute == null)
                {
                    continue;
                }

                if (field.CanRead)
                {
                    if ((Options & FastObjectRWOptions.SkipDefaultValue) != 0)
                    {
                        var local = locals.GetOrAdd(field.BeforeType, t => ilGen.DeclareLocal(t));

                        field.GetValueBefore(ilGen);
                        field.GetValueAfter(ilGen);

                        ilGen.StoreLocal(local);

                        var isDefaultValue = ilGen.DefineLabel();

                        ilGen.BranchDefaultValue(local, isDefaultValue);

                        field.WriteValueBefore(ilGen);

                        ilGen.LoadArgument(1);
                        ilGen.LoadString(field.Name);
                        ilGen.Call(IDataWriterIndexerGetMethod);

                        ilGen.LoadLocal(local);

                        field.WriteValueAfter(ilGen);

                        ilGen.MarkLabel(isDefaultValue);
                    }
                    else
                    {
                        field.WriteValueBefore(ilGen);

                        ilGen.LoadArgument(1);
                        ilGen.LoadString(field.Name);
                        ilGen.Call(IDataWriterIndexerGetMethod);

                        field.GetValueBefore(ilGen);

                        field.GetValueAfter(ilGen);

                        field.WriteValueAfter(ilGen);
                    }
                }
            }

            ilGen.Return();
        }

        public static void ImplOnWriteAll(ILGenerator ilGen)
        {
            var fields = Fields;

            foreach (var field in fields)
            {
                if (field.CanWrite)
                {
                    field.SetValueBefore(ilGen);

                    field.ReadValueBefore(ilGen);

                    ilGen.LoadArgument(1);
                    ilGen.LoadString(field.Name);
                    ilGen.Call(IDataReaderIndexerGetMethod);

                    field.ReadValueAfter(ilGen);

                    field.SetValueAfter(ilGen);
                }
            }

            ilGen.Return();
        }

        public static void ImplOnReadAllWithFilter(ILGenerator ilGen)
        {
            var locals = new TypeCache<LocalBuilder>();

            var valueInfo = ilGen.DeclareLocal(typeof(ValueFilterInfo<string>));

            ilGen.NewObject(typeof(ValueFilterInfo<string>).GetConstructor(Type.EmptyTypes));

            ilGen.StoreLocal(valueInfo);

            foreach (var field in Fields)
            {
                if ((Options & FastObjectRWOptions.MembersOptIn) != 0 && field.Attribute == null)
                {
                    continue;
                }

                if (field.CanRead)
                {
                    if ((Options & FastObjectRWOptions.SkipDefaultValue) != 0)
                    {
                        var local = locals.GetOrAdd(field.BeforeType, t => ilGen.DeclareLocal(t));

                        field.GetValueBefore(ilGen);
                        field.GetValueAfter(ilGen);

                        ilGen.StoreLocal(local);

                        var skip = ilGen.DefineLabel();

                        ilGen.BranchDefaultValue(local, skip);

                        field.WriteValueBefore(ilGen);

                        ilGen.LoadLocal(valueInfo);
                        ilGen.LoadField(ValueFilterInfoValueCopyerField);

                        ilGen.LoadLocal(local);

                        field.WriteValueAfter(ilGen);

                        FilterItem(field, skip);

                        ilGen.MarkLabel(skip);
                    }
                    else
                    {
                        field.WriteValueBefore(ilGen);

                        ilGen.LoadLocal(valueInfo);
                        ilGen.LoadField(ValueFilterInfoValueCopyerField);

                        field.GetValueBefore(ilGen);

                        field.GetValueAfter(ilGen);

                        field.WriteValueAfter(ilGen);

                        var skip = ilGen.DefineLabel();

                        FilterItem(field, skip);

                        ilGen.MarkLabel(skip);
                    }
                }
            }

            ilGen.Return();

            void FilterItem(BaseField field, Label skip)
            {
                ilGen.LoadLocal(valueInfo);

                ilGen.LoadString(field.Name);

                ilGen.StoreField(ValueFilterInfoKeyField);

                ilGen.LoadLocal(valueInfo);
                
                ilGen.LoadType(field.AfterType);

                ilGen.StoreField(ValueFilterInfoTypeField);

                ilGen.LoadArgument(2);

                ilGen.LoadLocal(valueInfo);

                ilGen.Call(IValueFilterFilterMethod);

                ilGen.BranchFalse(skip);

                ilGen.LoadLocal(valueInfo);

                ilGen.LoadField(ValueFilterInfoValueCopyerField);

                ilGen.LoadArgument(1);

                ilGen.LoadLocal(valueInfo);

                ilGen.LoadField(ValueFilterInfoKeyField);

                ilGen.Call(IDataWriterIndexerGetMethod);

                ilGen.Call(ValueCopyerWriteToMethod);
            }
        }

        public static void ImplOnReadValue(ILGenerator ilGen)
        {
            var fields = Fields;

            var Cases = new CaseInfo<string>[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                Cases[i] = new CaseInfo<string>(fields[i].Name, ilGen.DefineLabel());
            }

            var DefaultLabel = ilGen.DefineLabel();

            ilGen.Switch((iLGen) => { iLGen.LoadArgument(1); }, Cases, DefaultLabel, (Options & FastObjectRWOptions.IgnoreCase) != 0);

            ilGen.MarkLabel(DefaultLabel);

            if ((Options & FastObjectRWOptions.NotFoundException) != 0)
            {
                ilGen.LoadString(typeof(T).Name);

                ilGen.LoadArgument(1);

                ilGen.NewObject(MissingMemberException_String_String_Constructor);

                ilGen.Throw();
            }
            else
            {
                ilGen.LoadArgument(2);

                ilGen.LoadNull();

                ilGen.Call(typeof(IValueWriter).GetMethod(nameof(IValueWriter.DirectWrite)));

                ilGen.Return();
            }

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var @case = Cases[i];

                ilGen.MarkLabel(@case.Label);

                if (field.CanRead)
                {
                    field.WriteValueBefore(ilGen);

                    ilGen.LoadArgument(2);

                    field.GetValueBefore(ilGen);

                    field.GetValueAfter(ilGen);

                    field.WriteValueAfter(ilGen);

                    ilGen.Return();
                }
                else
                {
                    if ((Options & FastObjectRWOptions.CannotGetException) != 0)
                    {
                        ilGen.LoadString($"This member '{field.Name}' no get method or cannot access.");

                        ilGen.NewObject(MemberAccessException_String_Constructor);

                        ilGen.Throw();
                    }
                    else
                    {
                        ilGen.LoadArgument(2);

                        ilGen.LoadNull();

                        ilGen.Call(typeof(IValueWriter).GetMethod(nameof(IValueWriter.DirectWrite)));

                        ilGen.Return();
                    }
                }
            }
        }

        public static void ImplOnWriteValue(ILGenerator ilGen)
        {
            var fields = Fields;

            var cases = new CaseInfo<string>[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                cases[i] = new CaseInfo<string>(fields[i].Name, ilGen.DefineLabel());
            }

            var DefaultLabel = ilGen.DefineLabel();

            ilGen.Switch((iLGen) => { iLGen.LoadArgument(1); }, cases, DefaultLabel, (Options & FastObjectRWOptions.IgnoreCase) != 0);

            ilGen.MarkLabel(DefaultLabel);

            if ((Options & FastObjectRWOptions.NotFoundException) != 0)
            {
                ilGen.LoadString(typeof(T).Name);
                ilGen.LoadArgument(1);
                ilGen.NewObject(MissingMemberException_String_String_Constructor);
                ilGen.Throw();
            }
            else
            {
                ilGen.LoadArgument(2);
                ilGen.Call(typeof(IValueReader).GetMethod(nameof(IValueReader.DirectRead)));
                ilGen.Pop();
                ilGen.Return();
            }

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var @case = cases[i];

                ilGen.MarkLabel(@case.Label);

                if (field.CanWrite)
                {
                    field.SetValueBefore(ilGen);

                    field.ReadValueBefore(ilGen);

                    ilGen.LoadArgument(2);

                    field.ReadValueAfter(ilGen);

                    field.SetValueAfter(ilGen);

                    ilGen.Return();
                }
                else
                {
                    if ((Options & FastObjectRWOptions.CannotSetException) != 0)
                    {
                        ilGen.LoadString($"This member '{field.Name}' no set method or cannot access.");
                        ilGen.NewObject(MemberAccessException_String_Constructor);
                        ilGen.Throw();
                    }
                    else
                    {
                        ilGen.LoadArgument(2);
                        ilGen.Call(typeof(IValueReader).GetMethod(nameof(IValueReader.DirectRead)));
                        ilGen.Pop();
                        ilGen.Return();
                    }
                }
            }
        }

        public static void ImplId64OnReadValue(ILGenerator ilGen)
        {
            var fields = Fields;

            var cases = new CaseInfo<long>[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                long value;

                if ((Options & FastObjectRWOptions.IndexId64) != 0)
                {
                    value = i;
                }
                else
                {
                    value = ComputeId64(fields[i].Name);
                }

                cases[i] = new CaseInfo<long>(value, ilGen.DefineLabel());
            }

            var DefaultLabel = ilGen.DefineLabel();

            ilGen.Switch((iLGen) => { iLGen.LoadArgument(1); }, cases, DefaultLabel);

            ilGen.MarkLabel(DefaultLabel);

            if ((Options & FastObjectRWOptions.NotFoundException) != 0)
            {
                ilGen.NewObject(MissingMemberException_Constructor);

                ilGen.Throw();
            }
            else
            {
                ilGen.LoadArgument(2);

                ilGen.LoadNull();

                ilGen.Call(typeof(IValueWriter).GetMethod(nameof(IValueWriter.DirectWrite)));

                ilGen.Return();
            }

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var @case = cases[i];

                ilGen.MarkLabel(@case.Label);

                if (field.CanRead)
                {
                    field.WriteValueBefore(ilGen);

                    ilGen.LoadArgument(2);

                    field.GetValueBefore(ilGen);

                    field.GetValueAfter(ilGen);

                    field.WriteValueAfter(ilGen);

                    ilGen.Return();
                }
                else
                {
                    if ((Options & FastObjectRWOptions.CannotGetException) != 0)
                    {
                        ilGen.LoadString($"This member '{field.Name}' no get method or cannot access.");

                        ilGen.NewObject(MemberAccessException_String_Constructor);

                        ilGen.Throw();
                    }
                    else
                    {
                        ilGen.LoadArgument(2);

                        ilGen.LoadNull();

                        ilGen.Call(typeof(IValueWriter).GetMethod(nameof(IValueWriter.DirectWrite)));

                        ilGen.Return();
                    }
                }
            }
        }

        public static void ImplId64OnWriteValue(ILGenerator ilGen)
        {
            var fields = Fields;

            var cases = new CaseInfo<long>[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                long value;

                if ((Options & FastObjectRWOptions.IndexId64) != 0)
                {
                    value = i;
                }
                else
                {
                    value = ComputeId64(fields[i].Name);
                }

                cases[i] = new CaseInfo<long>(value, ilGen.DefineLabel());
            }

            var label_default = ilGen.DefineLabel();

            ilGen.Switch((iLGen) => { iLGen.LoadArgument(1); }, cases, label_default);

            ilGen.MarkLabel(label_default);

            if ((Options & FastObjectRWOptions.NotFoundException) != 0)
            {
                ilGen.NewObject(MissingMemberException_Constructor);
                ilGen.Throw();
            }
            else
            {
                ilGen.LoadArgument(2);
                ilGen.Call(typeof(IValueReader).GetMethod(nameof(IValueReader.DirectRead)));
                ilGen.Pop();
                ilGen.Return();
            }

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var @case = cases[i];

                ilGen.MarkLabel(@case.Label);

                if (field.CanWrite)
                {
                    field.SetValueBefore(ilGen);

                    field.ReadValueBefore(ilGen);

                    ilGen.LoadArgument(2);

                    field.ReadValueAfter(ilGen);

                    field.SetValueAfter(ilGen);

                    ilGen.Return();
                }
                else
                {
                    if ((Options & FastObjectRWOptions.CannotSetException) != 0)
                    {
                        ilGen.LoadString($"This member '{field.Name}' no set method or cannot access.");
                        ilGen.NewObject(MemberAccessException_String_Constructor);
                        ilGen.Throw();
                    }
                    else
                    {
                        ilGen.LoadArgument(2);
                        ilGen.Call(typeof(IValueReader).GetMethod(nameof(IValueReader.DirectRead)));
                        ilGen.Pop();
                        ilGen.Return();
                    }
                }
            }
        }

        public static void ImplGetId64(ILGenerator ilGen)
        {
            if ((Options & FastObjectRWOptions.IndexId64) != 0)
            {
                ImplGetIndexId64(ilGen);

                return;
            }

            // long result;
            var local_result = ilGen.DeclareLocal(typeof(long));

            // int index;
            var local_index = ilGen.DeclareLocal(typeof(int));

            // result = 0L;
            ilGen.LoadConstant(0L);
            ilGen.StoreLocal(local_result);

            // index = 0;
            ilGen.LoadConstant(0);
            ilGen.StoreLocal(local_index);

            var label_loop = ilGen.DefineLabel();
            var label_return = ilGen.DefineLabel();

            ilGen.MarkLabel(label_loop);
            // if (index >= length) goto Return;
            ilGen.LoadLocal(local_index);
            ilGen.LoadArgument(2);
            ilGen.BranchIfGreaterOrEqual(label_return);

            // result = result ^ (chars[index] * Mult);
            ilGen.LoadLocal(local_result);
            ilGen.LoadArgument(1);
            ilGen.LoadLocal(local_index);
            ilGen.SizeOf(typeof(char));
            ilGen.Multiply();
            ilGen.Add();
            ilGen.LoadValue(typeof(char));
            if ((Options & FastObjectRWOptions.IgnoreCase) != 0)
            {
                ilGen.Call(CharToUpperMethod);
            }
            ilGen.ConvertInt64();
            ilGen.LoadConstant(Mult);
            ilGen.Multiply();
            ilGen.Xor();
            ilGen.StoreLocal(local_result);

            // index = index + 1;
            ilGen.LoadLocal(local_index);
            ilGen.LoadConstant(1);
            ilGen.Add();
            ilGen.StoreLocal(local_index);

            // goto Loop;
            ilGen.Branch(label_loop);

            // Return:
            ilGen.MarkLabel(label_return);

            // return result;
            ilGen.LoadLocal(local_result);
            ilGen.Return();
        }

        public static void ImplGetIndexId64(ILGenerator ilGen)
        {
            var fields = Fields;

            var cases = new CaseInfo<string>[fields.Length];

            var label_default = ilGen.DefineLabel();

            if ((Options & FastObjectRWOptions.IgnoreCase) != 0)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    cases[i] = new CaseInfo<string>(StringHelper.ToLower(fields[i].Name), ilGen.DefineLabel())
                    {
                        HashCode = StringHelper.GetLoweredHashCode(fields[i].Name)
                    };
                }

                ilGen.Switch(LoadValue, CharsGetLoweredHashCode, CharsVsStringIgnoreCaseEqualsByLower, cases, label_default, LoadCase);
            }
            else
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    cases[i] = new CaseInfo<string>(fields[i].Name, ilGen.DefineLabel())
                    {
                        HashCode = StringHelper.GetHashCode(fields[i].Name)
                    };
                }

                ilGen.Switch(LoadValue, CharsGetHashCode, CharsVsStringEquals, cases, label_default, LoadCase);
            }

            ilGen.MarkLabel(label_default);

            if ((Options & FastObjectRWOptions.NotFoundException) != 0)
            {
                ilGen.LoadString(typeof(T).Name);

                ilGen.LoadArgument(1);
                ilGen.LoadConstant(0);
                ilGen.LoadArgument(2);
                ilGen.NewObject(String_PChar_Length_Constructor);

                ilGen.NewObject(MissingMemberException_String_String_Constructor);
                ilGen.Throw();
            }
            else
            {
                ilGen.LoadConstant(-1L);

                ilGen.Return();
            }

            for (long i = 0; i < cases.Length; i++)
            {
                var field = fields[i];
                var @case = cases[i];

                ilGen.MarkLabel(@case.Label);

                ilGen.LoadConstant(i);
                ilGen.Return();
            }


            void LoadValue(ILGenerator iL)
            {
                iL.LoadArgument(1);
                iL.LoadArgument(2);
            }

            void LoadCase(ILGenerator iL, string value)
            {
                iL.LoadString(value);
            }
        }





        public static void ImplCreate(TypeBuilder typeBuilder, Type rtType)
        {
            var methodBuilder = typeBuilder.DefineMethod(
               nameof(IFastObjectRWCreater<T>.Create),
               MethodAttributes.Public | MethodAttributes.Virtual |
               MethodAttributes.Final,
               CallingConventions.HasThis, 
               typeof(FastObjectRW<T>),
               Type.EmptyTypes);

            ImplCreate(methodBuilder.GetILGenerator(), rtType);
        }

        public static void ImplWriteValue(TypeBuilder typeBuilder, Type rtType)
        {
            var methodBuilder = typeBuilder.DefineMethod(
               nameof(IValueInterface<T>.WriteValue),
               MethodAttributes.Public | MethodAttributes.Virtual |
               MethodAttributes.Final,
               CallingConventions.HasThis,
               typeof(void),
               new Type[] { typeof(IValueWriter), typeof(T) });

            ImplWriteValue(methodBuilder.GetILGenerator(), rtType);
        }

        public static void ImplReadValue(TypeBuilder typeBuilder, Type rtType)
        {
            var methodBuilder = typeBuilder.DefineMethod(
               nameof(IValueInterface<T>.ReadValue),
               MethodAttributes.Public | MethodAttributes.Virtual |
               MethodAttributes.Final,
               CallingConventions.HasThis,
               typeof(T),
               new Type[] { typeof(IValueReader) });

            ImplReadValue(methodBuilder.GetILGenerator(), rtType);
        }


        public static void ImplCreate(ILGenerator ilGen, Type rtType)
        {
            ilGen.NewObject(rtType.GetConstructor(Type.EmptyTypes));
            ilGen.Return();
        }

        public static void ImplWriteValue(ILGenerator ilGen, Type rtType)
        {
            var label_write = ilGen.DefineLabel();

            if (!TypeInfo<T>.IsValueType)
            {
                ilGen.LoadArgument(2);
                ilGen.BranchTrue(label_write);
                ilGen.LoadArgument(1);
                ilGen.LoadNull();
                ilGen.Call(typeof(IValueWriter).GetMethod(nameof(IValueWriter.DirectWrite)));
                ilGen.Return();
            }

            if (!TypeInfo<T>.IsFinal)
            {
                ilGen.LoadConstant(TypeInfo<T>.Int64TypeHandle);
                ilGen.LoadArgument(2);
                ilGen.Call(GetTypeHandle_Object);
                ilGen.ConvertInt64();
                ilGen.BranchIfEqual(label_write);
                ilGen.LoadArgument(2);
                ilGen.Call(GetInterface_Object);
                ilGen.LoadArgument(1);
                ilGen.LoadArgument(2);
                ilGen.Call(Write_IValueWriter_Object);
                ilGen.Return();
            }

            ilGen.MarkLabel(label_write);
            ilGen.LoadArgument(1);
            ilGen.NewObject(rtType.GetConstructor(Type.EmptyTypes));
            ilGen.Duplicate();
            ilGen.LoadArgument(2);
            ilGen.Call(rtType.GetMethod(nameof(FastObjectRW<T>.Initialize), new Type[] { typeof(T) }));
            ilGen.Call(typeof(IValueWriter).GetMethod(nameof(IValueWriter.WriteObject)));
            ilGen.Return();
        }

        public static void ImplReadValue(ILGenerator ilGen, Type rtType)
        {
            var label_read = ilGen.DefineLabel();

            // var rw = new FastObjectRW_T();
            var local_rw = ilGen.DeclareLocal(rtType);
            ilGen.NewObject(rtType.GetConstructor(Type.EmptyTypes));
            ilGen.Duplicate();
            ilGen.StoreLocal(local_rw);

            ilGen.LoadArgument(1);
            ilGen.LoadLocal(local_rw);

            // if (!valueReader is IId64Filler<char>) goto read;
            ilGen.LoadArgument(1);
            ilGen.IsInstance(typeof(IId64Filler<char>));
            ilGen.BranchFalse(label_read);

            // valueReader.FillValue<FastObjectRW_T>(rw); return rw.Content;
            ilGen.Call(typeof(IId64Filler<char>).GetMethod(nameof(IId64Filler<char>.FillValue)).MakeGenericMethod(rtType));
            ilGen.LoadField(ContentField);
            ilGen.Return();

            ilGen.MarkLabel(label_read);
            ilGen.Call(typeof(IValueReader).GetMethod(nameof(IValueReader.ReadObject)));
            ilGen.LoadField(ContentField);
            ilGen.Return();
        }
    }
}