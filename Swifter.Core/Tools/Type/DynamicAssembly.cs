// #define Save



using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    /// <summary>
    /// Swifter 内部动态程序集。
    /// </summary>
    internal static class DynamicAssembly
    {
        private static readonly AssemblyBuilder AssBuilder;
        private static readonly ModuleBuilder ModBuilder;


#if NETFRAMEWORK && Save
        private const AssemblyBuilderAccess Access = AssemblyBuilderAccess.RunAndSave;
        private const string AssName = "Swifter.DynamicAssembles";
        private const string ModName = "Swifter.DynamicAssembles.dll";


        static DynamicAssembly()
        {
            AssBuilder =
                VersionDifferences.DefineDynamicAssembly(
                    new AssemblyName(AssName),
                    Access);

            ModBuilder = AssBuilder.DefineDynamicModule(
                AssName, ModName);
        }

        public static void Save()
        {
            AssBuilder.Save(ModName);
        }
#else

        private const AssemblyBuilderAccess Access = AssemblyBuilderAccess.Run;
        private const string AssName = "Swifter.DynamicAssembles";

        static DynamicAssembly()
        {
            AssBuilder =
                VersionDifferences.DefineDynamicAssembly(
                    new AssemblyName(AssName),
                    Access);

            ModBuilder = AssBuilder.DefineDynamicModule(
                AssName);
        }
#endif


        public static TypeBuilder DefineType(string name, TypeAttributes attributes, Type baseType = null)
        {
            return ModBuilder.DefineType(name, attributes, baseType);
        }
    }
}