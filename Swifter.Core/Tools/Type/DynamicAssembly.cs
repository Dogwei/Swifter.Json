
//#if NETFRAMEWORK
//#define Save
//#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using static Swifter.Tools.MethodHelper;


namespace Swifter.Tools
{
    /// <summary>
    /// Swifter 内部动态程序集。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DynamicAssembly
    {
        private const AssemblyBuilderAccess Access = AssemblyBuilderAccess.Run;

        /// <summary>
        /// 获取动态程序集的名称。
        /// </summary>
        public const string AssName = "Swifter.DynamicAssembles";


        private static readonly List<AssemblyBuilder> AssBuilders = new List<AssemblyBuilder>();

        private static readonly Dictionary<Assembly, bool> IgnoresAccessChecksToDic = new Dictionary<Assembly, bool>();
        private static readonly object Lock = new object();


        /// <summary>
        /// 获取动态程序集是否可以访问非公开类型。
        /// </summary>
        public static readonly bool CanAccessNonPublicTypes;

        /// <summary>
        /// 获取动态程序集是否可以访问非公开成员。
        /// </summary>
        public static readonly bool CanAccessNonPublicMembers;

        private class TestClass
        {
            internal static void TestMethod()
            {

            }
        }

        private static AssemblyBuilder AssBuilder;
        private static ModuleBuilder ModBuilder;
        private static bool HaveDefinedType;

        static IEnumerable<Assembly> GetAssemblies()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                yield return assembly;

                foreach (var assemblyName in assembly.GetReferencedAssemblies())
                {
                    Assembly item = null;

                    try
                    {
                        item = Assembly.Load(assemblyName);
                    }
                    catch
                    {
                    }

                    if (!(item is null))
                    {
                        yield return item;
                    }
                }
            }
        }

        static DynamicAssembly()
        {
            lock (Lock)
            {
                try
                {
                    foreach (var assembly in GetAssemblies().Distinct())
                    {
                        IgnoresAccessChecksToDic.Add(assembly, true);
                    }
                }
                catch
                {
                }

                HaveDefinedType = true;
                CanAccessNonPublicTypes = true;
                CanAccessNonPublicMembers = true;

                Reset();

                try
                {
                    DefineType($"{nameof(TestClass)}_{Guid.NewGuid():N}", TypeAttributes.Public, typeof(TestClass), typeBuilder =>
                    {

                    });
                }
                catch
                {
                    CanAccessNonPublicTypes = false;
                }

                try
                {
                    var DynamicMethodName = $"{nameof(TestClass.TestMethod)}_{Guid.NewGuid():N}";

                    var method = DefineType($"{nameof(TestClass)}_{Guid.NewGuid():N}", TypeAttributes.Public, typeBuilder =>
                    {
                        typeBuilder.DefineMethod(
                            DynamicMethodName,
                            MethodAttributes.Public | MethodAttributes.Static,
                            CallingConventions.Standard,
                            typeof(void),
                            Type.EmptyTypes,
                            (methodBuilder, ilGen) =>
                            {
                                ilGen.Call(MethodOf(TestClass.TestMethod));
                                ilGen.Return();
                            });

                    }).GetMethod(DynamicMethodName);

                    method.Invoke(null, null);
                }
                catch
                {
                    CanAccessNonPublicMembers = false;
                }

#if DEBUG

                Console.WriteLine($"{nameof(DynamicAssembly)} : CanAccessNonPublicTypes : {CanAccessNonPublicTypes}");
                Console.WriteLine($"{nameof(DynamicAssembly)} : CanAccessNonPublicMembers : {CanAccessNonPublicMembers}");

#endif
            }

        }

        private static void Reset()
        {
            if (!HaveDefinedType)
            {
                return;
            }

            lock (Lock)
            {
                if (!HaveDefinedType)
                {
                    return;
                }

                AssBuilder =
                    VersionDifferences.DefineDynamicAssembly(
                        new AssemblyName(AssName),
                        Access
#if Save
                        | AssemblyBuilderAccess.Save
#endif
                        );

                AssBuilders.Add(AssBuilder);

                ModBuilder = AssBuilder.DefineDynamicModule(
                    AssName
#if Save
                    , $"{AssName}.dll"
#endif
                    );

                HaveDefinedType = false;

                var assemblies = IgnoresAccessChecksToDic.Keys.ToArray();

                IgnoresAccessChecksToDic.Clear();

                foreach (var assembly in assemblies)
                {
                    IgnoresAccessChecksTo(assembly);
                }
            }
        }

