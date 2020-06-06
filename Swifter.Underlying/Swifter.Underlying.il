.assembly Swifter.Underlying
{
  .hash algorithm 0x00008004
  .ver 2:0:0:0
}

.module Swifter.Underlying.dll
// MVID: {1E97D84A-565B-49C5-B60A-F31A1A4ACE13}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x02ED0000


// =============== CLASS MEMBERS DECLARATION ===================

.class public abstract auto ansi sealed beforefieldinit Swifter.Underlying extends [CORE_ASSEMBLY]System.Object
{
  .method public hidebysig static !!T Read<T>(void* source) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )

        .maxstack 1
        ldarg.0
        ldobj !!T
        ret
  } // end of method Underlying::Read

  .method public hidebysig static !!T ReadUnaligned<T>(void* source) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 1
        ldarg.0        
        unaligned. 0x1
        ldobj !!T
        ret
  } // end of method Underlying::ReadUnaligned
  
  .method public hidebysig static !!T ReadUnaligned<T>(uint8& source) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 1
        ldarg.0        
        unaligned. 0x1
        ldobj !!T
        ret
  } // end of method Underlying::ReadUnaligned
  
  .method public hidebysig static void Write<T>(void* destination, !!T 'value') cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.0
        ldarg.1
        stobj !!T
        ret
  } // end of method Underlying::Write

  .method public hidebysig static void WriteUnaligned<T>(void* destination, !!T 'value') cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.0
        ldarg.1
        unaligned. 0x01
        stobj !!T
        ret
  } // end of method Underlying::WriteUnaligned

  .method public hidebysig static void WriteUnaligned<T>(uint8& destination, !!T 'value') cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.0
        ldarg.1        
        unaligned. 0x01
        stobj !!T
        ret
  } // end of method Underlying::WriteUnaligned

  .method public hidebysig static void Copy<T>(void* destination, !!T& source) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.0
        ldarg.1
        ldobj !!T
        stobj !!T
        ret
  } // end of method Underlying::Copy

  .method public hidebysig static void Copy<T>(!!T& destination, void* source) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.0
        ldarg.1
        ldobj !!T
        stobj !!T
        ret
  } // end of method Underlying::Copy

  .method public hidebysig static void* AsPointer<T>(!!T& 'value') cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 1
        ldarg.0
        conv.u
        ret
  } // end of method Underlying::AsPointer

  .method public hidebysig static int32 SizeOf<T>() cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 1
        sizeof !!T
        ret
  } // end of method Underlying::SizeOf

  .method public hidebysig static void CopyBlock(void* destination, void* source, uint32 byteCount) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        cpblk
        ret
  } // end of method Underlying::CopyBlock

  .method public hidebysig static void CopyBlock(uint8& destination, uint8& source, uint32 byteCount) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        cpblk
        ret
  } // end of method Underlying::CopyBlock

  .method public hidebysig static void CopyBlockUnaligned(void* destination, void* source, uint32 byteCount) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        unaligned. 0x1
        cpblk
        ret
  } // end of method Underlying::CopyBlockUnaligned

  .method public hidebysig static void CopyBlockUnaligned(uint8& destination, uint8& source, uint32 byteCount) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        unaligned. 0x1
        cpblk
        ret
  } // end of method Underlying::CopyBlockUnaligned

  .method public hidebysig static void InitBlock(void* startAddress, uint8 'value', uint32 byteCount) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        initblk
        ret
  } // end of method Underlying::InitBlock

  .method public hidebysig static void InitBlock(uint8& startAddress, uint8 'value', uint32 byteCount) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        initblk
        ret
  } // end of method Underlying::InitBlock

  .method public hidebysig static void InitBlockUnaligned(void* startAddress, uint8 'value', uint32 byteCount) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        unaligned. 0x1
        initblk
        ret
  } // end of method Underlying::InitBlockUnaligned

  .method public hidebysig static void InitBlockUnaligned(uint8& startAddress, uint8 'value', uint32 byteCount) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        ldarg.2
        unaligned. 0x1
        initblk
        ret
  } // end of method Underlying::InitBlockUnaligned

  .method public hidebysig static !!T As<class T>(object o) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 1
        ldarg.0
        ret
  } // end of method Underlying::As

  .method public hidebysig static !!T& AsRef<T>(void* source) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
