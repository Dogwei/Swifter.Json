using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Swifter
{
    public sealed class UnsafeBuilder
    {
        public const string AssemblyName = "Swifter.Unsafe";
        public const string ModuleName = "Swifter.Unsafe.dll";
        public const string ClassName = "Swifter.Unsafe";
        public const string IsReadOnlyAttributeName = "System.Runtime.CompilerServices.IsReadOnlyAttribute";

        public const MethodImplAttributes AggressiveInlining = (MethodImplAttributes)256;

        private readonly AssemblyBuilder assemblyBuilder;
        private readonly ModuleBuilder moduleBuilder;
        private readonly TypeBuilder typeBuilder;
        private readonly Type typeIsReadOnlyAttribute;

        public CustomAttributeBuilder CreateIsReadOnlyAttributeBuilder()
        {
            return new CustomAttributeBuilder(typeIsReadOnlyAttribute.GetConstructor(Type.EmptyTypes), new object[] { }); ;
        }

        public Type BuildIsReadOnlyAttribute()
        {
            var typeBuilder = moduleBuilder.DefineType(
                IsReadOnlyAttributeName,
                TypeAttributes.NotPublic | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

            return typeBuilder.CreateType();
        }

        public UnsafeBuilder()
        {
            var assemblyName = new AssemblyName(
                AssemblyName);


            assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    assemblyName,
                    AssemblyBuilderAccess.RunAndSave);

            moduleBuilder = assemblyBuilder.DefineDynamicModule(
                AssemblyName,
                ModuleName);

            typeBuilder = moduleBuilder.DefineType(
                ClassName,
                TypeAttributes.Public | TypeAttributes.Abstract |
                TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

            typeIsReadOnlyAttribute = Type.GetType("System.Runtime.CompilerServices.IsReadOnlyAttribute");

            if (typeIsReadOnlyAttribute == null || !typeIsReadOnlyAttribute.IsVisible)
            {
                typeIsReadOnlyAttribute = BuildIsReadOnlyAttribute();
            }
        }

        public void BuildByReference()
        {
            var byRefBuilder = moduleBuilder.DefineType(
                "Swifter.ByReference",
                TypeAttributes.Public | TypeAttributes.Sealed, typeof(ValueType));

            byRefBuilder.DefineField("_ref", typeof(int).MakeByRefType(), FieldAttributes.Public);

            byRefBuilder.CreateType();
        }

        public void BuildAll()
        {
            BuildByReference();

            Add();

            AddByteOffset();

            AreSame();

            As();

            AsPointer();

            AsIntPtr();

            AsObject();

            AsByte();

            AsRef();

            ByteOffset();

            Copy();

            CopyBlock();

            CopyBlockUnaligned();

            InitBlock();

            InitBlockUnaligned();

            IsAddressGreaterThan();

            IsAddressLessThan();

            Read();

            ReadAndAdd();

            //ReadAndSubtract();

            ReadUnaligned();

            SizeOf();

            // USizeOf();

            Subtract();

            SubtractByteOffset();

            Unbox();

            Write();

            WriteAndAdd();

            //WriteAndSubtract();

            WriteUnaligned();

            GetObjectTypeHandle();
        }

        public void Save()
        {
            typeBuilder.CreateType();

            assemblyBuilder.Save(ModuleName);
        }

        public void Add()
        {
            RefT_IntPtr();
            IntPtr_Int();
            RefT_Int();



            void RefT_IntPtr()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Add), 
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeof(IntPtr));

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "elementOffset");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Sizeof, typeT);

                ilGen.Emit(OpCodes.Mul);
                ilGen.Emit(OpCodes.Add);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }


            void IntPtr_Int()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Add),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                methodBuilder.SetParameters(typeof(void*), typeof(int));

                methodBuilder.SetReturnType(typeof(void*));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "elementOffset");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Sizeof, typeT);
                ilGen.Emit(OpCodes.Conv_I);

                ilGen.Emit(OpCodes.Mul);
                ilGen.Emit(OpCodes.Add);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }


            void RefT_Int()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Add),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeof(int));

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "elementOffset");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Sizeof, typeT);
                ilGen.Emit(OpCodes.Conv_I);

                ilGen.Emit(OpCodes.Mul);
                ilGen.Emit(OpCodes.Add);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void AddByteOffset()
        {
            RefT_IntPtr();

            RefT_Int();

            void RefT_IntPtr()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(AddByteOffset),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeof(IntPtr));

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "byteOffset");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Add);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }

            void RefT_Int()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(AddByteOffset),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeof(int));

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "byteOffset");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Add);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void AreSame()
        {
            RefT_RefT();

            void RefT_RefT()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(AreSame),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeRefT);

                methodBuilder.SetReturnType(typeof(bool));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "left");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "right");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Ceq);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void As()
        {
            TFrom();
            Object();

            void TFrom()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(As),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeTFrom, typeTTo, length) = methodBuilder.DefineGenericParameters("TFrom", "TTo");

                var typeRefTFrom = typeTFrom.MakeByRefType();
                var typeRefTTo = typeTTo.MakeByRefType();

                methodBuilder.SetParameters(typeRefTFrom);

                methodBuilder.SetReturnType(typeRefTTo);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }

            void Object()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(As),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeTTo, length) = methodBuilder.DefineGenericParameters("TTo");

                typeTTo.SetGenericParameterAttributes(GenericParameterAttributes.ReferenceTypeConstraint);

                methodBuilder.SetParameters(typeof(object));

                methodBuilder.SetReturnType(typeTTo);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "o");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void AsPointer()
        {
            RefT();

            void RefT()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(AsPointer),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");
                
                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT);

                methodBuilder.SetReturnType(typeof(void*));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "value");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Conv_U);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void AsIntPtr()
        {
            RefT();

            void RefT()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(AsIntPtr),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT);

                methodBuilder.SetReturnType(typeof(IntPtr));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "value");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Conv_U);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void AsObject()
        {
            RefT();

            void RefT()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(AsObject),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT);

                methodBuilder.SetReturnType(typeof(object));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "value");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void AsByte()
        {
            RefT();

            void RefT()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(AsByte),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                var typeRefByte = typeof(byte).MakeByRefType();

                methodBuilder.SetParameters(typeRefT);

                methodBuilder.SetReturnType(typeRefByte);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "value");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void AsRef()
        {
            Void();
            Object();
            RefT();
            TypedReference();

            void Void()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(AsRef),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeof(void*));

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }

            void Object()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(AsRef),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeof(object));

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "o");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }

            void RefT()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(AsRef),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT);

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source").SetCustomAttribute(CreateIsReadOnlyAttributeBuilder());

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }

            void TypedReference()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(AsRef),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeof(TypedReference));

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarga_S, 0);

                ilGen.Emit(OpCodes.Ldind_I);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void ByteOffset()
        {
            RefT_RefT();

            void RefT_RefT()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(ByteOffset),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeRefT);

                methodBuilder.SetReturnType(typeof(IntPtr));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "origin");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "target");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Sub);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }
        
        public void Copy()
        {
            RefT_Void();
            Void_RefT();



            void RefT_Void()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Copy),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeof(void*));

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "source");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Ldobj, typeT);
                ilGen.Emit(OpCodes.Stobj, typeT);
                
                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }


            void Void_RefT()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Copy),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeof(void*), typeRefT);

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "source");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Ldobj, typeT);
                ilGen.Emit(OpCodes.Stobj, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void CopyBlock()
        {
            RefByte_RefByte_UInt();
            Void_Void_UInt();


            void RefByte_RefByte_UInt()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(CopyBlock),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var typeRefByte = typeof(byte).MakeByRefType();

                methodBuilder.SetParameters(typeRefByte, typeRefByte, typeof(uint));

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(3, ParameterAttributes.None, "byteCount");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_2);

                ilGen.Emit(OpCodes.Cpblk);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }


            void Void_Void_UInt()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(CopyBlock),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);
                
                methodBuilder.SetParameters(typeof(void*), typeof(void*), typeof(uint));

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(3, ParameterAttributes.None, "byteCount");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_2);

                ilGen.Emit(OpCodes.Cpblk);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void CopyBlockUnaligned()
        {
            RefByte_RefByte_UInt();
            Void_Void_UInt();


            void RefByte_RefByte_UInt()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(CopyBlockUnaligned),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var typeRefByte = typeof(byte).MakeByRefType();

                methodBuilder.SetParameters(typeRefByte, typeRefByte, typeof(uint));

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(3, ParameterAttributes.None, "byteCount");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_2);

                ilGen.Emit(OpCodes.Unaligned, (byte)1);
                ilGen.Emit(OpCodes.Cpblk);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }


            void Void_Void_UInt()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(CopyBlockUnaligned),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                methodBuilder.SetParameters(typeof(void*), typeof(void*), typeof(uint));

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(3, ParameterAttributes.None, "byteCount");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_2);

                ilGen.Emit(OpCodes.Unaligned, (byte)1);
                ilGen.Emit(OpCodes.Cpblk);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void InitBlock()
        {
            Void_Byte_UInt();
            RefByte_Byte_UInt();


            void Void_Byte_UInt()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(InitBlock),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                methodBuilder.SetParameters(typeof(void*), typeof(byte), typeof(uint));

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "startAddress");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "value");
                methodBuilder.DefineParameter(3, ParameterAttributes.None, "byteCount");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_2);

                ilGen.Emit(OpCodes.Initblk);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }


            void RefByte_Byte_UInt()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(InitBlock),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var typeRefByte = typeof(byte).MakeByRefType();

                methodBuilder.SetParameters(typeRefByte, typeof(byte), typeof(uint));

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "startAddress");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "value");
                methodBuilder.DefineParameter(3, ParameterAttributes.None, "byteCount");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_2);

                ilGen.Emit(OpCodes.Initblk);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void InitBlockUnaligned()
        {
            Void_Byte_UInt();
            RefByte_Byte_UInt();


            void Void_Byte_UInt()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(InitBlockUnaligned),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                methodBuilder.SetParameters(typeof(void*), typeof(byte), typeof(uint));

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "startAddress");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "value");
                methodBuilder.DefineParameter(3, ParameterAttributes.None, "byteCount");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_2);

                ilGen.Emit(OpCodes.Unaligned, (byte)1);
                ilGen.Emit(OpCodes.Initblk);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }


            void RefByte_Byte_UInt()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(InitBlockUnaligned),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var typeRefByte = typeof(byte).MakeByRefType();

                methodBuilder.SetParameters(typeRefByte, typeof(byte), typeof(uint));

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "startAddress");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "value");
                methodBuilder.DefineParameter(3, ParameterAttributes.None, "byteCount");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_2);

                ilGen.Emit(OpCodes.Unaligned, (byte)1);
                ilGen.Emit(OpCodes.Initblk);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void IsAddressGreaterThan()
        {
            RefT_RefT();

            void RefT_RefT()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(IsAddressGreaterThan),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeRefT);

                methodBuilder.SetReturnType(typeof(bool));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "left");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "right");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Cgt_Un);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void IsAddressLessThan()
        {
            RefT_RefT();

            void RefT_RefT()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(IsAddressLessThan),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeRefT);

                methodBuilder.SetReturnType(typeof(bool));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "left");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "right");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Clt_Un);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void Read()
        {
            Void();
            RefByte();

            void Void()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Read),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                methodBuilder.SetParameters(typeof(void*));

                methodBuilder.SetReturnType(typeT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Ldobj, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }

            void RefByte()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Read),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefByte = typeof(byte).MakeByRefType();

                methodBuilder.SetParameters(typeRefByte);

                methodBuilder.SetReturnType(typeT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                
                ilGen.Emit(OpCodes.Ldobj, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void ReadAndAdd()
        {
            RefVoid();

            void RefVoid()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(ReadAndAdd),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefVoid = typeof(void*).MakeByRefType();

                methodBuilder.SetParameters(typeRefVoid);

                methodBuilder.SetReturnType(typeT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination").SetCustomAttribute(CreateIsReadOnlyAttributeBuilder());

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldind_I);

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Dup);
                ilGen.Emit(OpCodes.Ldind_I);
                ilGen.Emit(OpCodes.Sizeof, typeT);
                ilGen.Emit(OpCodes.Add);
                ilGen.Emit(OpCodes.Stind_I);

                ilGen.Emit(OpCodes.Ldobj, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void ReadAndSubtract()
        {
            RefVoid();

            void RefVoid()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(ReadAndSubtract),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefVoid = typeof(void*).MakeByRefType();

                methodBuilder.SetParameters(typeRefVoid);

                methodBuilder.SetReturnType(typeT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination").SetCustomAttribute(CreateIsReadOnlyAttributeBuilder());

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldind_I);
                
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Dup);
                ilGen.Emit(OpCodes.Ldind_I);
                ilGen.Emit(OpCodes.Sizeof, typeT);
                ilGen.Emit(OpCodes.Sub);
                ilGen.Emit(OpCodes.Stind_I);

                ilGen.Emit(OpCodes.Ldobj, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void ReadUnaligned()
        {
            Void();
            RefByte();

            void Void()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(ReadUnaligned),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                methodBuilder.SetParameters(typeof(void*));

                methodBuilder.SetReturnType(typeT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Unaligned, (byte)1);
                ilGen.Emit(OpCodes.Ldobj, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }

            void RefByte()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(ReadUnaligned),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefByte = typeof(byte).MakeByRefType();

                methodBuilder.SetParameters(typeRefByte);

                methodBuilder.SetReturnType(typeT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Unaligned, (byte)1);
                ilGen.Emit(OpCodes.Ldobj, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void SizeOf()
        {
            _();

            void _()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(SizeOf),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");
                
                methodBuilder.SetReturnType(typeof(int));

                var ilGen = methodBuilder.GetILGenerator();
                
                ilGen.Emit(OpCodes.Sizeof, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void USizeOf()
        {
            _();

            void _()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(USizeOf),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                methodBuilder.SetReturnType(typeof(uint));

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Sizeof, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }
        
        public void Subtract()
        {
            RefT_IntPtr();
            IntPtr_Int();
            RefT_Int();



            void RefT_IntPtr()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Subtract),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeof(IntPtr));

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "elementOffset");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Sizeof, typeT);

                ilGen.Emit(OpCodes.Mul);
                ilGen.Emit(OpCodes.Sub);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }


            void IntPtr_Int()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Subtract),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                methodBuilder.SetParameters(typeof(void*), typeof(int));

                methodBuilder.SetReturnType(typeof(void*));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "elementOffset");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Sizeof, typeT);
                ilGen.Emit(OpCodes.Conv_I);

                ilGen.Emit(OpCodes.Mul);
                ilGen.Emit(OpCodes.Sub);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }


            void RefT_Int()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Subtract),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeof(int));

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "elementOffset");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Sizeof, typeT);
                ilGen.Emit(OpCodes.Conv_I);

                ilGen.Emit(OpCodes.Mul);
                ilGen.Emit(OpCodes.Sub);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void SubtractByteOffset()
        {
            RefT_IntPtr();
            RefT_Int();

            void RefT_IntPtr()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(SubtractByteOffset),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeof(IntPtr));

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "byteOffset");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Sub);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }

            void RefT_Int()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(SubtractByteOffset),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeRefT, typeof(int));

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "source");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "byteOffset");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Sub);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void Unbox()
        {
            Object();

            void Object()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Unbox),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                typeT.SetGenericParameterAttributes(GenericParameterAttributes.NotNullableValueTypeConstraint);

                var typeRefT = typeT.MakeByRefType();

                methodBuilder.SetParameters(typeof(object));

                methodBuilder.SetReturnType(typeRefT);

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "box");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Unbox, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void Write()
        {
            Void_T();
            RefByte_T();

            void Void_T()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Write),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                methodBuilder.SetParameters(typeof(void*), typeT);

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "value");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Stobj, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }

            void RefByte_T()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(Write),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefByte = typeof(byte).MakeByRefType();

                methodBuilder.SetParameters(typeRefByte, typeT);

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "value");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                
                ilGen.Emit(OpCodes.Stobj, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void WriteAndAdd()
        {
            RefVoid_T();

            void RefVoid_T()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(WriteAndAdd),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefVoid = typeof(void*).MakeByRefType();

                methodBuilder.SetParameters(typeRefVoid, typeT);

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination").SetCustomAttribute(CreateIsReadOnlyAttributeBuilder());
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "value");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Dup);
                ilGen.Emit(OpCodes.Ldind_I);
                ilGen.Emit(OpCodes.Dup);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Stobj, typeT);

                ilGen.Emit(OpCodes.Sizeof, typeT);
                ilGen.Emit(OpCodes.Add);
                ilGen.Emit(OpCodes.Stind_I);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void WriteAndSubtract()
        {
            RefVoid_T();

            void RefVoid_T()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(WriteAndSubtract),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefVoid = typeof(void*).MakeByRefType();

                methodBuilder.SetParameters(typeRefVoid, typeT);

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination").SetCustomAttribute(CreateIsReadOnlyAttributeBuilder());
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "value");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Dup);
                ilGen.Emit(OpCodes.Ldind_I);
                ilGen.Emit(OpCodes.Dup);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Stobj, typeT);

                ilGen.Emit(OpCodes.Sizeof, typeT);
                ilGen.Emit(OpCodes.Sub);
                ilGen.Emit(OpCodes.Stind_I);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void WriteUnaligned()
        {
            Void_T();
            RefByte_T();


            void Void_T()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(WriteUnaligned),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                methodBuilder.SetParameters(typeof(void*), typeT);

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "value");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Unaligned, (byte)1);
                ilGen.Emit(OpCodes.Stobj, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }

            void RefByte_T()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(WriteUnaligned),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                var (typeT, length) = methodBuilder.DefineGenericParameters("T");

                var typeRefByte = typeof(byte).MakeByRefType();

                methodBuilder.SetParameters(typeRefByte, typeT);

                methodBuilder.SetReturnType(typeof(void));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination");
                methodBuilder.DefineParameter(2, ParameterAttributes.None, "value");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Unaligned, (byte)1);
                ilGen.Emit(OpCodes.Stobj, typeT);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }

        public void GetObjectTypeHandle()
        {
            Object();

            void Object()
            {
                var methodBuilder = typeBuilder.DefineMethod(nameof(GetObjectTypeHandle),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static);

                methodBuilder.SetParameters(typeof(object));

                methodBuilder.SetReturnType(typeof(IntPtr));

                methodBuilder.DefineParameter(1, ParameterAttributes.None, "o");

                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);

                ilGen.Emit(OpCodes.Ldind_I);

                ilGen.Emit(OpCodes.Ret);

                methodBuilder.SetImplementationFlags(AggressiveInlining);
            }
        }
    }
}