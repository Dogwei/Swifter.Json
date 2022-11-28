using InlineIL;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static Swifter.RW.StaticFastObjectRW;

namespace Swifter.RW
{
    internal static unsafe class StaticFastObjectRW
    {
        private static readonly Dictionary<Type, MethodInfo> IValueReaderReadMethodInfos;
        private static readonly Dictionary<Type, MethodInfo> IValueWriterWriteMethodInfos;

        static StaticFastObjectRW()
        {
            IValueReaderReadMethodInfos = typeof(IValueReader)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.ReturnType != typeof(void) && x.GetParameters().Length == 0 && !x.IsGenericMethodDefinition && x.Name.StartsWith("Read"))
                .ToDictionary(x => x.ReturnType);

            IValueWriterWriteMethodInfos = typeof(IValueWriter)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.ReturnType == typeof(void) && x.GetParameters().Length == 1 && !x.IsGenericMethodDefinition && x.Name.StartsWith("Write"))
                .ToDictionary(x => x.GetParameters()[0].ParameterType);
        }

        public static MethodInfo ValueInterfaceReadValueMethod
        {
            [MethodImpl(MethodImplOptions.NoInlining)] // Compatible with MONO AOT
            get => TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(ValueInterface), nameof(ValueInterface.ReadValue), 1, typeof(IValueReader))));
        }

        public static MethodInfo ValueInterfaceWriteValueMethod
        {
            [MethodImpl(MethodImplOptions.NoInlining)] // Compatible with MONO AOT
            get => TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(ValueInterface), nameof(ValueInterface.WriteValue), 1, typeof(IValueWriter), TypeRef.MethodGenericParameters[0])));
        }

        public static MethodInfo RWStopTokenGetIsStopRequestedMethod
        {
            [MethodImpl(MethodImplOptions.NoInlining)] // Compatible with MONO AOT
            get => TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.PropertyGet(typeof(RWStopToken), nameof(RWStopToken.IsStopRequested))));
        }

        public static MethodInfo RWStopTokenGetCanBeStoppedMethod
        {
            [MethodImpl(MethodImplOptions.NoInlining)] // Compatible with MONO AOT
            get => TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.PropertyGet(typeof(RWStopToken), nameof(RWStopToken.CanBeStopped))));
        }

        public static MethodInfo RWStopTokenPopStateMethod
        {
            [MethodImpl(MethodImplOptions.NoInlining)] // Compatible with MONO AOT
            get => TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(RWStopToken), nameof(RWStopToken.PopState))));
        }

        public static MethodInfo RWStopTokenSetStateMethod
        {
            [MethodImpl(MethodImplOptions.NoInlining)] // Compatible with MONO AOT
            get => TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(RWStopToken), nameof(RWStopToken.SetState))));
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // Compatible with MONO AOT
        public static MethodInfo? GetReadValueMethod(Type type)
        {
            if (!ValueInterface.GetInterface(type).IsDefaultBehaviorInternal)
            {
                return null;
            }

            if (type.IsEnum)
            {
                return TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(IValueReader), nameof(IValueReader.ReadEnum)))).MakeGenericMethod(type);
            }

            if (Nullable.GetUnderlyingType(type) is Type nullableType)
            {
                return TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(IValueReader), nameof(IValueReader.ReadNullable)))).MakeGenericMethod(nullableType);
            }

            if (IValueReaderReadMethodInfos.TryGetValue(type, out var method))
            {
                return method;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // Compatible with MONO AOT
        public static MethodInfo? GetWriteValueMethod(Type type)
        {
            if (!ValueInterface.GetInterface(type).IsDefaultBehaviorInternal)
            {
                return null;
            }

            if (type.IsEnum)
            {
                return TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(IValueWriter), nameof(IValueWriter.WriteEnum)))).MakeGenericMethod(type);
            }

            if (IValueWriterWriteMethodInfos.TryGetValue(type, out var method))
            {
                return method;
            }

            return null;
        }

        public static void Switch(ref FastObjectRWOptions options, FastObjectRWOptions target, RWBoolean boolean)
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

        public static Ps<char>* ToUTF16Keys(string[] keys)
        {
            var total_length = 0;

            foreach (var item in keys)
            {
                total_length += item.Length;
            }

            var UTF16Keys = (Ps<char>*)Marshal.AllocHGlobal(keys.Length * sizeof(Ps<char>) + sizeof(char) * total_length);

            var hGChars = (char*)(UTF16Keys + keys.Length);

            for (int i = 0; i < keys.Length; i++)
            {
                var item = keys[i];

                for (int j = 0; j < item.Length; j++)
                {
                    hGChars[j] = item[j];
                }

                UTF16Keys[i] = new Ps<char>(hGChars, item.Length);

                hGChars += item.Length;
            }

            return UTF16Keys;
        }

        public static Ps<Utf8Byte>* ToUTF8Keys(string[] keys)
        {
            var total_length = 0;

            foreach (var item in keys)
            {
                total_length += Encoding.UTF8.GetByteCount(item);
            }

            var UTF8Keys = (Ps<Utf8Byte>*)Marshal.AllocHGlobal(keys.Length * sizeof(Ps<Utf8Byte>) + sizeof(Utf8Byte) * total_length);

            var hGChars = (Utf8Byte*)(UTF8Keys + keys.Length);
            var hGCharsRest = total_length;

            for (int i = 0; i < keys.Length; i++)
            {
                var item = keys[i];

                fixed(char* pItem = item)
                {
                    int length = Encoding.UTF8.GetBytes(pItem, item.Length, (byte*)hGChars, hGCharsRest);

                    UTF8Keys[i] = new Ps<Utf8Byte>(hGChars, length);

                    hGChars += length;
                    hGCharsRest -= length;
                }
            }

            return UTF8Keys;
        }

        public static void SaveField(OpenDictionary<string, BaseField> fields, BaseField field)
        {
            var index = fields.FindIndex(field.Name);

            if (index >= 0)
            {
                ref var store = ref fields[index].Value;

                store = ChooseField(field, store);
            }
            else
            {
                fields.Add(field.Name, field);
            }
        }

        public static BaseField ChooseField(BaseField field1, BaseField field2)
        {
            if (field1.Attribute != null && field2.Attribute is null)
            {
                return field1;
            }

            if (field2.Attribute != null && field1.Attribute is null)
            {
                return field2;
            }

            if (
                field1.MemberInfo is MemberInfo member1
                && field2.MemberInfo is MemberInfo member2
                && member1.DeclaringType is var declaringType1
                && member2.DeclaringType is var declaringType2
                && declaringType1 != declaringType2
                )
            {
                if (declaringType1 is null)
                {
                    return field2;
                }

                if (declaringType2 is null)
                {
                    return field1;
                }

                if (declaringType1.IsAssignableFrom(declaringType2))
                {
                    return field2;
                }

                return field1;
            }
            else
            {
                return field1;
            }

            throw new ArgumentException($"Member name conflict of '{member1}' with '{member2}'.");
        }
    }

    internal static unsafe partial class StaticFastObjectRW<T>
    {
        public static readonly BaseField[] Fields;

        public static readonly string[] Keys;
        public static readonly Ps<char>[] UTF16Keys;
        public static readonly Ps<Utf8Byte>[] UTF8Keys;

        public static readonly Ps<char>* _UTF16Keys;
        public static readonly Ps<Utf8Byte>* _UTF8Keys;

#if IMMUTABLE_COLLECTIONS
        public static readonly ImmutableArray<string> ExportKeys;
        public static readonly ImmutableArray<Ps<char>> ExportUTF16Keys;
        public static readonly ImmutableArray<Ps<Utf8Byte>> ExportUTF8Keys;
#else
        public static readonly ReadOnlyCollection<string> ExportKeys;
        public static readonly ReadOnlyCollection<Ps<char>> ExportUTF16Keys;
        public static readonly ReadOnlyCollection<Ps<Utf8Byte>> ExportUTF8Keys;
#endif

        public static readonly IFastObjectRWCreater<T> Creater;

        public static readonly FastObjectRWOptions Options;

        public static readonly bool IsVisibleTo;

        public static TypeBuilder TypeBuilder;

        public static MethodBase GetGetValueInterfaceInstanceMethod()
        {
            IL.Emit.Ldtoken(MethodRef.Method(typeof(FastObjectRW<T>), nameof(FastObjectRW<T>.GetValueInterfaceInstance)));
            IL.Emit.Ldtoken(typeof(FastObjectRW<T>));
            IL.Emit.Call(MethodRef.Method(typeof(MethodBase), nameof(MethodBase.GetMethodFromHandle), typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle)));
            return IL.Return<MethodBase>();
        }

        public static FieldInfo GetContentField()
        {
            IL.Emit.Ldtoken(FieldRef.Field(typeof(FastObjectRW<T>), nameof(FastObjectRW<T>.content)));
            IL.Emit.Ldtoken(typeof(FastObjectRW<T>));
            IL.Emit.Call(MethodRef.Method(typeof(FieldInfo), nameof(FieldInfo.GetFieldFromHandle), typeof(RuntimeFieldHandle), typeof(RuntimeTypeHandle)));
            return IL.Return<FieldInfo>();
        }

        public static void GetFields(Type type, OpenDictionary<string, BaseField> dicFields, RWObjectAttribute[] objectAttributes)
        {
            if (Options.On(FastObjectRWOptions.InheritedMembers))
            {
                var baseType = type.BaseType;

                if (baseType != null && baseType != typeof(object))
                {
                    GetFields(baseType, dicFields, objectAttributes);
                }
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);


            if (fields is not null && fields.Length != 0)
            {
                foreach (var item in fields)
                {
                    var tempAttributes = item.GetCustomAttributes(typeof(RWFieldAttribute), true);
                    var attributes = new List<RWFieldAttribute>(tempAttributes.Length);

                    foreach (RWFieldAttribute rWFieldAttribute in tempAttributes)
                    {
                        attributes.Add(rWFieldAttribute);
                    }

                    if (objectAttributes.Length != 0)
                    {
                        foreach (var objectAttribute in objectAttributes)
                        {
                            objectAttribute.OnLoadMember(typeof(T), item, attributes);
                        }
                    }

                    if (attributes.Count != 0)
                    {
                        foreach (var attribute in attributes)
                        {
                            var attributedField = new FastField(item, attribute);

                            if (attributedField.CanRead || attributedField.CanWrite)
                            {
                                SaveField(dicFields, attributedField);
                            }
                        }
                    }
                    else if (Options.On(FastObjectRWOptions.Field) && item.IsPublic)
                    {
                        var field = new FastField(item, null);

                        if (field.CanRead || field.CanWrite)
                        {
                            SaveField(dicFields, field);
                        }
                    }
                }
            }


            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (properties is not null && properties.Length != 0)
            {
                foreach (var item in properties)
                {
                    var indexParameters = item.GetIndexParameters();

                    if (indexParameters != null && indexParameters.Length != 0)
                    {
                        /* Ignore Indexer. */
                        continue;
                    }

                    var tempAttributes = item.GetCustomAttributes(typeof(RWFieldAttribute), true);
                    var attributes = new List<RWFieldAttribute>(tempAttributes.Length);

                    foreach (RWFieldAttribute rWFieldAttribute in tempAttributes)
                    {
                        attributes.Add(rWFieldAttribute);
                    }

                    if (objectAttributes != null && objectAttributes.Length != 0)
                    {
                        foreach (var objectAttribute in objectAttributes)
                        {
                            objectAttribute.OnLoadMember(typeof(T), item, attributes);
                        }
                    }

                    if (attributes != null && attributes.Count != 0)
                    {
                        foreach (var attribute in attributes)
                        {
                            var attributedField = new FastProperty(item, attribute);

                            if (attributedField.CanRead || attributedField.CanWrite)
                            {
                                SaveField(dicFields, attributedField);
                            }
                        }
                    }
                    else if (Options.On(FastObjectRWOptions.Property))
                    {
                        var field = new FastProperty(item, null);

                        if (field.CanRead || field.CanWrite)
                        {
                            SaveField(dicFields, field);
                        }
                    }
                }
            }
        }

        public static void LoadContent(ILGenerator ilGen)
        {
            ilGen.LoadArgument(0);

            if (typeof(T).IsValueType)
            {
                ilGen.LoadFieldAddress(GetContentField());
            }
            else
            {
                ilGen.LoadField(GetContentField());
            }
        }

        public static TKey GetKeyByIndex<TKey>(int index) where TKey : notnull
        {
            if (typeof(TKey) == typeof(Ps<char>))
            {
                return Unsafe.As<Ps<char>, TKey>(ref UTF16Keys[index]);
            }
            else if (typeof(TKey) == typeof(Ps<Utf8Byte>))
            {
                return Unsafe.As<Ps<Utf8Byte>, TKey>(ref UTF8Keys[index]);
            }
            else if (typeof(TKey) == typeof(string))
            {
                return Unsafe.As<string, TKey>(ref Keys[index]);
            }
            else if (typeof(TKey) == typeof(int))
            {
                return Unsafe.As<int, TKey>(ref index);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static int GetIndexByKey<TKey>(TKey key) where TKey : notnull
        {
            if (typeof(TKey) == typeof(Ps<char>))
            {
                return Array.IndexOf(UTF16Keys, Unsafe.As<TKey, Ps<char>>(ref key));
            }
            else if (typeof(TKey) == typeof(Ps<Utf8Byte>))
            {
                return Array.IndexOf(UTF8Keys, Unsafe.As<TKey, Ps<Utf8Byte>>(ref key));
            }
            else if (typeof(TKey) == typeof(string))
            {
                return Array.IndexOf(Keys, Unsafe.As<TKey, string>(ref key));
            }
            else if (typeof(TKey) == typeof(int))
            {
                return Unsafe.As<TKey, int>(ref key);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static void EmitToString<TKey>(ILGenerator ilGen) where TKey : notnull
        {
            if (typeof(TKey) == typeof(Ps<char>))
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.ToStringEx), typeof(Ps<char>)))));
            }
            else if (typeof(TKey) == typeof(Ps<Utf8Byte>))
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.ToStringEx), typeof(Ps<Utf8Byte>)))));
            }
            else if (typeof(TKey) == typeof(int))
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(Convert), nameof(Convert.ToString), typeof(int)))));
            }
            else if (typeof(TKey) == typeof(string))
            {
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static void EmitLoadKey<TKey>(ILGenerator ilGen, int index) where TKey : notnull
        {
            if (typeof(TKey) == typeof(Ps<char>))
            {
                ilGen.LoadConstant((IntPtr)(_UTF16Keys + index));
                ilGen.LoadValue(typeof(Ps<char>));
            }
            else if (typeof(TKey) == typeof(Ps<Utf8Byte>))
            {
                ilGen.LoadConstant((IntPtr)(_UTF8Keys + index));
                ilGen.LoadValue(typeof(Ps<Utf8Byte>));
            }
            else if (typeof(TKey) == typeof(string))
            {
                ilGen.LoadString(Keys[index]);
            }
            else if (typeof(TKey) == typeof(int))
            {
                ilGen.LoadConstant(index);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static void EmitSwitch<TKey>(ILGenerator ilGen, Action<ILGenerator> emitLdValue, CaseInfo<TKey>[] cases, Label defaultLabel) where TKey : notnull
        {
            if (typeof(TKey) == typeof(int))
            {
                ilGen.Switch(emitLdValue, Unsafe.As<CaseInfo<int>[]>(cases), defaultLabel);
            }
            else if (typeof(TKey) == typeof(string))
            {
                ilGen.Switch(emitLdValue, (il, item) => il.LoadString(item), Unsafe.As<CaseInfo<string>[]>(cases), defaultLabel, Options.On(FastObjectRWOptions.IgnoreCase));
            }
            else if (typeof(TKey) == typeof(Ps<char>))
            {
                ilGen.Switch(emitLdValue, (il, item) => EmitLoadKey<TKey>(il, GetIndexByKey(item)), Unsafe.As<CaseInfo<Ps<char>>[]>(cases), defaultLabel, Options.On(FastObjectRWOptions.IgnoreCase));
            }
            else if (typeof(TKey) == typeof(Ps<Utf8Byte>))
            {
                ilGen.Switch(emitLdValue, (il, item) => EmitLoadKey<TKey>(il, GetIndexByKey(item)), Unsafe.As<CaseInfo<Ps<Utf8Byte>>[]>(cases), defaultLabel, Options.On(FastObjectRWOptions.IgnoreCase));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        static StaticFastObjectRW()
        {
            var type = typeof(T);

            var attributes = Unsafe.As<RWObjectAttribute[]>(type.GetCustomAttributes(typeof(RWObjectAttribute), true)); // TODO: 是否满足顺序需求

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

            var fieldsMap = new OpenDictionary<string, BaseField>(
                Options.On(FastObjectRWOptions.IgnoreCase) 
                ? StringComparer.InvariantCultureIgnoreCase 
                : StringComparer.InvariantCulture);

            GetFields(type, fieldsMap, attributes);

            var fields = new List<BaseField>(fieldsMap.Count);

            for (int i = 0; i < fieldsMap.Count; i++)
            {
                fields.Add(fieldsMap[i].Value);
            }

            fields.Sort();

            if (attributes.Length != 0)
            {
                foreach (var item in attributes)
                {
                    item.OnCreate(type, Unsafe.As<List<IObjectField>>(fields));
                }
            }

            Fields = fields.ToArray();

            IsVisibleTo = DynamicAssembly.IsInternalsVisibleTo(typeof(T).Assembly);

            var isVisible = typeof(T).IsExternalVisible();

            var isCanAccess = isVisible || IsVisibleTo;

            if (!isCanAccess)
            {
                DynamicAssembly.IgnoresAccessChecksTo(typeof(T).Assembly);

                isCanAccess = DynamicAssembly.IsIgnoresAccessChecksTo(typeof(T).Assembly);
            }

            foreach (var field in Fields)
            {
                var fieldType = field.FieldType;
                var readType = field.ReadType;
                var writeType = field.WriteType;

                if (isCanAccess && ((field.CanRead && !field.IsPublicGet) || (field.CanWrite && !field.IsPublicSet)))
                {
                    isCanAccess = DynamicAssembly.CanAccessNonPublicMembers || IsVisibleTo;
                }

                if (isCanAccess && !(fieldType.IsExternalVisible() || DynamicAssembly.IsInternalsVisibleTo(fieldType.Assembly)))
                {
                    DynamicAssembly.IgnoresAccessChecksTo(fieldType.Assembly);

                    isCanAccess = DynamicAssembly.IsIgnoresAccessChecksTo(fieldType.Assembly);
                }

                if (isCanAccess && readType != fieldType && !(readType.IsExternalVisible() || DynamicAssembly.IsInternalsVisibleTo(readType.Assembly)))
                {
                    DynamicAssembly.IgnoresAccessChecksTo(readType.Assembly);

                    isCanAccess = DynamicAssembly.IsIgnoresAccessChecksTo(readType.Assembly);
                }

                if (isCanAccess && writeType != fieldType && !(writeType.IsExternalVisible() || DynamicAssembly.IsInternalsVisibleTo(writeType.Assembly)))
                {
                    DynamicAssembly.IgnoresAccessChecksTo(writeType.Assembly);

                    isCanAccess = DynamicAssembly.IsIgnoresAccessChecksTo(writeType.Assembly);
                }
            }

            Keys = Fields.Map(field => field.Name);

            _UTF16Keys = ToUTF16Keys(Keys);
            _UTF8Keys = ToUTF8Keys(Keys);

            UTF16Keys = new Ps<char>[Keys.Length];
            UTF8Keys = new Ps<Utf8Byte>[Keys.Length];

            for (int i = 0; i < Keys.Length; i++)
            {
                UTF16Keys[i] = _UTF16Keys[i];
                UTF8Keys[i] = _UTF8Keys[i];
            }

#if IMMUTABLE_COLLECTIONS
            ExportKeys = ImmutableArray.CreateRange(Keys);
            ExportUTF16Keys = ImmutableArray.CreateRange(UTF16Keys);
            ExportUTF8Keys = ImmutableArray.CreateRange(UTF8Keys);
#else
            ExportKeys = new ReadOnlyCollection<string>(Keys);
            ExportUTF16Keys = new ReadOnlyCollection<Ps<char>>(UTF16Keys);
            ExportUTF8Keys = new ReadOnlyCollection<Ps<Utf8Byte>>(UTF8Keys);
#endif

            try
            {
                if (isCanAccess)
                {
                    Creater = CreateCreater();
                }
                else
                {
                    RuntimeHelpers.RunClassConstructor(typeof(NonPublicFastObjectRW<T>).TypeHandle);

                    Creater = new NonPublicFastObjectCreater<T>();
                }

                if (Creater is IValueInterface<T> valueInterface && ValueInterface<T>.Content is FastObjectInterface<T>)
                {
                    ValueInterface<T>.Content = valueInterface;
                }
            }
            catch (Exception e)
            {
                Creater = new ErrorFastObjectRWCreater<T>(e);
            }
            finally
            {
                TypeBuilder = null!;
            }
        }

        public static IFastObjectRWCreater<T> CreateCreater()
        {
            TypeBuilder = DynamicAssembly.DefineType(
                $"{"FastObjectRW"}_{typeof(T).Name}_{Guid.NewGuid():N}",
                TypeAttributes.Sealed | TypeAttributes.Public,
                typeof(FastObjectRW<T>));

            ImplInitialize();

            ImplOnWriteValue<string>();

            ImplOnReadValue<string>();

            ImplOnReadAll<string>();

            ImplOnWriteAll<string>();

            ImplGetOrdinal<string>();


            ImplOnWriteValue<Ps<char>>();

            ImplOnReadValue<Ps<char>>();

            ImplOnReadAll<Ps<char>>();

            ImplOnWriteAll<Ps<char>>();

            ImplGetOrdinal<Ps<char>>();


            ImplOnWriteValue<Ps<Utf8Byte>>();

            ImplOnReadValue<Ps<Utf8Byte>>();

            ImplOnReadAll<Ps<Utf8Byte>>();

            ImplOnWriteAll<Ps<Utf8Byte>>();

            ImplGetOrdinal<Ps<Utf8Byte>>();


            ImplOnWriteValue<int>();

            ImplOnReadValue<int>();

            ImplOnReadAll<int>();

            ImplOnWriteAll<int>();


            ImplGetValueRW();

            ImplLoadValue();

            ImplStoreValue();



            var defaultConstructor = TypeBuilder.CreateTypeInfo()!.GetConstructor(Type.EmptyTypes)!;

            TypeBuilder = DynamicAssembly.DefineType(
                $"{"FastObjectRWCreater"}_{typeof(T).Name}_{Guid.NewGuid():N}",
                TypeAttributes.Sealed | TypeAttributes.Public);

            TypeBuilder.AddInterfaceImplementation(typeof(IFastObjectRWCreater<T>));
            TypeBuilder.AddInterfaceImplementation(typeof(IValueInterface<T>));


            ImplCreate(defaultConstructor);

            ImplReadValue(defaultConstructor);

            ImplWriteValue(defaultConstructor);

            return (IFastObjectRWCreater<T>)TypeBuilder.CreateTypeInfo()!.GetConstructor(Type.EmptyTypes)!.Invoke(new object[] { });
        }

        public static void ImplInitialize()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.Initialize),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                Type.EmptyTypes);

            ImplInitialize(methodBuilder.GetILGenerator());
        }

        public static void ImplOnWriteValue<TKey>() where TKey : notnull
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnWriteValue),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(TKey), typeof(IValueReader) });

            ImplOnWriteValue<TKey>(methodBuilder.GetILGenerator());
        }

        public static void ImplOnReadAll<TKey>() where TKey: notnull
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadAll),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IDataWriter<TKey>), typeof(RWStopToken) });

            ImplOnReadAll<TKey>(methodBuilder.GetILGenerator());
        }

        public static void ImplOnWriteAll<TKey>() where TKey : notnull
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnWriteAll),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IDataReader<TKey>), typeof(RWStopToken) });

            ImplOnWriteAll<TKey>(methodBuilder.GetILGenerator());
        }

        public static void ImplOnReadValue<TKey>() where TKey : notnull
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadValue),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(TKey), typeof(IValueWriter) });

            ImplOnReadValue<TKey>(methodBuilder.GetILGenerator());
        }

        public static void ImplGetOrdinal<TKey>() where TKey : notnull
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.GetOrdinal),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(int),
                new Type[] { typeof(TKey) });

            ImplGetOrdinal<TKey>(methodBuilder.GetILGenerator());
        }

        public static void ImplLoadValue()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.LoadValue),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(int), typeof(object).MakeByRefType() });

            ImplLoadValue(methodBuilder.GetILGenerator());
        }

        public static void ImplStoreValue()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.StoreValue),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(int), typeof(object).MakeByRefType() });

            ImplStoreValue(methodBuilder.GetILGenerator());
        }

        public static void ImplGetValueRW()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.GetValueRW),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(IValueRW),
                new Type[] { typeof(int) });

            ImplGetValueRW(methodBuilder.GetILGenerator());
        }

        public static void ImplCreate(ConstructorInfo defaultConstructor)
        {
            var methodBuilder = TypeBuilder.DefineMethod(
               nameof(IFastObjectRWCreater<T>.Create),
               MethodAttributes.Public | MethodAttributes.Virtual,
               CallingConventions.HasThis,
               typeof(FastObjectRW<T>),
               Type.EmptyTypes);

            var ilGen = methodBuilder.GetILGenerator();

            ilGen.NewObject(defaultConstructor);
            ilGen.Return();
        }

        public static void ImplWriteValue(ConstructorInfo defaultConstructor)
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(IValueInterface<T>.WriteValue),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IValueWriter), typeof(T) }
                );

            var ilGen = methodBuilder.GetILGenerator();

            var label_write = ilGen.DefineLabel();
            var label_final = ilGen.DefineLabel();

            if (!typeof(T).IsValueType)
            {
                ilGen.LoadArgument(2);
                ilGen.BranchTrue(label_final);
                ilGen.LoadArgument(1);
                ilGen.LoadNull();
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(IValueWriter), nameof(IValueWriter.DirectWrite)))));
                ilGen.Return();
            }

            ilGen.MarkLabel(label_final);
            if (!(typeof(T).IsSealed || typeof(T).IsValueType))
            {
                ilGen.LoadConstant((long)TypeHelper.GetTypeHandle(typeof(T)));
                ilGen.LoadArgument(2);
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(TypeHelper), nameof(TypeHelper.GetTypeHandle), typeof(object)))));
                ilGen.ConvertInt64();
                ilGen.BranchIfEqual(label_write);
                ilGen.LoadArgument(2);
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(ValueInterface), nameof(ValueInterface.GetInterface), typeof(object)))));
                ilGen.LoadArgument(1);
                ilGen.LoadArgument(2);
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(ValueInterface), nameof(ValueInterface.Write), typeof(IValueWriter), typeof(object)))));
                ilGen.Return();
            }

            ilGen.MarkLabel(label_write);
            ilGen.LoadArgument(1);
            ilGen.NewObject(defaultConstructor);
            ilGen.Duplicate();
            ilGen.LoadArgument(2);
            ilGen.StoreField(GetContentField());
            ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(IValueWriter), nameof(IValueWriter.WriteObject)))));
            ilGen.Return();
        }

        public static void ImplReadValue(ConstructorInfo defaultConstructor)
        {
            var methodBuilder = TypeBuilder.DefineMethod(
               nameof(IValueInterface<T>.ReadValue),
               MethodAttributes.Public | MethodAttributes.Virtual,
               CallingConventions.HasThis,
               typeof(T),
               new Type[] { typeof(IValueReader) });

            var ilGen = methodBuilder.GetILGenerator();

            // var rw = new FastObjectRW_T();
            var local_rw = ilGen.DeclareLocal(typeof(FastObjectRW<T>));
            ilGen.NewObject(defaultConstructor);
            ilGen.StoreLocal(local_rw);

            //// 
            ilGen.LoadArgument(1);
            ilGen.LoadLocal(local_rw);
            ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(IValueReader), nameof(IValueReader.ReadObject)))));

            ilGen.LoadLocal(local_rw);
            ilGen.LoadField(GetContentField());
            ilGen.Return();
        }

        public static void ImplInitialize(ILGenerator ilGen)
        {
            if (typeof(T).IsValueType)
            {
                var local = ilGen.DeclareLocal(typeof(T));

                ilGen.LoadArgument(0);
                ilGen.LoadLocal(local);
                ilGen.StoreField(GetContentField());

                ilGen.Return();
            }
            else if (Options.On(FastObjectRWOptions.Allocate))
            {
                ilGen.LoadArgument(0);

                ilGen.LoadType(typeof(T));

                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(TypeHelper), nameof(TypeHelper.Allocate), typeof(Type)))));

                ilGen.StoreField(GetContentField());
                ilGen.Return();
            }
            else if(typeof(T).GetConstructor(Type.EmptyTypes) is ConstructorInfo constructor && (constructor.IsExternalVisible() || DynamicAssembly.CanAccessNonPublicMembers || IsVisibleTo))
            {
                ilGen.LoadArgument(0);
                ilGen.NewObject(constructor);
                ilGen.StoreField(GetContentField());
                ilGen.Return();
            }
            else
            {
                ilGen.LoadArgument(0);

                ilGen.LoadType(typeof(T));

                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(Activator), nameof(Activator.CreateInstance), typeof(Type)))));

                ilGen.StoreField(GetContentField());
                ilGen.Return();
            }
        }

        public static void ImplOnReadAll<TKey>(ILGenerator ilGen) where TKey : notnull
        {
            var m_GetValueWriter = typeof(IDataWriter<TKey>).GetProperty(new Type[] { typeof(TKey) })!.GetGetMethod(true)!;

            var locals = new Dictionary<Type, LocalBuilder>();

            var canBeStoppedLabel = ilGen.DefineLabel();

            ilGen.LoadArgumentAddress(2);

            ilGen.AutoCall(RWStopTokenGetCanBeStoppedMethod);

            ilGen.BranchTrue(canBeStoppedLabel);

            InternalImplOnReadAll(false);

            ilGen.MarkLabel(canBeStoppedLabel);

            InternalImplOnReadAll(true);

            void InternalImplOnReadAll(bool canBeStopped)
            {
                var labels = new List<Label>();

                var stateSwitchLabel = ilGen.DefineLabel();

                if (canBeStopped)
                {
                    ilGen.LoadArgumentAddress(2);

                    ilGen.AutoCall(RWStopTokenPopStateMethod);

                    ilGen.Duplicate();

                    ilGen.IsInstance(typeof(int));

                    ilGen.BranchTrue(stateSwitchLabel);

                    ilGen.Pop();
                }

                for (int i = 0; i < Fields.Length; i++)
                {
                    var field = Fields[i];

                    if (Options.On(FastObjectRWOptions.MembersOptIn) && field.Attribute is null)
                    {
                        continue;
                    }

                    if (field.CanRead)
                    {
                        if (canBeStopped)
                        {
                            var label = ilGen.DefineLabel();

                            ilGen.LoadArgumentAddress(2);

                            ilGen.AutoCall(RWStopTokenGetIsStopRequestedMethod);

                            ilGen.BranchFalse(label);

                            ilGen.LoadArgumentAddress(2);

                            ilGen.LoadConstant(labels.Count);

                            ilGen.Box(typeof(int));

                            ilGen.AutoCall(RWStopTokenSetStateMethod);

                            ilGen.Return();

                            ilGen.MarkLabel(label);

                            labels.Add(label);
                        }

                        if (field.SkipDefaultValue)
                        {
                            var local = locals.GetOrAdd(field.FieldType, t => ilGen.DeclareLocal(t));

                            field.GetValueBefore(ilGen);
                            field.GetValueAfter(ilGen);

                            ilGen.StoreLocal(local);

                            var isDefaultValue = ilGen.DefineLabel();

                            ilGen.BranchDefaultValue(local, isDefaultValue);

                            field.WriteValueBefore(ilGen);

                            ilGen.LoadArgument(1);

                            EmitLoadKey<TKey>(ilGen, i);

                            ilGen.AutoCall(m_GetValueWriter);

                            ilGen.LoadLocal(local);

                            field.WriteValueAfter(ilGen);

                            ilGen.MarkLabel(isDefaultValue);
                        }
                        else
                        {
                            field.WriteValueBefore(ilGen);

                            ilGen.LoadArgument(1);

                            EmitLoadKey<TKey>(ilGen, i);

                            ilGen.AutoCall(m_GetValueWriter);

                            field.GetValueBefore(ilGen);

                            field.GetValueAfter(ilGen);

                            field.WriteValueAfter(ilGen);
                        }
                    }
                }

                ilGen.Return();

                if (canBeStopped)
                {
                    ilGen.MarkLabel(stateSwitchLabel);

                    ilGen.UnboxAny(typeof(int));

                    ilGen.Switch(labels.ToArray());
                }

                ilGen.Return();
            }
        }

        public static void ImplOnWriteAll<TKey>(ILGenerator ilGen) where TKey : notnull
        {
            var m_GetValueReader = typeof(IDataReader<TKey>).GetProperty(new Type[] { typeof(TKey) })!.GetGetMethod(true)!;

            var canBeStoppedLabel = ilGen.DefineLabel();

            ilGen.LoadArgumentAddress(2);

            ilGen.AutoCall(RWStopTokenGetCanBeStoppedMethod);

            ilGen.BranchTrue(canBeStoppedLabel);

            InternalImplOnWriteAll(false);

            ilGen.MarkLabel(canBeStoppedLabel);

            InternalImplOnWriteAll(true);

            void InternalImplOnWriteAll(bool canBeStopped)
            {
                var labels = new List<Label>();

                var stateSwitchLabel = ilGen.DefineLabel();

                if (canBeStopped)
                {
                    ilGen.LoadArgumentAddress(2);

                    ilGen.AutoCall(RWStopTokenPopStateMethod);

                    ilGen.Duplicate();

                    ilGen.IsInstance(typeof(int));

                    ilGen.BranchTrue(stateSwitchLabel);

                    ilGen.Pop();
                }

                for (int i = 0; i < Fields.Length; i++)
                {
                    var field = Fields[i];

                    if (field.CanWrite)
                    {
                        if (canBeStopped)
                        {
                            var label = ilGen.DefineLabel();

                            ilGen.LoadArgumentAddress(2);

                            ilGen.AutoCall(RWStopTokenGetIsStopRequestedMethod);

                            ilGen.BranchFalse(label);

                            ilGen.LoadArgumentAddress(2);

                            ilGen.LoadConstant(labels.Count);

                            ilGen.Box(typeof(int));

                            ilGen.AutoCall(RWStopTokenSetStateMethod);

                            ilGen.Return();

                            ilGen.MarkLabel(label);

                            labels.Add(label);
                        }

                        field.SetValueBefore(ilGen);

                        field.ReadValueBefore(ilGen);

                        ilGen.LoadArgument(1);

                        EmitLoadKey<TKey>(ilGen, i);

                        ilGen.AutoCall(m_GetValueReader);

                        field.ReadValueAfter(ilGen);

                        field.SetValueAfter(ilGen);
                    }
                }

                ilGen.Return();

                if (canBeStopped)
                {
                    ilGen.MarkLabel(stateSwitchLabel);

                    ilGen.UnboxAny(typeof(int));

                    ilGen.Switch(labels.ToArray());
                }

                ilGen.Return();
            }
        }

        public static void ImplOnReadValue<TKey>(ILGenerator ilGen) where TKey : notnull
        {
            EmitSwitchFields<TKey>(ilGen, 1, out var Cases, out var DefaultLabel);

            ilGen.MarkLabel(DefaultLabel);

            if (Options.On(FastObjectRWOptions.NotFoundException))
            {
                ilGen.LoadString(typeof(T).Name);

                ilGen.LoadArgument(1);

                EmitToString<TKey>(ilGen);

                ilGen.NewObject(TypeHelper.GetConstructorFromHandle(IL.Ldtoken(MethodRef.Constructor(typeof(MissingMemberException), typeof(string), typeof(string)))));

                ilGen.Throw();
            }
            else
            {
                ilGen.LoadArgument(2);

                ilGen.LoadNull();

                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(IValueWriter), nameof(IValueWriter.DirectWrite)))));

                ilGen.Return();
            }

            for (int i = 0; i < Fields.Length; i++)
            {
                var field = Fields[i];
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
                    if (field.CannotGetException)
                    {
                        ilGen.LoadString($"This member '{field.Name}' no get method or cannot access.");

                        ilGen.NewObject(TypeHelper.GetConstructorFromHandle(IL.Ldtoken(MethodRef.Constructor(typeof(MemberAccessException), typeof(string)))));

                        ilGen.Throw();
                    }
                    else
                    {
                        ilGen.LoadArgument(2);

                        ilGen.LoadNull();

                        ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(IValueWriter), nameof(IValueWriter.DirectWrite)))));

                        ilGen.Return();
                    }
                }
            }
        }

        public static void ImplOnWriteValue<TKey>(ILGenerator ilGen) where TKey : notnull
        {
            EmitSwitchFields<TKey>(ilGen, 1, out var Cases, out var DefaultLabel);

            ilGen.MarkLabel(DefaultLabel);

            if (Options.On(FastObjectRWOptions.NotFoundException))
            {
                ilGen.LoadString(typeof(T).Name);

                ilGen.LoadArgument(1);

                EmitToString<TKey>(ilGen);

                ilGen.NewObject(TypeHelper.GetConstructorFromHandle(IL.Ldtoken(MethodRef.Constructor(typeof(MissingMemberException), typeof(string), typeof(string)))));

                ilGen.Throw();
            }
            else
            {
                ilGen.LoadArgument(2);

                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(IValueReader), nameof(IValueReader.Pop)))));
                
                ilGen.Return();
            }

            for (int i = 0; i < Fields.Length; i++)
            {
                var field = Fields[i];
                var @case = Cases[i];

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
                    if (field.CannotSetException)
                    {
                        ilGen.LoadString($"This member '{field.Name}' no set method or cannot access.");

                        ilGen.NewObject(TypeHelper.GetConstructorFromHandle(IL.Ldtoken(MethodRef.Constructor(typeof(MemberAccessException), typeof(string)))));

                        ilGen.Throw();
                    }
                    else
                    {
                        ilGen.LoadArgument(2);

                        ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(IValueReader), nameof(IValueReader.Pop)))));

                        ilGen.Return();
                    }
                }
            }
        }

        public static void ImplGetOrdinal<TKey>(ILGenerator ilGen) where TKey : notnull
        {
            EmitSwitchFields<TKey>(ilGen, 1, out var Cases, out var DefaultLabel);

            ilGen.MarkLabel(DefaultLabel);

            ilGen.LoadConstant(-1);
            ilGen.Return();

            for (int i = 0; i < Fields.Length; i++)
            {
                var @case = Cases[i];

                ilGen.MarkLabel(@case.Label);

                ilGen.LoadConstant(i);
                ilGen.Return();
            }
        }

        public static void EmitSwitchFields<TKey>(ILGenerator ilGen, int argIndex, out CaseInfo<TKey>[] Cases, out Label DefaultLabel) where TKey : notnull
        {
            Cases = new CaseInfo<TKey>[Fields.Length];

            for (int i = 0; i < Fields.Length; i++)
            {
                Cases[i] = new CaseInfo<TKey>(GetKeyByIndex<TKey>(i), ilGen.DefineLabel());
            }

            DefaultLabel = ilGen.DefineLabel();

            EmitSwitch(ilGen, iLGen => ilGen.LoadArgument(1), Cases, DefaultLabel);
        }

        public static void ImplLoadValue(ILGenerator ilGen)
        {
            EmitSwitchFields<int>(ilGen, 1, out var Cases, out var DefaultLabel);

            ilGen.MarkLabel(DefaultLabel);
            ilGen.Return();

            for (int i = 0; i < Fields.Length; i++)
            {
                var field = Fields[i];
                var @case = Cases[i];

                ilGen.MarkLabel(@case.Label);

                if (field.CanRead)
                {
                    ilGen.LoadArgument(2);

                    field.GetValueBefore(ilGen);
                    field.GetValueAfter(ilGen);

                    ilGen.StoreValue(field.FieldType);

                    ilGen.Return();
                }
                else
                {
                    if (field.CannotGetException)
                    {
                        ilGen.LoadString($"This member '{field.Name}' no get method or cannot access.");

                        ilGen.NewObject(TypeHelper.GetConstructorFromHandle(IL.Ldtoken(MethodRef.Constructor(typeof(MemberAccessException), typeof(string)))));

                        ilGen.Throw();
                    }
                    else
                    {
                        ilGen.Return();
                    }
                }
            }
        }

        public static void ImplStoreValue(ILGenerator ilGen)
        {
            EmitSwitchFields<int>(ilGen, 1, out var Cases, out var DefaultLabel);

            ilGen.MarkLabel(DefaultLabel);
            ilGen.Return();

            for (int i = 0; i < Fields.Length; i++)
            {
                var field = Fields[i];
                var @case = Cases[i];

                ilGen.MarkLabel(@case.Label);

                if (field.CanWrite)
                {
                    field.SetValueBefore(ilGen);

                    ilGen.LoadArgument(2);
                    ilGen.LoadValue(field.FieldType);

                    field.SetValueAfter(ilGen);

                    ilGen.Return();
                }
                else
                {
                    if (field.CannotSetException)
                    {
                        ilGen.LoadString($"This member '{field.Name}' no set method or cannot access.");

                        ilGen.NewObject(TypeHelper.GetConstructorFromHandle(IL.Ldtoken(MethodRef.Constructor(typeof(MemberAccessException), typeof(string)))));

                        ilGen.Throw();
                    }
                    else
                    {
                        ilGen.Return();
                    }
                }
            }
        }

        public static void ImplGetValueRW(ILGenerator ilGen)
        {
            var ValueRWConstructorParameterTypes = new Type[] { typeof(FastObjectRW<T>), typeof(int) };

            EmitSwitchFields<int>(ilGen, 1, out var Cases, out var DefaultLabel);

            ilGen.MarkLabel(DefaultLabel);
            ilGen.Throw();
            ilGen.ThrowNewException(typeof(IndexOutOfRangeException));

            for (int i = 0; i < Fields.Length; i++)
            {
                var field = Fields[i];
                var @case = Cases[i];

                ilGen.MarkLabel(@case.Label);

                ilGen.LoadArgument(0);
                ilGen.LoadArgument(1);

                ilGen.NewObject(typeof(FastObjectRW<>.ValueRW<>).MakeGenericType(typeof(T), field.FieldType).GetConstructor(ValueRWConstructorParameterTypes)!);

                ilGen.Return();
            }
        }
    }
}