#ifdef netcoreapp
        .maxstack 1
        ldarg.0
        ret
#else
        .locals (int32&)
        .maxstack 1
        ldarg.0
        stloc.0
        ldloc.0
        ret
#endif
  } // end of method Underlying::AsRef

  .method public hidebysig static native int& GetMethodTablePointer(object o) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 1
        ldarg.0
        conv.i
        ret
  } // end of method Underlying::GetMethodTablePointer

  .method public hidebysig static !!T& AsRef<T>(!!T& source) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .param [1] .custom instance void IS_READ_ONLY_ATTRIBUTE::.ctor() = ( 01 00 00 00 )

        .maxstack 1
        ldarg.0
        ret
  } // end of method Underlying::AsRef

  .method public hidebysig static !!TTo& As<TFrom, TTo>(!!TFrom& source) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )

        .maxstack 1
        ldarg.0
        ret
  } // end of method Underlying::As

  .method public hidebysig static !!T& Unbox<valuetype .ctor ([CORE_ASSEMBLY]System.ValueType) T> (object 'box') cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 1
        ldarg.0
        unbox !!T
        ret
  } // end of method Underlying::Unbox

  .method public hidebysig static !!T& Add<T>(!!T& source, int32 elementOffset) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        sizeof !!T
        conv.i
        mul
        add
        ret
  } // end of method Underlying::Add

  .method public hidebysig static void* Add<T>(void* source, int32 elementOffset) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        sizeof !!T
        conv.i
        mul
        add
        ret
  } // end of method Underlying::Add

  .method public hidebysig static !!T& Add<T>(!!T& source, native int elementOffset) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        sizeof !!T
        mul
        add
        ret
  } // end of method Underlying::Add

  .method public hidebysig static !!T& AddByteOffset<T>(!!T& source, native int byteOffset) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.0
        ldarg.1
        add
        ret
  } // end of method Underlying::AddByteOffset

  .method public hidebysig static !!T& AddByteOffset<T>(!!T& source, int32 byteOffset) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.0
        ldarg.1
        add
        ret
  } // end of method Underlying::AddByteOffset

  .method public hidebysig static !!T& Subtract<T>(!!T& source, int32 elementOffset) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        sizeof !!T
        conv.i
        mul
        sub
        ret
  } // end of method Underlying::Subtract

  .method public hidebysig static void* Subtract<T>(void* source, int32 elementOffset) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        sizeof !!T
        conv.i
        mul
        sub
        ret
  } // end of method Underlying::Subtract

  .method public hidebysig static !!T& Subtract<T>(!!T& source, native int elementOffset) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 3
        ldarg.0
        ldarg.1
        sizeof !!T
        mul
        sub
        ret
  } // end of method Underlying::Subtract

  .method public hidebysig static !!T& SubtractByteOffset<T>(!!T& source, native int byteOffset) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.0
        ldarg.1
        sub
        ret
  } // end of method Underlying::SubtractByteOffset

  .method public hidebysig static !!T& SubtractByteOffset<T>(!!T& source, int32 byteOffset) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.0
        ldarg.1
        sub
        ret
  } // end of method Underlying::SubtractByteOffset

  .method public hidebysig static native int ByteOffset<T>(!!T& origin, !!T& target) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.1
        ldarg.0
        sub
        ret
  } // end of method Underlying::ByteOffset

  .method public hidebysig static bool AreSame<T>(!!T& left, !!T& right) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.0
        ldarg.1
        ceq
        ret 
  } // end of method Underlying::AreSame

  .method public hidebysig static bool IsAddressGreaterThan<T>(!!T& left, !!T& right) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.0
        ldarg.1
        cgt.un
        ret 
  } // end of method Underlying::IsAddressGreaterThan

  .method public hidebysig static bool IsAddressLessThan<T>(!!T& left, !!T& right) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )
        
        .maxstack 2
        ldarg.0
        ldarg.1
        clt.un
        ret 
  } // end of method Underlying::IsAddressLessThan

  .method public hidebysig static void Call<TArg1, TArg2>(!!TArg1 arg1, !!TArg2 arg2, native int pFunc) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )

        .maxstack  3
        ldarg.0
        ldarg.1
        ldarg.2
        calli      void(!!TArg1, !!TArg2)
        ret
  } // end of method Underlying::Call

  .method public hidebysig static void Call<TArg1>(!!TArg1 arg1, native int pFunc) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )

        .maxstack  2
        ldarg.0
        ldarg.1
        calli      void(!!TArg1)
        ret
  } // end of method Underlying::Call

  .method public hidebysig static void Call(native int pFunc) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )

        .maxstack  1
        ldarg.0
        calli      void()
        ret
  } // end of method Underlying::Call

  .method public hidebysig static void Call<TArg1, TArg2, TArg3>(!!TArg1 arg1, !!TArg2 arg2, !!TArg3 arg3, native int pFunc) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )

        .maxstack  4
        ldarg.0
        ldarg.1
        ldarg.2
        ldarg.3
        calli      void(!!TArg1, !!TArg2, !!TArg3)
        ret
  } // end of method Underlying::Call

  .method public hidebysig static !!TResult Invoke<TArg1, TArg2, TResult>(!!TArg1 arg1, !!TArg2 arg2, native int pFunc) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )

        .maxstack  3
        ldarg.0
        ldarg.1
        ldarg.2
        calli      !!TResult(!!TArg1, !!TArg2)
        ret
  } // end of method Underlying::Invoke

  .method public hidebysig static !!TResult Invoke<TResult>(native int pFunc) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )

        .maxstack  1
        ldarg.0
        calli      !!TResult()
        ret
  } // end of method Underlying::Invoke

  .method public hidebysig static !!TResult Invoke<TArg1, TArg2, TArg3, TResult>(!!TArg1 arg1, !!TArg2 arg2, !!TArg3 arg3, native int pFunc) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )

        .maxstack  4
        ldarg.0
        ldarg.1
        ldarg.2
        ldarg.3
        calli      !!TResult(!!TArg1, !!TArg2, !!TArg3)
        ret
  } // end of method Underlying::Invoke

  .method public hidebysig static !!TResult Invoke<TArg1, TResult>(!!TArg1 arg1, native int pFunc) cil managed aggressiveinlining
  {
        .custom instance void System.Runtime.Versioning.NonVersionableAttribute::.ctor() = ( 01 00 00 00 )

        .maxstack  2
        ldarg.0
        ldarg.1
        calli      !!TResult(!!TArg1)
        ret
  } // end of method Underlying::Invoke



} // end of Swifter.Underlying

.class private auto ansi sealed beforefieldinit System.Runtime.Versioning.NonVersionableAttribute extends [CORE_ASSEMBLY]System.Attribute
{
  .custom instance void [CORE_ASSEMBLY]System.AttributeUsageAttribute::.ctor(valuetype [CORE_ASSEMBLY]System.AttributeTargets) = ( 01 00 6C 00 00 00 02 00 54 02 0D 41 6C 6C 6F 77
                                                                                                                                    4D 75 6C 74 69 70 6C 65 00 54 02 09 49 6E 68 65
                                                                                                                                    72 69 74 65 64 00 )
  .method public hidebysig specialname rtspecialname instance void  .ctor() cil managed
  {
        .maxstack 1
        ldarg.0
        call instance void [CORE_ASSEMBLY]System.Attribute::.ctor()
        ret
  } // end of method NonVersionableAttribute::.ctor

} // end of class System.Runtime.Versioning.NonVersionableAttribute