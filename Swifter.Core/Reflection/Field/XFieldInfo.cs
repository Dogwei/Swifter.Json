using System;
using System.Reflection;

namespace Swifter.Reflection
{
    /// <summary>
    /// XFieldInfo 字段信息。
    /// 此 XFieldInfo 的提供读写方法比 .Net 自带的 FieldInfo 快很多。
    /// </summary>
    public abstract class XFieldInfo
    {
        /// <summary>
        /// 创建 XFieldInfo 字段信息。
        /// </summary>
        /// <param name="fieldInfo">.Net 自带的 FieldInfo 字段信息。</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回一个 XFieldInfo 字段信息。</returns>
        public static XFieldInfo Create(FieldInfo fieldInfo, XBindingFlags flags)
        {
            if (fieldInfo == null)
            {
                throw new ArgumentNullException(nameof(fieldInfo));
            }

            var declaringType = fieldInfo.DeclaringType;
            var fieldType = fieldInfo.FieldType;

            Type targetType;

            if (fieldType.IsByRef || fieldType.IsByRefLike())
            {
                targetType = typeof(XDefaultFieldInfo);
            }
            else if (fieldInfo.IsLiteral)
            {
                targetType = typeof(XLiteralFieldInfo<>);
            }
            else if (fieldInfo.IsStatic && VersionDifferences.IsSupportEmit)
            {
                targetType = typeof(XStaticFieldInfo<>);
            }
            else if (declaringType.IsValueType && VersionDifferences.IsSupportEmit)
            {
                targetType = typeof(XStructFieldInfo<>);
            }
            else if (VersionDifferences.IsSupportEmit)
            {
                targetType = typeof(XClassFieldInfo<>);
            }
            else
            {
                targetType = typeof(XDefaultFieldInfo);
            }

            if (fieldType.IsPointer)
            {
                fieldType = typeof(IntPtr);
            }

            if (targetType.IsGenericTypeDefinition)
            {
                targetType = targetType.MakeGenericType(fieldType);
            }

            var result = (XFieldInfo)Activator.CreateInstance(targetType);

            result.Initialize(fieldInfo, flags);

            return result;
        }

        internal string name;
        internal XBindingFlags flags;
        internal FieldInfo fieldInfo;

        internal virtual void Initialize(FieldInfo fieldInfo, XBindingFlags flags)
        {
            this.fieldInfo = fieldInfo;
            this.flags = flags;

            name = fieldInfo.Name;
        }

        /// <summary>
        /// 获取 .Net 自带的 FieldInfo 字段信息。
        /// </summary>
        public FieldInfo FieldInfo => fieldInfo;

        /// <summary>
        /// 获取此字段的名称。
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 获取该实例字段的值。
        /// </summary>
        /// <param name="obj">类型的实例</param>
        /// <returns>返回该字段的值</returns>
        public virtual object GetValue(object obj) => throw new InvalidOperationException("Is not an instance field.");

        /// <summary>
        /// 设置该实例字段的值。
        /// </summary>
        /// <param name="obj">类型的实例</param>
        /// <param name="value">该字段的值</param>
        public virtual void SetValue(object obj, object value) => throw new InvalidOperationException("Is not an instance field.");

        /// <summary>
        /// 获取该静态字段的值。
        /// </summary>
        /// <returns>返回该字段的值</returns>
        public virtual object GetValue() => throw new InvalidOperationException("Is not an static field.");

        /// <summary>
        /// 设置该静态字段的值。
        /// </summary>
        /// <param name="value">返回该字段的值</param>
        public virtual void SetValue(object value) => throw new InvalidOperationException("Is not an static field.");
    }
}