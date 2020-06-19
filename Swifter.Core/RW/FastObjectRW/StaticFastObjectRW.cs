using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static Swifter.Tools.MethodHelper;

namespace Swifter.RW
{
    internal sealed unsafe partial class StaticFastObjectRW<T>
    {
        public const BindingFlags StaticDeclaredOnly = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static readonly BaseField[] Fields;

        public static readonly string[] Keys;
        public static readonly Ps<char>* UTF16Keys;
        public static readonly Ps<Utf8Byte>* UTF8Keys;


        public static readonly IFastObjectRWCreater<T> Creater;

        public static readonly FastObjectRWOptions Options;

        public static readonly bool IsVisibleTo;

        public static TypeBuilder TypeBuilder;

        public static FieldInfo StringKeysField 
            => typeof(StaticFastObjectRW<T>).GetField(nameof(Keys));

        public static FieldInfo UTF16KeysField 
            => typeof(StaticFastObjectRW<T>).GetField(nameof(UTF16Keys));

        public static FieldInfo UTF8KeysField 
            => typeof(StaticFastObjectRW<T>).GetField(nameof(UTF8Keys));

        public static MethodInfo GetValueInterfaceInstanceMethod 
            => typeof(FastObjectRW<T>).GetMethod(nameof(FastObjectRW<T>.GetValueInterfaceInstance), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        public static FieldInfo ContentField 
            => typeof(FastObjectRW<T>).GetField(nameof(FastObjectRW<T>.content), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public static MethodInfo GetTypeHandle_Object 
            => typeof(TypeHelper).GetMethod(nameof(TypeHelper.GetTypeHandle), new Type[] { typeof(object) });

        public static MethodInfo GetInterface_Object 
            => typeof(ValueInterface).GetMethod(nameof(ValueInterface.GetInterface), new Type[] { typeof(object) });

        public static MethodInfo Write_IValueWriter_Object 
            => typeof(ValueInterface).GetMethod(nameof(ValueInterface.Write), new Type[] { typeof(IValueWriter), typeof(object) });

        public static ConstructorInfo MemberAccessException_String_Constructor 
            => typeof(MemberAccessException).GetConstructor(new Type[] { typeof(string) });

        public static ConstructorInfo MissingMemberException_String_String_Constructor 
            => typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string), typeof(string) });

        public static BaseField ChooseField(BaseField field1, BaseField field2)
        {
            if (field1.Attribute != null)
            {
                return field1;
            }

            if (field2.Attribute != null)
            {
                return field2;
            }

            if (field1.Original is MemberInfo member1 && field2.Original is MemberInfo member2)
            {
                if (member1.DeclaringType == member2.DeclaringType)
                {
                    throw new ArgumentException($"Member name conflict of '{member1}' with '{member2}' from {typeof(T)}.");
                }
                else if(member1.DeclaringType.IsSubclassOf(member2.DeclaringType))
                {
                    return field1;
                }
                else
                {
                    return field2;
                }
            }
            else
            {
                return field1;
            }
        }

