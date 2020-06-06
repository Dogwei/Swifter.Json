using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

using static Swifter.RW.StaticFastObjectRW;
using static Swifter.Tools.MethodHelper;

namespace Swifter.RW
{
    internal static class StaticFastObjectRW
    {
        public const BindingFlags StaticDeclaredOnly = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static readonly MethodInfo IValueFilterFilterMethod = typeof(IValueFilter<string>).GetMethod(nameof(IValueFilter<string>.Filter));
        public static readonly MethodInfo ValueCopyerWriteToMethod = typeof(ValueCopyer).GetMethod(nameof(ValueCopyer.WriteTo));
        //public static readonly MethodInfo IDataReaderIndexerGetMethod = typeof(IDataReader<string>).GetProperty(new Type[] { typeof(string) }).GetGetMethod(true);
        //public static readonly MethodInfo IDataWriterIndexerGetMethod = typeof(IDataWriter<string>).GetProperty(new Type[] { typeof(string) }).GetGetMethod(true);

        public static readonly MethodInfo GetTypeHandle_Object = typeof(TypeHelper).GetMethod(nameof(TypeHelper.GetTypeHandle), new Type[] { typeof(object) });
        public static readonly MethodInfo GetInterface_Object = typeof(ValueInterface).GetMethod(nameof(ValueInterface.GetInterface), new Type[] { typeof(object) });
        public static readonly MethodInfo Write_IValueWriter_Object = typeof(ValueInterface).GetMethod(nameof(ValueInterface.Write), new Type[] { typeof(IValueWriter), typeof(object) });

