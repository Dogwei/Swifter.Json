#define CORE_ASSEMBLY "System.Runtime"
#define IS_READ_ONLY_ATTRIBUTE "System.Runtime.CompilerServices.IsReadOnlyAttribute"


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