        private static void GetFields(Type type, Dictionary<string, BaseField> dicFields, RWObjectAttribute[] objectAttributes)
        {
            if ((Options & FastObjectRWOptions.InheritedMembers) != 0)
            {
                var baseType = type.BaseType;

                if (baseType != null && baseType != typeof(object))
                {
                    GetFields(baseType, dicFields, objectAttributes);
                }
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);


            if (fields != null && fields.Length != 0)
            {
                foreach (var item in fields)
                {
                    var attributes = item
                        .GetCustomAttributes(typeof(RWFieldAttribute), true)
                        ?.OfType<RWFieldAttribute>()
                        ?.ToList();

                    if (objectAttributes !=null && objectAttributes.Length != 0)
                    {
                        foreach (var objectAttribute in objectAttributes)
                        {
                            objectAttribute.OnLoadMember(typeof(T), item, ref attributes);
                        }
                    }

                    if (attributes != null && attributes.Count != 0)
                    {
                        foreach (var attribute in attributes)
                        {
                            var attributedField = new FastField(item, attribute);

                            if (attributedField.CanRead || attributedField.CanWrite)
                            {
                                SaveField(attributedField);
                            }
                        }
                    }
                    else if ((Options & FastObjectRWOptions.Field) != 0 && item.IsPublic)
                    {
                        var field = new FastField(item, null);

                        if (field.CanRead || field.CanWrite)
                        {
                            SaveField(field);
                        }
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

                    var attributes = item
                        .GetCustomAttributes(typeof(RWFieldAttribute), true)
                        ?.OfType<RWFieldAttribute>()
                        ?.ToList();

                    if (objectAttributes != null && objectAttributes.Length != 0)
                    {
                        foreach (var objectAttribute in objectAttributes)
                        {
                            objectAttribute.OnLoadMember(typeof(T), item, ref attributes);
                        }
                    }

                    if (attributes != null && attributes.Count != 0)
                    {
                        foreach (var attribute in attributes)
                        {
                            var attributedField = new FastProperty(item, attribute);

                            if (attributedField.CanRead || attributedField.CanWrite)
                            {
                                SaveField(attributedField);
                            }
                        }
                    }
                    else if ((Options & FastObjectRWOptions.Property) != 0)
                    {
                        var field = new FastProperty(item, null);

                        if (field.CanRead || field.CanWrite)
                        {
                            SaveField(field);
                        }
                    }
                }
            }


            void SaveField(BaseField field)
            {
                if (dicFields.TryGetValue(field.Name, out var exist))
                {
                    dicFields[field.Name] = ChooseField(field, exist);
                }
                else
                {
                    dicFields[field.Name] = field;
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

        public static Ps<char>* GetUTF16Keys()
        {
            var total_length = 0;

            foreach (var item in Keys)
            {
                total_length += item.Length;
            }

            var UTF16Keys = (Ps<char>*)Marshal.AllocHGlobal(Keys.Length * sizeof(Ps<char>) + sizeof(char) * total_length);

            var hGChars = (char*)(UTF16Keys + Keys.Length);

            for (int i = 0; i < Keys.Length; i++)
            {
                var item = Keys[i];

                for (int j = 0; j < item.Length; j++)
                {
                    hGChars[j] = item[j];
                }

                UTF16Keys[i] = new Ps<char>(hGChars, item.Length);

                hGChars += item.Length;
            }

            return UTF16Keys;
        }

        public static Ps<Utf8Byte>* GetUTF8Keys()
        {
            var total_length = 0;

            foreach (var item in Keys)
            {
                total_length += StringHelper.GetUtf8BytesLength(ref StringHelper.GetRawStringData(item), item.Length);
            }

            var UTF8Keys = (Ps<Utf8Byte>*)Marshal.AllocHGlobal(Keys.Length * sizeof(Ps<Utf8Byte>) + sizeof(Utf8Byte) * total_length);

            var hGChars = (Utf8Byte*)(UTF8Keys + Keys.Length);

            for (int i = 0; i < Keys.Length; i++)
            {
                var item = Keys[i];

                int length = StringHelper.GetUtf8Bytes(ref StringHelper.GetRawStringData(item), item.Length, (byte*)hGChars);

                UTF8Keys[i] = new Ps<Utf8Byte>(hGChars, length);

                hGChars += length;
            }

            return UTF8Keys;
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

        public static TKey GetKeyByIndex<TKey>(int index)
        {
            if (typeof(TKey) == typeof(Ps<char>))
            {
                return Underlying.As<Ps<char>, TKey>(ref UTF16Keys[index]);
            }
            else if (typeof(TKey) == typeof(Ps<Utf8Byte>))
            {
                return Underlying.As<Ps<Utf8Byte>, TKey>(ref UTF8Keys[index]);
            }
            else if (typeof(TKey) == typeof(string))
            {
                return Underlying.As<string, TKey>(ref Keys[index]);
            }
            else if (typeof(TKey) == typeof(int))
            {
                return Underlying.As<int, TKey>(ref index);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static int GetIndexByKey<TKey>(TKey key)
        {
            if (typeof(TKey) == typeof(Ps<char>))
            {
                return ArrayHelper.IndexOf(UTF16Keys, Keys.Length, Underlying.As<TKey, Ps<char>>(ref key));
            }
            else if (typeof(TKey) == typeof(Ps<Utf8Byte>))
            {
                return ArrayHelper.IndexOf(UTF8Keys, Keys.Length, Underlying.As<TKey, Ps<Utf8Byte>>(ref key));
            }
            else if (typeof(TKey) == typeof(string))
            {
                return Array.IndexOf(Keys, Underlying.As<TKey, string>(ref key));
            }
            else if (typeof(TKey) == typeof(int))
            {
                return Underlying.As<TKey, int>(ref key);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static void EmitToString<TKey>(ILGenerator ilGen)
        {
            if (typeof(TKey) == typeof(Ps<char>))
            {
                ilGen.Call(MethodOf<Ps<char>, string>(StringHelper.ToStringEx));
            }
            else if (typeof(TKey) == typeof(Ps<Utf8Byte>))
            {
                ilGen.Call(MethodOf<Ps<Utf8Byte>, string>(StringHelper.ToStringEx));
            }
            else if (typeof(TKey) == typeof(int))
            {
                ilGen.Call(MethodOf<int, string>(Convert.ToString));
            }
            else if (typeof(TKey) == typeof(string))
            {
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static void EmitLoadKey<TKey>(ILGenerator ilGen, int index)
        {
            if (typeof(TKey) == typeof(Ps<char>))
            {
                ilGen.LoadConstant((IntPtr)Underlying.AsPointer(ref UTF16Keys[index]));
                ilGen.LoadValue(typeof(Ps<char>));
            }
            else if (typeof(TKey) == typeof(Ps<Utf8Byte>))
            {
                ilGen.LoadConstant((IntPtr)Underlying.AsPointer(ref UTF8Keys[index]));
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

        public static void EmitSwitch<TKey>(ILGenerator ilGen, Action<ILGenerator> emitLdValue, CaseInfo<TKey>[] cases, Label defaultLabel)
        {
            if (typeof(TKey) == typeof(int))
            {
                ilGen.Switch(emitLdValue, Underlying.As<CaseInfo<int>[]>(cases), defaultLabel);
            }
            else if (typeof(TKey) == typeof(string))
            {
                ilGen.Switch(emitLdValue, (il, item) => il.LoadString(item), Underlying.As<CaseInfo<string>[]>(cases), defaultLabel, (Options & FastObjectRWOptions.IgnoreCase) != 0);
            }
            else if (typeof(TKey) == typeof(Ps<char>))
            {
                ilGen.Switch(emitLdValue, (il, item) => EmitLoadKey<TKey>(il, GetIndexByKey(item)), Underlying.As<CaseInfo<Ps<char>>[]>(cases), defaultLabel, (Options & FastObjectRWOptions.IgnoreCase) != 0);
            }
            else if (typeof(TKey) == typeof(Ps<Utf8Byte>))
            {
                ilGen.Switch(emitLdValue, (il, item) => EmitLoadKey<TKey>(il, GetIndexByKey(item)), Underlying.As<CaseInfo<Ps<Utf8Byte>>[]>(cases), defaultLabel, (Options & FastObjectRWOptions.IgnoreCase) != 0);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        static StaticFastObjectRW()
        {
            lock (typeof(FastObjectRW<T>))
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

                    var fieldsMap = new Dictionary<string, BaseField>((Options & FastObjectRWOptions.IgnoreCase) != 0 ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);

                    GetFields(type, fieldsMap, attributes);

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

                    IsVisibleTo = DynamicAssembly.IsInternalsVisibleTo(typeof(T).Assembly);

                    var isVisible = typeof(T).IsExternalVisible();

#if DEBUG
                    Console.WriteLine($"{nameof(FastObjectRW)} : \"{typeof(T)}\" IsVisible : {isVisible}");
                    Console.WriteLine($"{nameof(FastObjectRW)} : \"{typeof(T)}\" IsVisibleTo : {IsVisibleTo}");
#endif

                    var isCanAccess = isVisible || IsVisibleTo;

                    if (!isCanAccess)
                    {
                        DynamicAssembly.IgnoresAccessChecksTo(typeof(T).Assembly);

                        isCanAccess = DynamicAssembly.IsIgnoresAccessChecksTo(typeof(T).Assembly);
                    }

                    foreach (var field in Fields)
                    {
                        var before = field.BeforeType;
                        var after = field.AfterType;

                        var beforeIsVisible = before.IsExternalVisible() || DynamicAssembly.IsInternalsVisibleTo(before.Assembly);
                        var afterIsVisible = after.IsExternalVisible() || DynamicAssembly.IsInternalsVisibleTo(after.Assembly);
#if DEBUG
                        Console.WriteLine($"{nameof(FastObjectRW)} : \"{typeof(T)}.{field.Name}\" \t " +
                            $"CanRead : {field.CanRead}, " +
                            $"CanWrite : {field.CanWrite}, " +
                            $"IsPublicGet : {field.IsPublicGet}, " +
                            $"IsPublicSet : {field.IsPublicSet}, " +
                            $"BeforeIsVisible : {beforeIsVisible}, " +
                            $"AfterIsVisible : {afterIsVisible}");
#endif
                        if (isCanAccess && ((field.CanRead && !field.IsPublicGet) || (field.CanWrite && !field.IsPublicSet)))
                        {
                            isCanAccess = DynamicAssembly.CanAccessNonPublicMembers || IsVisibleTo;
                        }

                        if (isCanAccess && !beforeIsVisible)
                        {
                            DynamicAssembly.IgnoresAccessChecksTo(before.Assembly);

                            isCanAccess = DynamicAssembly.IsIgnoresAccessChecksTo(before.Assembly);
                        }

                        if (isCanAccess && after != before && !afterIsVisible)
                        {
                            DynamicAssembly.IgnoresAccessChecksTo(after.Assembly);

                            isCanAccess = DynamicAssembly.IsIgnoresAccessChecksTo(after.Assembly);
                        }
                    }

#if DEBUG
                    Console.WriteLine($"{nameof(FastObjectRW)} : \"{typeof(T)}\" IsCanAssess : {isCanAccess}");
#endif

                    Keys = Fields.Select(item => item.Name).ToArray();

                    UTF16Keys = GetUTF16Keys();

                    UTF8Keys = GetUTF8Keys();

                    if (isCanAccess)
                    {
                        Creater = CreateCreater();
                    }
                    else
                    {
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
                    TypeBuilder = null;
                }
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

            if (type.IsValueType && Nullable.GetUnderlyingType(type) != null && ValueInterface.GetInterface(type).InterfaceIsNotModified)
            {
                return nameof(IValueReader.ReadNullable);
            }

            return (Type.GetTypeCode(type)) switch
            {
                TypeCode.Boolean => ValueInterface<bool>.IsNotModified ? nameof(IValueReader.ReadBoolean) : null,
                TypeCode.Char => ValueInterface<char>.IsNotModified ? nameof(IValueReader.ReadChar) : null,
                TypeCode.SByte => ValueInterface<sbyte>.IsNotModified ? nameof(IValueReader.ReadSByte) : null,
                TypeCode.Byte => ValueInterface<byte>.IsNotModified ? nameof(IValueReader.ReadByte) : null,
                TypeCode.Int16 => ValueInterface<short>.IsNotModified ? nameof(IValueReader.ReadInt16) : null,
                TypeCode.UInt16 => ValueInterface<ushort>.IsNotModified ? nameof(IValueReader.ReadUInt16) : null,
                TypeCode.Int32 => ValueInterface<int>.IsNotModified ? nameof(IValueReader.ReadInt32) : null,
                TypeCode.UInt32 => ValueInterface<uint>.IsNotModified ? nameof(IValueReader.ReadUInt32) : null,
                TypeCode.Int64 => ValueInterface<long>.IsNotModified ? nameof(IValueReader.ReadInt64) : null,
                TypeCode.UInt64 => ValueInterface<ulong>.IsNotModified ? nameof(IValueReader.ReadUInt64) : null,
                TypeCode.Single => ValueInterface<float>.IsNotModified ? nameof(IValueReader.ReadSingle) : null,
                TypeCode.Double => ValueInterface<double>.IsNotModified ? nameof(IValueReader.ReadDouble) : null,
                TypeCode.Decimal => ValueInterface<decimal>.IsNotModified ? nameof(IValueReader.ReadDecimal) : null,
                TypeCode.DateTime => ValueInterface<DateTime>.IsNotModified ? nameof(IValueReader.ReadDateTime) : null,
                TypeCode.String => ValueInterface<string>.IsNotModified ? nameof(IValueReader.ReadString) : null,
                _ => null,
            };
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

            return (Type.GetTypeCode(type)) switch
            {
                TypeCode.Boolean => ValueInterface<bool>.IsNotModified ? nameof(IValueWriter.WriteBoolean) : null,
                TypeCode.Char => ValueInterface<char>.IsNotModified ? nameof(IValueWriter.WriteChar) : null,
                TypeCode.SByte => ValueInterface<sbyte>.IsNotModified ? nameof(IValueWriter.WriteSByte) : null,
                TypeCode.Byte => ValueInterface<byte>.IsNotModified ? nameof(IValueWriter.WriteByte) : null,
                TypeCode.Int16 => ValueInterface<short>.IsNotModified ? nameof(IValueWriter.WriteInt16) : null,
                TypeCode.UInt16 => ValueInterface<ushort>.IsNotModified ? nameof(IValueWriter.WriteUInt16) : null,
                TypeCode.Int32 => ValueInterface<int>.IsNotModified ? nameof(IValueWriter.WriteInt32) : null,
                TypeCode.UInt32 => ValueInterface<uint>.IsNotModified ? nameof(IValueWriter.WriteUInt32) : null,
                TypeCode.Int64 => ValueInterface<long>.IsNotModified ? nameof(IValueWriter.WriteInt64) : null,
                TypeCode.UInt64 => ValueInterface<ulong>.IsNotModified ? nameof(IValueWriter.WriteUInt64) : null,
                TypeCode.Single => ValueInterface<float>.IsNotModified ? nameof(IValueWriter.WriteSingle) : null,
                TypeCode.Double => ValueInterface<double>.IsNotModified ? nameof(IValueWriter.WriteDouble) : null,
                TypeCode.Decimal => ValueInterface<decimal>.IsNotModified ? nameof(IValueWriter.WriteDecimal) : null,
                TypeCode.DateTime => ValueInterface<DateTime>.IsNotModified ? nameof(IValueWriter.WriteDateTime) : null,
                TypeCode.String => ValueInterface<string>.IsNotModified ? nameof(IValueWriter.WriteString) : null,
                _ => null,
            };
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



            var defaultConstructor = TypeBuilder.CreateTypeInfo().GetConstructor(Type.EmptyTypes);

            TypeBuilder = DynamicAssembly.DefineType(
                $"{"FastObjectRWCreater"}_{typeof(T).Name}_{Guid.NewGuid():N}",
                TypeAttributes.Sealed | TypeAttributes.Public);

            TypeBuilder.AddInterfaceImplementation(typeof(IFastObjectRWCreater<T>));
            TypeBuilder.AddInterfaceImplementation(typeof(IValueInterface<T>));


            ImplCreate(defaultConstructor);

            ImplReadValue(defaultConstructor);

            ImplWriteValue(defaultConstructor);

            return (IFastObjectRWCreater<T>)TypeBuilder.CreateTypeInfo().GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
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

        public static void ImplOnWriteValue<TKey>()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnWriteValue),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(TKey), typeof(IValueReader) });

            ImplOnWriteValue<TKey>(methodBuilder.GetILGenerator());
        }

        public static void ImplOnReadAll<TKey>()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadAll),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IDataWriter<TKey>) });

            ImplOnReadAll<TKey>(methodBuilder.GetILGenerator());
        }

        public static void ImplOnWriteAll<TKey>()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnWriteAll),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IDataReader<TKey>) });

            ImplOnWriteAll<TKey>(methodBuilder.GetILGenerator());
        }

        public static void ImplOnReadValue<TKey>()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadValue),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(TKey), typeof(IValueWriter) });

            ImplOnReadValue<TKey>(methodBuilder.GetILGenerator());
        }

        public static void ImplGetOrdinal<TKey>()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.GetOrdinal),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(int),
                new Type[] { typeof(TKey) });

            ImplGetOrdinal<TKey>(methodBuilder.GetILGenerator());
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
            new Type[] { typeof(IValueWriter), typeof(T) });

            var ilGen = methodBuilder.GetILGenerator();

            var label_write = ilGen.DefineLabel();
            var label_final = ilGen.DefineLabel();

            if (!typeof(T).IsValueType)
            {
                ilGen.LoadArgument(2);
                ilGen.BranchTrue(label_final);
                ilGen.LoadArgument(1);
                ilGen.LoadNull();
                ilGen.Call(typeof(IValueWriter).GetMethod(nameof(IValueWriter.DirectWrite)));
                ilGen.Return();
            }

            ilGen.MarkLabel(label_final);
            if (!(typeof(T).IsSealed || typeof(T).IsValueType))
            {
                ilGen.LoadConstant((long)TypeHelper.GetTypeHandle(typeof(T)));
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
            ilGen.NewObject(defaultConstructor);
            ilGen.Duplicate();
            ilGen.LoadArgument(2);
            ilGen.StoreField(ContentField);
            ilGen.Call(typeof(IValueWriter).GetMethod(nameof(IValueWriter.WriteObject)));
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
            ilGen.Call(typeof(IValueReader).GetMethod(nameof(IValueReader.ReadObject)));

            ilGen.LoadLocal(local_rw);
            ilGen.LoadField(ContentField);
            ilGen.Return();
        }



        public static void ImplInitialize(ILGenerator ilGen)
        {
            if (typeof(T).IsValueType)
            {
                var local = ilGen.DeclareLocal(typeof(T));

                ilGen.LoadArgument(0);
                ilGen.LoadLocal(local);
                ilGen.StoreField(ContentField);

                ilGen.Return();
            }
            else if ((Options & FastObjectRWOptions.Allocate) != 0)
            {
                ilGen.LoadArgument(0);

                ilGen.LoadType(typeof(T));

                ilGen.Call(MethodOf<Type, object>(TypeHelper.Allocate));

                ilGen.StoreField(ContentField);
                ilGen.Return();
            }
            else if(typeof(T).GetConstructor(Type.EmptyTypes) is ConstructorInfo constructor && (constructor.IsExternalVisible() || DynamicAssembly.CanAccessNonPublicMembers || IsVisibleTo))
            {
                ilGen.LoadArgument(0);
                ilGen.NewObject(constructor);
                ilGen.StoreField(ContentField);
                ilGen.Return();
            }
            else
            {
                ilGen.LoadArgument(0);
                ilGen.LoadType(typeof(T));
                ilGen.Call(MethodOf<Type, object>(Activator.CreateInstance));
                ilGen.StoreField(ContentField);
                ilGen.Return();
            }
        }

        public static void ImplOnReadAll<TKey>(ILGenerator ilGen)
        {
            var m_GetValueWriter = typeof(IDataWriter<TKey>).GetProperty(new Type[] { typeof(TKey) }).GetGetMethod(true);

            var locals = new Dictionary<Type, LocalBuilder>();

            for (int i = 0; i < Fields.Length; i++)
            {
                var field = Fields[i];

                if ((Options & FastObjectRWOptions.MembersOptIn) != 0 && field.Attribute is null)
                {
                    continue;
                }

                if (field.CanRead)
                {
                    if (field.SkipDefaultValue)
                    {
                        var local = locals.GetOrAdd(field.BeforeType, t => ilGen.DeclareLocal(t));

                        field.GetValueBefore(ilGen);
                        field.GetValueAfter(ilGen);

                        ilGen.StoreLocal(local);

                        var isDefaultValue = ilGen.DefineLabel();

                        ilGen.BranchDefaultValue(local, isDefaultValue);

                        field.WriteValueBefore(ilGen);

                        ilGen.LoadArgument(1);

                        EmitLoadKey<TKey>(ilGen, i);

                        ilGen.Call(m_GetValueWriter);

                        ilGen.LoadLocal(local);

                        field.WriteValueAfter(ilGen);

                        ilGen.MarkLabel(isDefaultValue);
                    }
                    else
                    {
                        field.WriteValueBefore(ilGen);

                        ilGen.LoadArgument(1);

                        EmitLoadKey<TKey>(ilGen, i);

                        ilGen.Call(m_GetValueWriter);

                        field.GetValueBefore(ilGen);

                        field.GetValueAfter(ilGen);

                        field.WriteValueAfter(ilGen);
                    }
                }
            }

            ilGen.Return();
        }

        public static void ImplOnWriteAll<TKey>(ILGenerator ilGen)
        {
            var m_GetValueReader = typeof(IDataReader<TKey>).GetProperty(new Type[] { typeof(TKey) }).GetGetMethod(true);

            for (int i = 0; i < Fields.Length; i++)
            {
                var field = Fields[i];

                if (field.CanWrite)
                {
                    field.SetValueBefore(ilGen);

                    field.ReadValueBefore(ilGen);

                    ilGen.LoadArgument(1);

                    EmitLoadKey<TKey>(ilGen, i);

                    ilGen.Call(m_GetValueReader);

                    field.ReadValueAfter(ilGen);

                    field.SetValueAfter(ilGen);
                }
            }

            ilGen.Return();
        }

        public static void ImplOnReadValue<TKey>(ILGenerator ilGen)
        {
            var fields = Fields;

            var Cases = new CaseInfo<TKey>[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                Cases[i] = new CaseInfo<TKey>(GetKeyByIndex<TKey>(i), ilGen.DefineLabel());
            }

            var DefaultLabel = ilGen.DefineLabel();

            EmitSwitch(ilGen, iLGen => ilGen.LoadArgument(1), Cases, DefaultLabel);

            ilGen.MarkLabel(DefaultLabel);

            if ((Options & FastObjectRWOptions.NotFoundException) != 0)
            {
                ilGen.LoadString(typeof(T).Name);

                ilGen.LoadArgument(1);

                EmitToString<TKey>(ilGen);

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
                    if (field.CannotGetException)
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

        public static void ImplOnWriteValue<TKey>(ILGenerator ilGen)
        {
            var fields = Fields;

            var cases = new CaseInfo<TKey>[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                cases[i] = new CaseInfo<TKey>(GetKeyByIndex<TKey>(i), ilGen.DefineLabel());
            }

            var DefaultLabel = ilGen.DefineLabel();

            EmitSwitch(ilGen, iLGen => ilGen.LoadArgument(1), cases, DefaultLabel);

            ilGen.MarkLabel(DefaultLabel);

            if ((Options & FastObjectRWOptions.NotFoundException) != 0)
            {
                ilGen.LoadString(typeof(T).Name);

                ilGen.LoadArgument(1);

                EmitToString<TKey>(ilGen);

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
                    if (field.CannotSetException)
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

        public static void ImplGetOrdinal<TKey>(ILGenerator ilGen)
        {
            var fields = Fields;

            var cases = new CaseInfo<TKey>[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                cases[i] = new CaseInfo<TKey>(GetKeyByIndex<TKey>(i), ilGen.DefineLabel());
            }

            var DefaultLabel = ilGen.DefineLabel();

            EmitSwitch(ilGen, iLGen => ilGen.LoadArgument(1), cases, DefaultLabel);

            ilGen.MarkLabel(DefaultLabel);

            ilGen.LoadConstant(-1);
            ilGen.Return();

            for (int i = 0; i < fields.Length; i++)
            {
                var @case = cases[i];

                ilGen.MarkLabel(@case.Label);

                ilGen.LoadConstant(i);
                ilGen.Return();
            }
        }
    }
}