        public static readonly ConstructorInfo MemberAccessException_String_Constructor = typeof(MemberAccessException).GetConstructor(new Type[] { typeof(string) });
        public static readonly ConstructorInfo MissingMemberException_String_String_Constructor = typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string), typeof(string) });
        public static readonly ConstructorInfo MissingMemberException_Constructor = typeof(MissingMemberException).GetConstructor(new Type[] { });




        public static readonly bool DynamicAssemblyCanAccessNonPublicTypes;
        public static readonly bool DynamicAssemblyCanAccessNonPublicMembers;

        public static readonly Random RandomInstance = new Random();

        static StaticFastObjectRW()
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

            try
            {
                var dynamicMethodName = nameof(TestClass.TestMethod);

                var TypeBuilder = DynamicAssembly.DefineType(nameof(TestClass) + 2, TypeAttributes.Public);

                var methodBuilder = TypeBuilder.DefineMethod(
                    dynamicMethodName,
                    MethodAttributes.Public | MethodAttributes.Static,
                    CallingConventions.Standard,
                    typeof(void),
                    Type.EmptyTypes);

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Call(typeof(TestClass).GetMethod(nameof(TestClass.TestMethod), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));
                ilGen.Return();

                var method = TypeBuilder.CreateTypeInfo().GetMethod(dynamicMethodName);

                method.Invoke(null, null);

                DynamicAssemblyCanAccessNonPublicMembers = true;
            }
            catch (Exception)
            {
                DynamicAssemblyCanAccessNonPublicMembers = false;
            }
        }

        private class TestClass
        {
            internal static void TestMethod()
            {

            }
        }
    }

    internal sealed unsafe partial class StaticFastObjectRW<T>
    {
        public static readonly string[] StringKeys;
        public static readonly Ps<char>[] UTF16Keys;
        public static readonly Ps<Utf8Byte>[] UTF8Keys;


        public static readonly BaseField[] Fields;

        public static readonly IFastObjectRWCreater<T> Creater;

        public static readonly FastObjectRWOptions Options;

        public static bool HaveNonPublicReadMember;
        public static bool HaveNonPublicWriteMember;


        public static FieldInfo StringKeysField => typeof(StaticFastObjectRW<T>).GetField(nameof(StringKeys));

        public static FieldInfo UTF16KeysField => typeof(StaticFastObjectRW<T>).GetField(nameof(UTF16Keys));

        public static FieldInfo UTF8KeysField => typeof(StaticFastObjectRW<T>).GetField(nameof(UTF8Keys));

        public static MethodInfo GetValueInterfaceInstanceMethod => typeof(FastObjectRW<T>).GetMethod(nameof(FastObjectRW<T>.GetValueInterfaceInstance), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        public static FieldInfo ContentField
        {
            get
            {
                return typeof(T).IsExternalVisible() || (DynamicAssemblyCanAccessNonPublicTypes && DynamicAssemblyCanAccessNonPublicMembers)
                    ? typeof(FastObjectRW<T>).GetField(nameof(FastObjectRW<T>.content), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    : TypeBuilder.GetField(TypeBuilder.BaseType, typeof(FastObjectRW<>).GetField(nameof(FastObjectRW<T>.content), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            }
        }

        public static Type ContentType
        {
            get
            {
                if (typeof(T).IsExternalVisible() || DynamicAssemblyCanAccessNonPublicTypes)
                {
                    return typeof(T);
                }
                else if (TypeBuilder.IsGenericTypeDefinition && TypeBuilder.GetGenericArguments() is Type[] genericArgs && genericArgs.Length == 1)
                {
                    return genericArgs[0];
                }

                return typeof(T);
            }
        }

        public static FieldInfo RTContentField
        {
            get
            {
                return typeof(FastObjectRW<T>).GetField(nameof(FastObjectRW<T>.content), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }

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
                            var attributedField = new FastField(item, (RWFieldAttribute)attribute);

                            if (attributedField.CanRead || attributedField.CanWrite)
                            {
                                SaveField(attributedField);

                                if (attributedField.CanRead && !attributedField.IsPublicGet)
                                {
                                    HaveNonPublicReadMember = true;
                                }

                                if (attributedField.CanWrite && !attributedField.IsPublicSet)
                                {
                                    HaveNonPublicWriteMember = true;
                                }
                            }
                        }
                    }
                    else if ((Options & FastObjectRWOptions.Field) != 0 && item.IsPublic)
                    {
                        var field = new FastField(item, null);

                        if (field.CanRead || field.CanWrite)
                        {
                            SaveField(field);

                            if (field.CanRead && !field.IsPublicGet)
                            {
                                HaveNonPublicReadMember = true;
                            }

                            if (field.CanWrite && !field.IsPublicSet)
                            {
                                HaveNonPublicWriteMember = true;
                            }
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
                            var attributedField = new FastProperty(item, (RWFieldAttribute)attribute);

                            if (attributedField.CanRead || attributedField.CanWrite)
                            {
                                SaveField(attributedField);

                                if (attributedField.CanRead && !attributedField.IsPublicGet)
                                {
                                    HaveNonPublicReadMember = true;
                                }

                                if (attributedField.CanWrite && !attributedField.IsPublicSet)
                                {
                                    HaveNonPublicWriteMember = true;
                                }
                            }
                        }
                    }
                    else if ((Options & FastObjectRWOptions.Property) != 0)
                    {
                        var field = new FastProperty(item, null);

                        if (field.CanRead || field.CanWrite)
                        {
                            SaveField(field);

                            if (field.CanRead && !field.IsPublicGet)
                            {
                                HaveNonPublicReadMember = true;
                            }

                            if (field.CanWrite && !field.IsPublicSet)
                            {
                                HaveNonPublicWriteMember = true;
                            }
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

        public static void SetUTF16Keys()
        {
            var total_length = 0;

            foreach (var item in StringKeys)
            {
                total_length += item.Length;
            }

            var hGChars = (char*)Marshal.AllocHGlobal(sizeof(char) * total_length);

            for (int i = 0; i < UTF16Keys.Length; i++)
            {
                var item = StringKeys[i];

                for (int j = 0; j < item.Length; j++)
                {
                    hGChars[j] = item[j];
                }

                UTF16Keys[i] = new Ps<char>(hGChars, item.Length);

                hGChars += item.Length;
            }
        }

        public static void SetUTF8Keys()
        {
            var total_length = 0;

            for (int i = 0; i < UTF8Keys.Length; i++)
            {
                total_length += StringHelper.GetUtf8BytesLength(ref StringHelper.GetRawStringData(StringKeys[i]), StringKeys[i].Length);
            }

            var hGChars = (Utf8Byte*)Marshal.AllocHGlobal(sizeof(Utf8Byte) * total_length);

            for (int i = 0; i < UTF8Keys.Length; i++)
            {
                int length = StringHelper.GetUtf8Bytes(ref StringHelper.GetRawStringData(StringKeys[i]), StringKeys[i].Length, (byte*)hGChars);

                UTF8Keys[i] = new Ps<Utf8Byte>(hGChars, length);

                hGChars += length;
            }
        }

        public static void LoadContent(ILGenerator ilGen)
        {
            ilGen.LoadArgument(0);

            if (typeof(T).IsValueType)
            {
                ilGen.LoadFieldAddress(RTContentField);
            }
            else
            {
                ilGen.LoadField(RTContentField);
            }
        }

        public static TKey GetKeyByIndex<TKey>(int index)
        {
            if (typeof(TKey) == typeof(Ps<char>))
            {
                return Underlying.As<TKey[]>(UTF16Keys)[index];
            }
            else if (typeof(TKey) == typeof(Ps<Utf8Byte>))
            {
                return Underlying.As<TKey[]>(UTF8Keys)[index];
            }
            else if (typeof(TKey) == typeof(string))
            {
                return Underlying.As<TKey[]>(StringKeys)[index];
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
                return Array.IndexOf(UTF16Keys, Underlying.As<TKey, Ps<char>>(ref key));
            }
            else if (typeof(TKey) == typeof(Ps<Utf8Byte>))
            {
                return Array.IndexOf(UTF8Keys, Underlying.As<TKey, Ps<Utf8Byte>>(ref key));
            }
            else if (typeof(TKey) == typeof(string))
            {
                return Array.IndexOf(StringKeys, Underlying.As<TKey, string>(ref key));
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
                ilGen.LoadString(StringKeys[index]);
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

                    StringKeys = Fields.Select(item => item.Name).ToArray();

                    UTF16Keys = new Ps<char>[StringKeys.Length];

                    UTF8Keys = new Ps<Utf8Byte>[StringKeys.Length];

                    GCHandle.Alloc(UTF16Keys, GCHandleType.Pinned);

                    GCHandle.Alloc(UTF8Keys, GCHandleType.Pinned);

                    SetUTF16Keys();

                    SetUTF8Keys();

                    Creater = CreateCreater();
                }
                catch (Exception e)
                {
                    Creater = new ErrorFastObjectRWCreater<T>(e);
                }
            }
        }


        public static TypeBuilder TypeBuilder;

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
                    return ValueInterface<bool>.IsNotModified ? nameof(IValueReader.ReadBoolean) : null;
                case TypeCode.Char:
                    return ValueInterface<char>.IsNotModified ? nameof(IValueReader.ReadChar) : null;
                case TypeCode.SByte:
                    return ValueInterface<sbyte>.IsNotModified ? nameof(IValueReader.ReadSByte) : null;
                case TypeCode.Byte:
                    return ValueInterface<byte>.IsNotModified ? nameof(IValueReader.ReadByte) : null;
                case TypeCode.Int16:
                    return ValueInterface<short>.IsNotModified ? nameof(IValueReader.ReadInt16) : null;
                case TypeCode.UInt16:
                    return ValueInterface<ushort>.IsNotModified ? nameof(IValueReader.ReadUInt16) : null;
                case TypeCode.Int32:
                    return ValueInterface<int>.IsNotModified ? nameof(IValueReader.ReadInt32) : null;
                case TypeCode.UInt32:
                    return ValueInterface<uint>.IsNotModified ? nameof(IValueReader.ReadUInt32) : null;
                case TypeCode.Int64:
                    return ValueInterface<long>.IsNotModified ? nameof(IValueReader.ReadInt64) : null;
                case TypeCode.UInt64:
                    return ValueInterface<ulong>.IsNotModified ? nameof(IValueReader.ReadUInt64) : null;
                case TypeCode.Single:
                    return ValueInterface<float>.IsNotModified ? nameof(IValueReader.ReadSingle) : null;
                case TypeCode.Double:
                    return ValueInterface<double>.IsNotModified ? nameof(IValueReader.ReadDouble) : null;
                case TypeCode.Decimal:
                    return ValueInterface<decimal>.IsNotModified ? nameof(IValueReader.ReadDecimal) : null;
                case TypeCode.DateTime:
                    return ValueInterface<DateTime>.IsNotModified ? nameof(IValueReader.ReadDateTime) : null;
                case TypeCode.String:
                    return ValueInterface<string>.IsNotModified ? nameof(IValueReader.ReadString) : null;
            }

            if (type.IsValueType && Nullable.GetUnderlyingType(type) != null && ValueInterface.GetInterface(type).InterfaceIsNotModified)
            {
                return nameof(IValueReader.ReadNullable);
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
                    return ValueInterface<bool>.IsNotModified ? nameof(IValueWriter.WriteBoolean) : null;
                case TypeCode.Char:
                    return ValueInterface<char>.IsNotModified ? nameof(IValueWriter.WriteChar) : null;
                case TypeCode.SByte:
                    return ValueInterface<sbyte>.IsNotModified ? nameof(IValueWriter.WriteSByte) : null;
                case TypeCode.Byte:
                    return ValueInterface<byte>.IsNotModified ? nameof(IValueWriter.WriteByte) : null;
                case TypeCode.Int16:
                    return ValueInterface<short>.IsNotModified ? nameof(IValueWriter.WriteInt16) : null;
                case TypeCode.UInt16:
                    return ValueInterface<ushort>.IsNotModified ? nameof(IValueWriter.WriteUInt16) : null;
                case TypeCode.Int32:
                    return ValueInterface<int>.IsNotModified ? nameof(IValueWriter.WriteInt32) : null;
                case TypeCode.UInt32:
                    return ValueInterface<uint>.IsNotModified ? nameof(IValueWriter.WriteUInt32) : null;
                case TypeCode.Int64:
                    return ValueInterface<long>.IsNotModified ? nameof(IValueWriter.WriteInt64) : null;
                case TypeCode.UInt64:
                    return ValueInterface<ulong>.IsNotModified ? nameof(IValueWriter.WriteUInt64) : null;
                case TypeCode.Single:
                    return ValueInterface<float>.IsNotModified ? nameof(IValueWriter.WriteSingle) : null;
                case TypeCode.Double:
                    return ValueInterface<double>.IsNotModified ? nameof(IValueWriter.WriteDouble) : null;
                case TypeCode.Decimal:
                    return ValueInterface<decimal>.IsNotModified ? nameof(IValueWriter.WriteDecimal) : null;
                case TypeCode.DateTime:
                    return ValueInterface<DateTime>.IsNotModified ? nameof(IValueWriter.WriteDateTime) : null;
                case TypeCode.String:
                    return ValueInterface<string>.IsNotModified ? nameof(IValueWriter.WriteString) : null;
            }

            return null;
        }

        public static IFastObjectRWCreater<T> CreateCreater()
        {
            TypeBuilder = DynamicAssembly.DefineType(
                $"{nameof(FastObjectRW)}_{typeof(T).Name}_{Guid.NewGuid().ToString("N")}",
                TypeAttributes.Sealed | TypeAttributes.Public);

            Type tType;

            if (typeof(T).IsExternalVisible() || DynamicAssemblyCanAccessNonPublicTypes)
            {
                TypeBuilder.SetParent(typeof(FastObjectRW<T>));

                TypeBuilder.AddInterfaceImplementation(typeof(IFastObjectRWCreater<T>));
                TypeBuilder.AddInterfaceImplementation(typeof(IValueInterface<T>));

                tType = typeof(T);
            }
            else
            {
                tType = TypeBuilder.DefineGenericParameters("T")[0];

                TypeBuilder.SetParent(typeof(FastObjectRW<>).MakeGenericType(tType));

                TypeBuilder.AddInterfaceImplementation(typeof(IFastObjectRWCreater<>).MakeGenericType(tType));
                TypeBuilder.AddInterfaceImplementation(typeof(IValueInterface<>).MakeGenericType(tType));
            }

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

            var defaultConstructor = TypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            ImplCreate(defaultConstructor);

            ImplReadValue(defaultConstructor);

            ImplWriteValue(defaultConstructor);

            Type rtType = TypeBuilder.CreateTypeInfo();

            if (rtType.IsGenericTypeDefinition)
            {
                rtType = rtType.MakeGenericType(typeof(T));
            }

            var creater = (IFastObjectRWCreater<T>)Activator.CreateInstance(rtType);

            if (ValueInterface<T>.Content is FastObjectInterface<T>)
            {
                ValueInterface<T>.SetInterface((IValueInterface<T>)creater);
            }

            return creater;
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

            if ((typeof(T).IsExternalVisible() && (!HaveNonPublicWriteMember)) || (DynamicAssemblyCanAccessNonPublicTypes && DynamicAssemblyCanAccessNonPublicMembers))
            {
                ImplOnWriteValue<TKey>(methodBuilder.GetILGenerator());
            }
            else
            {
                var dynamicMethod = new DynamicMethod(
                    $"{TypeBuilder.Name}_{nameof(ImplOnWriteValue)}",
                    typeof(void),
                    new Type[] { typeof(FastObjectRW<T>), typeof(TKey), typeof(IValueReader) },
                    typeof(FastObjectRW<T>).Module, true);

                ImplOnWriteValue<TKey>(dynamicMethod.GetILGenerator());

                methodBuilder.GetILGenerator()
                    .LoadArgument(0)
                    .LoadArgument(1)
                    .LoadArgument(2)
                    .Calli(dynamicMethod)
                    .Return();
            }
        }

        public static void ImplOnReadAll<TKey>()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadAll),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IDataWriter<TKey>) });

            if ((typeof(T).IsExternalVisible() && (!HaveNonPublicReadMember)) || (DynamicAssemblyCanAccessNonPublicTypes && DynamicAssemblyCanAccessNonPublicMembers))
            {
                ImplOnReadAll<TKey>(methodBuilder.GetILGenerator());
            }
            else
            {
                var dynamicMethod = new DynamicMethod(
                    $"{TypeBuilder.Name}_{nameof(ImplOnReadAll)}",
                    typeof(void),
                    new Type[] { typeof(FastObjectRW<T>), typeof(IDataWriter<TKey>) },
                    typeof(FastObjectRW<T>).Module,
                    true);

                ImplOnReadAll<TKey>(dynamicMethod.GetILGenerator());

                methodBuilder.GetILGenerator()
                    .LoadArgument(0)
                    .LoadArgument(1)
                    .Calli(dynamicMethod)
                    .Return();
            }
        }

        public static void ImplOnWriteAll<TKey>()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnWriteAll),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(IDataReader<TKey>) });


            if ((typeof(T).IsExternalVisible() && (!HaveNonPublicWriteMember)) || (DynamicAssemblyCanAccessNonPublicTypes && DynamicAssemblyCanAccessNonPublicMembers))
            {
                ImplOnWriteAll<TKey>(methodBuilder.GetILGenerator());
            }
            else
            {
                var dynamicMethod = new DynamicMethod(
                    $"{TypeBuilder.Name}_{nameof(ImplOnWriteAll)}",
                    typeof(void),
                    new Type[] { typeof(FastObjectRW<T>), typeof(IDataReader<TKey>) },
                    typeof(FastObjectRW<T>).Module, true);

                ImplOnWriteAll<TKey>(dynamicMethod.GetILGenerator());

                methodBuilder.GetILGenerator()
                    .LoadArgument(0)
                    .LoadArgument(1)
                    .Calli(dynamicMethod)
                    .Return();
            }
        }

        public static void ImplOnReadValue<TKey>()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.OnReadValue),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(void),
                new Type[] { typeof(TKey), typeof(IValueWriter) });

            if ((typeof(T).IsExternalVisible() && (!HaveNonPublicReadMember)) || (DynamicAssemblyCanAccessNonPublicTypes && DynamicAssemblyCanAccessNonPublicMembers))
            {
                ImplOnReadValue<TKey>(methodBuilder.GetILGenerator());
            }
            else
            {
                var dynamicMethod = new DynamicMethod(
                    $"{TypeBuilder.Name}_{nameof(ImplOnReadValue)}",
                    typeof(void),
                    new Type[] { typeof(FastObjectRW<T>), typeof(TKey), typeof(IValueWriter) },
                    typeof(FastObjectRW<T>).Module, true);

                ImplOnReadValue<TKey>(dynamicMethod.GetILGenerator());

                methodBuilder.GetILGenerator()
                    .LoadArgument(0)
                    .LoadArgument(1)
                    .LoadArgument(2)
                    .Calli(dynamicMethod)
                    .Return();
            }
        }

        public static void ImplGetOrdinal<TKey>()
        {
            var methodBuilder = TypeBuilder.DefineMethod(
                nameof(FastObjectRW<T>.GetOrdinal),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(int),
                new Type[] { typeof(TKey) });

            if ((typeof(T).IsExternalVisible() && (!HaveNonPublicReadMember)) || (DynamicAssemblyCanAccessNonPublicTypes && DynamicAssemblyCanAccessNonPublicMembers))
            {
                ImplGetOrdinal<TKey>(methodBuilder.GetILGenerator());
            }
            else
            {
                var dynamicMethod = new DynamicMethod(
                    $"{TypeBuilder.Name}_{nameof(ImplGetOrdinal)}",
                    typeof(int),
                    new Type[] { typeof(FastObjectRW<T>), typeof(TKey) },
                    typeof(FastObjectRW<T>).Module, true);

                ImplGetOrdinal<TKey>(dynamicMethod.GetILGenerator());

                methodBuilder.GetILGenerator()
                    .LoadArgument(0)
                    .LoadArgument(1)
                    .Calli(dynamicMethod)
                    .Return();
            }
        }

        public static void ImplCreate(ConstructorBuilder defaultConstructor)
        {
            var methodBuilder = TypeBuilder.DefineMethod(
               nameof(IFastObjectRWCreater<T>.Create),
               MethodAttributes.Public | MethodAttributes.Virtual,
               CallingConventions.HasThis,
               TypeBuilder.BaseType,
               Type.EmptyTypes);

            var ilGen = methodBuilder.GetILGenerator();

            ilGen.NewObject(defaultConstructor);
            ilGen.Return();
        }

        public static void ImplWriteValue(ConstructorBuilder defaultConstructor)
        {
            var methodBuilder = TypeBuilder.DefineMethod(
            nameof(IValueInterface<T>.WriteValue),
            MethodAttributes.Public | MethodAttributes.Virtual,
            CallingConventions.HasThis,
            typeof(void),
            new Type[] { typeof(IValueWriter), ContentType });

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

        public static void ImplReadValue(ConstructorBuilder defaultConstructor)
        {
            var methodBuilder = TypeBuilder.DefineMethod(
               nameof(IValueInterface<T>.ReadValue),
               MethodAttributes.Public | MethodAttributes.Virtual,
               CallingConventions.HasThis,
               ContentType,
               new Type[] { typeof(IValueReader) });

            var ilGen = methodBuilder.GetILGenerator();

            // var rw = new FastObjectRW_T();
            var local_rw = ilGen.DeclareLocal(TypeBuilder);
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
                var local = ilGen.DeclareLocal(ContentType);

                ilGen.LoadArgument(0);
                ilGen.LoadLocal(local);
                ilGen.StoreField(ContentField);

                ilGen.Return();
            }
            else if ((Options & FastObjectRWOptions.Allocate) != 0)
            {
                ilGen.LoadArgument(0);

                ilGen.LoadType(ContentType);

                ilGen.Call(MethodOf<Type, object>(TypeHelper.Allocate));

                ilGen.StoreField(ContentField);
                ilGen.Return();
            }
            else
            {
                var constructor = typeof(T).GetConstructor(Type.EmptyTypes);

                if (constructor is null)
                {
                    ilGen.LoadArgument(0);
                    ilGen.LoadType(ContentType);
                    ilGen.Call(MethodOf<Type, object>(Activator.CreateInstance));
                    ilGen.StoreField(ContentField);
                    ilGen.Return();

                    return;
                }

                if (typeof(T).IsExternalVisible() || (DynamicAssemblyCanAccessNonPublicTypes && DynamicAssemblyCanAccessNonPublicMembers))
                {
                    ilGen.LoadArgument(0);
                    ilGen.NewObject(constructor);
                    ilGen.StoreField(ContentField);
                    ilGen.Return();
                }
                else
                {
                    var dynamicMethod = new DynamicMethod(
                        $"{TypeBuilder.Name}_{nameof(ImplInitialize)}",
                        typeof(void),
                        new Type[] { typeof(FastObjectRW<T>) },
                        typeof(FastObjectRW<T>).Module, true);

                    dynamicMethod.GetILGenerator()
                        .LoadArgument(0)
                        .NewObject(constructor)
                        .StoreField(RTContentField)
                        .Return();

                    ilGen.LoadArgument(0);
                    ilGen.Calli(dynamicMethod);
                    ilGen.Return();
                }
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