#if Save

        /// <summary>
        /// 保存动态程序集。
        /// </summary>
        public static void Save()
        {
            AssBuilder.Save($"{AssName}.dll");
        }
#endif

        /// <summary>
        /// 忽略对指定程序集的访问检查。
        /// </summary>
        /// <param name="assembly">指定程序集</param>
        public static void IgnoresAccessChecksTo(Assembly assembly)
        {
            if (!CanAccessNonPublicTypes)
            {
                return;
            }

            if (assembly is null)
            {
                return;
            }

            if (IgnoresAccessChecksToDic.ContainsKey(assembly))
            {
                return;
            }

            var assemblyName = GetAssemblyName(assembly);

            if (string.IsNullOrEmpty(assemblyName))
            {
                return;
            }

            lock (Lock)
            {
                if (IgnoresAccessChecksToDic.ContainsKey(assembly))
                {
                    return;
                }

                if (HaveDefinedType)
                {
                    Reset();
                }

                AssBuilder.SetCustomAttribute(
                    new IgnoresAccessChecksToAttribute(assemblyName).ToCustomAttributeBuilder()
                        );

                IgnoresAccessChecksToDic.Add(assembly, true);
            }
        }

        /// <summary>
        /// 获取程序集的程序集名称。
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <returns>返回程序集名称</returns>
        public static string GetAssemblyName(Assembly assembly)
        {
            var name = assembly.GetName();

            if (name is null)
            {
                return null;
            }

            var publicKey = name.GetPublicKey()?.ToHexString();

            return string.IsNullOrEmpty(publicKey) ? name.Name : $"{name.Name}, PublicKey={publicKey}";
        }

        /// <summary>
        /// 定义一个动态类型。
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <param name="attributes">类型属性</param>
        /// <param name="baseType">基类</param>
        /// <returns>返回一个类型生成器</returns>
        public static TypeBuilder DefineType(string name, TypeAttributes attributes, Type baseType = null)
        {
            HaveDefinedType = true;

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
        public static TDelegate BuildDynamicMethod<TDelegate>(Action<DynamicMethod, ILGenerator> callback, bool restrictedSkipVisibility = false) where TDelegate : Delegate
        {
            GetParametersTypes(typeof(TDelegate), out var parameterTypes, out var returnType);

            var dynamicMethod = new DynamicMethod(
                $"{nameof(DynamicAssembly)}_{nameof(DynamicMethod)}_{Guid.NewGuid():N}",
                returnType,
                parameterTypes,
                restrictedSkipVisibility
                );

            callback(dynamicMethod, dynamicMethod.GetILGenerator());

            return CreateDelegate<TDelegate>(dynamicMethod);
        }

        /// <summary>
        /// 定义一个动态方法。
        /// </summary>
        /// <typeparam name="TDelegate">委托类型</typeparam>
        /// <param name="callback">方法生成器回调</param>
        /// <param name="module">绑定模块</param>
        /// <param name="skipVisibility">是否跳过验证</param>
        /// <returns>返回一个委托</returns>
        public static TDelegate BuildDynamicMethod<TDelegate>(Action<DynamicMethod, ILGenerator> callback, Module module, bool skipVisibility = false) where TDelegate : Delegate
        {
            GetParametersTypes(typeof(TDelegate), out var parameterTypes, out var returnType);

            var dynamicMethod = new DynamicMethod(
                $"{nameof(DynamicAssembly)}_{nameof(DynamicMethod)}_{Guid.NewGuid():N}",
                returnType,
                parameterTypes,
                module,
                skipVisibility
                );

            callback(dynamicMethod, dynamicMethod.GetILGenerator());

            return CreateDelegate<TDelegate>(dynamicMethod);
        }

        /// <summary>
        /// 获取已定义的动态类型。不存在则返回 Null。
        /// </summary>
        /// <param name="typeName">动态类型名称</param>
        /// <returns>返回一个类型</returns>
        public static Type GetType(string typeName)
        {
            foreach (var assemblyBuilder in AssBuilders)
            {
                if (assemblyBuilder.GetType(typeName) is Type type)
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// 判断指定程序集是否对动态程序集显示非共有成员。
        /// </summary>
        /// <param name="assembly">指定程序集</param>
        /// <returns>返回一个 <see cref="bool"/> 值。</returns>
        public static bool IsInternalsVisibleTo(Assembly assembly)
        {
            if (assembly is null)
            {
                return true;
            }

            foreach (InternalsVisibleToAttribute attribute in assembly.GetCustomAttributes(typeof(InternalsVisibleToAttribute),false))
            {
                if (attribute.AssemblyName == AssName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断动态程序集是否对指定程序集忽略访问检查。
        /// </summary>
        /// <param name="assembly">指定程序集</param>
        /// <returns>返回一个 <see cref="bool"/> 值。</returns>
        public static bool IsIgnoresAccessChecksTo(Assembly assembly)
        {
            if (assembly is null)
            {
                return true;
            }

            if (CanAccessNonPublicTypes)
            {
                if (IgnoresAccessChecksToDic.ContainsKey(assembly))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 定义自动完成的属性。
        /// </summary>
        /// <param name="typeBuilder">类型生成器</param>
        /// <param name="attributes">属性的属性</param>
        /// <param name="name">属性的名称</param>
        /// <param name="type">属性的类型</param>
        /// <param name="fieldAttributes">字段的属性</param>
        /// <param name="methodAttributes">get 和 set 方法的属性</param>
        /// <returns>返回当前类型生成器</returns>
        public static TypeBuilder DefineAutoProperty(
            this TypeBuilder typeBuilder,
            string name,
            Type type,
            PropertyAttributes attributes = PropertyAttributes.HasDefault,
            FieldAttributes fieldAttributes = FieldAttributes.Private | FieldAttributes.SpecialName,
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName)
        {
            var fieldBuilder = typeBuilder.DefineField(
                $"_{name}_{Guid.NewGuid():N}",
                type,
                fieldAttributes
                );

            return typeBuilder.DefineProperty(name, attributes, type,
                (propertyBuilder, methodBuilder, ilGen) =>
                {
                    ilGen.LoadArgument(0);
                    ilGen.LoadField(fieldBuilder);
                    ilGen.Return();
                }, (propertyBuilder, methodBuilder, ilGen) =>
                {

                    ilGen.LoadArgument(0);
                    ilGen.LoadArgument(1);
                    ilGen.StoreField(fieldBuilder);
                    ilGen.Return();
                }, methodAttributes);
        }

        /// <summary>
        /// 定义属性。
        /// </summary>
        /// <param name="typeBuilder">类型生成器</param>
        /// <param name="name">属性名</param>
        /// <param name="attributes">属性的属性</param>
        /// <param name="type">属性的类型</param>
        /// <param name="getCallback">get 方法的回调</param>
        /// <param name="setCallback">set 方法的回调</param>
        /// <param name="methodAttributes">get 和 set 方法的属性</param>
        /// <returns>返回当前类生成器</returns>
        public static TypeBuilder DefineProperty(
            this TypeBuilder typeBuilder,
            string name,
            PropertyAttributes attributes,
            Type type,
            Action<PropertyBuilder, MethodBuilder, ILGenerator> getCallback,
            Action<PropertyBuilder, MethodBuilder, ILGenerator> setCallback,
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName)
        {
            var propertyBuilder = typeBuilder.DefineProperty(
                name,
                attributes,
                type,
                Type.EmptyTypes);

            if (getCallback != null)
            {
                typeBuilder.DefineMethod(
                    $"{"get"}_{name}_{Guid.NewGuid():N}",
                    methodAttributes,
                    type,
                    Type.EmptyTypes,
                    (methodBuilder, ilGen) =>
                    {
                        getCallback(propertyBuilder, methodBuilder, ilGen);

                        propertyBuilder.SetGetMethod(methodBuilder);
                    });
            }

            if (setCallback != null)
            {
                typeBuilder.DefineMethod(
                    $"{"set"}_{name}_{Guid.NewGuid():N}",
                    methodAttributes,
                    type,
                    Type.EmptyTypes,
                    (methodBuilder, ilGen) =>
                    {
                        setCallback(propertyBuilder, methodBuilder, ilGen);

                        propertyBuilder.SetGetMethod(methodBuilder);
                    });
            }

            return typeBuilder;
        }

        /// <summary>
        /// 定义方法。
        /// </summary>
        /// <param name="typeBuilder">类型生成器</param>
        /// <param name="name">方法名</param>
        /// <param name="attributes">方法的属性</param>
        /// <param name="returnType">返回值类型</param>
        /// <param name="parameterTypes">参数类型集合</param>
        /// <param name="callback">方法生成器回调</param>
        /// <returns>返回当前类型生成器</returns>
        public static TypeBuilder DefineMethod(this TypeBuilder typeBuilder, string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes, Action<MethodBuilder, ILGenerator> callback)
        {
            var methodBuilder = typeBuilder.DefineMethod(name, attributes, returnType, parameterTypes);

            callback(methodBuilder, methodBuilder.GetILGenerator());

            return typeBuilder;
        }

        /// <summary>
        /// 将特性转换为特性生成器。
        /// </summary>
        /// <param name="attribute">特性实例</param>
        /// <returns>返回一个将特性转换为特性生成器</returns>
        public static CustomAttributeBuilder ToCustomAttributeBuilder(this Attribute attribute)
        {
            object[] emptyValues = new object[0];

            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            var type = attribute.GetType();

            bool HaveField(ParameterInfo par)
            {
                if (type.GetProperty(par.Name, bindingFlags) is PropertyInfo propertyInfo && propertyInfo.CanRead)
                {
                    return true;
                }

                if (type.GetField(par.Name, bindingFlags) is FieldInfo fieldInfo)
                {
                    return true;
                }

                return false;
            }

            object GetValue(ParameterInfo par)
            {
                if (type.GetProperty(par.Name, bindingFlags) is PropertyInfo propertyInfo && propertyInfo.CanRead)
                {
                    return propertyInfo.GetValue(attribute, emptyValues);
                }

                if (type.GetField(par.Name, bindingFlags) is FieldInfo fieldInfo)
                {
                    return fieldInfo.GetValue(attribute);
                }

                return null;
            }

            foreach (var constructor in type.GetConstructors())
            {
                if (constructor.GetParameters().All(HaveField))
                {
                    try
                    {
                        var constructorArgs = constructor.GetParameters().Select(GetValue).ToArray();

                        var newObj = constructor.Invoke(constructorArgs);

                        var properties = new Dictionary<PropertyInfo, object>();

                        foreach (var property in type.GetProperties(bindingFlags))
                        {
                            if (property.CanRead && property.CanWrite)
                            {
                                var x = property.GetValue(attribute, emptyValues);
                                var y = property.GetValue(newObj, emptyValues);

                                if (!Equals(x, y))
                                {
                                    properties.Add(property, x);
                                }
                            }
                        }

                        var fields = new Dictionary<FieldInfo, object>();

                        foreach (var field in type.GetFields(bindingFlags))
                        {
                            var x = field.GetValue(attribute);
                            var y = field.GetValue(newObj);

                            if (!Equals(x, y))
                            {
                                fields.Add(field, x);
                            }
                        }

                        return new CustomAttributeBuilder(
                            constructor, constructorArgs,
                            properties.Keys.ToArray(), properties.Values.ToArray(),
                            fields.Keys.ToArray(), fields.Values.ToArray()
                            );

                    }
                    catch
                    {
                    }
                }
            }

            throw new NotSupportedException("unable");
        }
    }
}