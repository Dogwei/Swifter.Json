// #define Save



using System;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    /// <summary>
    /// Swifter 内部动态程序集。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DynamicAssembly
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


        /// <summary>
        /// 定义一个动态类型。
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <param name="attributes">类型属性</param>
        /// <param name="baseType">基类</param>
        /// <returns>返回一个类型生成器</returns>
        public static TypeBuilder DefineType(string name, TypeAttributes attributes, Type baseType = null)
        {
            return ModBuilder.DefineType(name, attributes, baseType);
        }
    }
}