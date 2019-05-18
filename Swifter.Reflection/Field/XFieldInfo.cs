using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
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

            if (fieldInfo.IsLiteral)
            {
                targetType = typeof(XLiteralFieldInfo<>);
            }
            else if (fieldInfo.IsStatic)
            {
                targetType = typeof(XStaticFieldInfo<>);
            }
            else if (declaringType.IsValueType)
            {
                targetType = typeof(XStructFieldInfo<>);
            }
            else
            {
                targetType = typeof(XClassFieldInfo<>);
            }

            if (fieldType.IsPointer || fieldType.IsByRef)
            {
                fieldType = typeof(IntPtr);
            }

            targetType = targetType.MakeGenericType(fieldType);

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
        /// 获取该字段的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该字段时静态的，则指定为 Null</param>
        /// <returns>返回该字段的值</returns>
        public abstract object GetValue(object obj);

        /// <summary>
        /// 设置该字段的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该字段时静态的，则指定为 Null</param>
        /// <param name="value">该字段的值</param>
        public abstract void SetValue(object obj, object value);

        /// <summary>
        /// 获取该字段的值。
        /// </summary>
        /// <param name="typedRef">类型的实例的引用。如果该字段时静态的，则指定为 Null</param>
        /// <returns>返回该字段的值</returns>
        public abstract object GetValue(TypedReference typedRef);

        /// <summary>
        /// 设置该字段的值。
        /// </summary>
        /// <param name="typedRef">类型的实例的引用。如果该字段时静态的，则指定为 Null</param>
        /// <param name="value">返回该字段的值</param>
        public abstract void SetValue(TypedReference typedRef, object value);
    }
}