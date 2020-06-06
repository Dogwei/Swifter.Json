#define CORE_ASSEMBLY "mscorlib"
#define IS_READ_ONLY_ATTRIBUTE "System.Runtime.CompilerServices.IsReadOnlyAttribute"

// Metadata version: v2.0.50727
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 2:0:0:0
}

.class private auto ansi sealed beforefieldinit System.Runtime.CompilerServices.IsReadOnlyAttribute extends [CORE_ASSEMBLY]System.Attribute
{
  .method public hidebysig specialname rtspecialname instance void .ctor () cil managed 
  {
        .maxstack 1
        ldarg.0
        call instance void [CORE_ASSEMBLY]System.Attribute::.ctor()
        ret
  } // end of method IsReadOnlyAttribute::.ctor

} // end of class System.Runtime.CompilerServices.IsReadOnlyAttribute


#include "../Swifter.Underlying.il"