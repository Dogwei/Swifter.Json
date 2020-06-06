#define Save



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

        /// <summary>
        /// 保存程序集。
        /// </summary>
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

        /// <summary>
        /// 定义一个动态类型。
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <param name="attributes">类型属性</param>
        /// <param name="callback">类型生成器回调</param>
        /// <returns>返回一个运行时类型</returns>
        public static Type DefineType(string name, TypeAttributes attributes, Action<TypeBuilder> callback)
        {
            return DefineType(name, attributes, null, callback);
        }

        /// <summary>
        /// 定义一个动态类型。
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <param name="attributes">类型属性</param>
        /// <param name="baseType">基类</param>
        /// <param name="callback">类型生成器回调</param>
        /// <returns>返回一个运行时类型</returns>
        public static Type DefineType(string name, TypeAttributes attributes, Type baseType, Action<TypeBuilder> callback)
        {
            var typeBuilder = DefineType(name, attributes, baseType);

            callback(typeBuilder);

            return typeBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// 定义一个动态方法。
        /// </summary>
        /// <param name="typeBuilder">类型生成器</param>
        /// <param name="name">方法名</param>
        /// <param name="attributes">方法标识</param>
        /// <param name="callingConvention">调用约定</param>
        /// <param name="returnType">返回值类型</param>
        /// <param name="parametersTypes">参数类型集合</param>
        /// <param name="callback">方法生成器回调</param>
        /// <returns>返回当前类型生成器</returns>
        public static TypeBuilder DefineMethod(this TypeBuilder typeBuilder,
            string name,
            MethodAttributes attributes,
            CallingConventions callingConvention,
            Type returnType,
            Type[] parametersTypes,
            Action<MethodBuilder, ILGenerator> callback)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                name,
                attributes,
                callingConvention,
                returnType,
                parametersTypes);

            callback(methodBuilder, methodBuilder.GetILGenerator());

            return typeBuilder;
        }

        /// <summary>
        /// 定义一个动态方法。
        /// </summary>
        /// <typeparam name="TDelegate">委托类型</typeparam>
        /// <param name="callback">方法生成器回调</param>
        /// <param name="restrictedSkipVisibility">是否跳过验证</param>
        /// <returns>返回一个委托</returns>
        public static TDelegate DefineDynamicMethod<TDelegate>(Action<DynamicMethod, ILGenerator> callback, bool restrictedSkipVisibility = false) where TDelegate : Delegate
        {
            MethodHelper.GetParametersTypes(typeof(TDelegate), out var parameterTypes, out var returnType);

            var dynamicMethod = new DynamicMethod(
                $"{nameof(DynamicAssembly)}_{nameof(DynamicMethod)}_{Guid.NewGuid().ToString("N")}",
                returnType,
                parameterTypes,
                restrictedSkipVisibility
                );

            callback(dynamicMethod, dynamicMethod.GetILGenerator());

            return MethodHelper.CreateDelegate<TDelegate>(dynamicMethod);
        }

        /// <summary>
        /// 定义一个动态方法。
        /// </summary>
        /// <typeparam name="TDelegate">委托类型</typeparam>
        /// <param name="callback">方法生成器回调</param>
        /// <param name="module">绑定模块</param>
        /// <param name="skipVisibility">是否跳过验证</param>
        /// <returns>返回一个委托</returns>
        public static TDelegate DefineDynamicMethod<TDelegate>(Action<DynamicMethod, ILGenerator> callback, Module module, bool skipVisibility = false) where TDelegate : Delegate
        {
            MethodHelper.GetParametersTypes(typeof(TDelegate), out var parameterTypes, out var returnType);

            var dynamicMethod = new DynamicMethod(
                $"{nameof(DynamicAssembly)}_{nameof(DynamicMethod)}_{Guid.NewGuid().ToString("N")}",
                returnType,
                parameterTypes,
                module,
                skipVisibility
                );

            callback(dynamicMethod, dynamicMethod.GetILGenerator());

            return MethodHelper.CreateDelegate<TDelegate>(dynamicMethod);
        }

        /// <summary>
        /// 获取已定义的动态类型。不存在则返回 Null。
        /// </summary>
        /// <param name="typeName">动态类型名称</param>
        /// <returns>返回一个类型</returns>
        public static Type GetType(string typeName)
        {
            return AssBuilder.GetType(typeName);
        }
    }
}