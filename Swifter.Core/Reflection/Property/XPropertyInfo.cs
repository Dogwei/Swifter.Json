using Swifter.Tools;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using static Swifter.Reflection.ThrowHelpers;

namespace Swifter.Reflection
{
    /// <summary>
    /// XPropertyInfo 属性信息。
    /// 此属性信息提供的读写方法比 .Net 自带的 PropertyInfo 属性信息快很多。
    /// </summary>
    public abstract class XPropertyInfo
    {
        /// <summary>
        /// 创建 XPropertyInfo 属性信息。
        /// </summary>
        /// <param name="propertyInfo">.Net 自带的 PropertyInfo 属性信息</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回 XPropertyInfo 属性信息。</returns>
        public static XPropertyInfo Create(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            if (propertyInfo is null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            var declaringType = propertyInfo.DeclaringType;

            var propertyType
                = propertyInfo.PropertyType.IsPointer ? typeof(IntPtr)
                : propertyInfo.PropertyType.IsByRef ? propertyInfo.PropertyType.GetElementType()
                : propertyInfo.PropertyType;

            XPropertyInfo result;

            try
            {
                var targetType
                    = propertyType.IsByRefLike() ? typeof(XDefaultPropertyInfo)
                     : propertyInfo.IsStatic() ? typeof(XStaticPropertyInfo<>).MakeGenericType(propertyType)
                     : declaringType.IsValueType ? typeof(XStructPropertyInfo<,>).MakeGenericType(declaringType, propertyType)
                    : typeof(XClassPropertyInfo<,>).MakeGenericType(declaringType, propertyType);

                result = (XPropertyInfo)Activator.CreateInstance(targetType, true);

                result.Initialize(propertyInfo, flags);

            }
            catch

#if DEBUG
            (Exception e)
#endif
            {
#if DEBUG
                if (e is TargetInvocationException tie)
                {
                    e = tie.InnerException;
                }

                Console.WriteLine($"{nameof(XPropertyInfo)} : Exception : {e.GetType()} -- {e.Message}");
#endif

                result = new XDefaultPropertyInfo();

                result.Initialize(propertyInfo, flags);
            }

            return result;
        }

        internal string name;
        internal XBindingFlags flags;
        internal PropertyInfo propertyInfo;

        internal void Initialize(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            if (propertyInfo.PropertyType.IsByRef)
            {
                InitializeByRef(propertyInfo, flags);
            }
            else
            {
                InitializeByValue(propertyInfo, flags);
            }
        }

        private protected virtual void InitializeByValue(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            this.propertyInfo = propertyInfo;

            name = propertyInfo.Name;
            this.flags = flags;
        }

        private protected virtual void InitializeByRef(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            this.propertyInfo = propertyInfo;

            name = propertyInfo.Name;
            this.flags = flags;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void Assert(bool err, string name)
        {
            if (!err)
            {
                Throw();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            void Throw()
            {
                throw new MissingMethodException($"Property '{PropertyInfo.DeclaringType.Name}.{PropertyInfo.Name}' No define '{name}' method or cannot access.");
            }
        }

        /// <summary>
        /// 获取 .Net 自带的 PropertyInfo 属性信息。
        /// </summary>
        public PropertyInfo PropertyInfo => propertyInfo;

        /// <summary>
        /// 获取此属性的名称。
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 获取该实例属性的值。
        /// </summary>
        /// <param name="obj">类型的实例</param>
        /// <returns>返回该属性的值</returns>
        public virtual object GetValue(object obj)
        {
            ThrowInvalidOperationException("property", "instance");

            return default;
        }

        /// <summary>
        /// 设置该实例属性的值。
        /// </summary>
        /// <param name="obj">类型的实例</param>
        /// <param name="value">值</param>
        public virtual void SetValue(object obj, object value)
        {
            ThrowInvalidOperationException("property", "instance");
        }

        /// <summary>
        /// 获取该静态属性的值。
        /// </summary>
        /// <returns>返回该属性的值</returns>
        public virtual object GetValue()
        {
            ThrowInvalidOperationException("property", "instance");

            return default;
        }

        /// <summary>
        /// 设置该静态属性的值。
        /// </summary>
        /// <param name="value">值</param>
        public virtual void SetValue(object value)
        {
            ThrowInvalidOperationException("property", "static");
        }
    }
}