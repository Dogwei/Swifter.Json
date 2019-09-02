using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;



#pragma warning disable 1591

namespace Swifter.RW
{
    /// <summary>
    /// 表示字段格式的读写器特性。
    /// </summary>
    public class RWFormatAttribute : RWFieldAttribute
    {
        /// <summary>
        /// 格式化参数。
        /// </summary>
        public virtual string Format { get; set; }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Type InterfaceType
        {
            get => typeof(Interface<>);
            set { }
        }

        /// <summary>
        /// 初始化字段格式特性。
        /// </summary>
        /// <param name="format">格式化参数</param>
        public RWFormatAttribute(string format)
        {
            Format = format;
        }

        /// <summary>
        /// 获取最匹配字段类型的值读写器和方法。
        /// </summary>
        /// <param name="fieldType">字段类型</param>
        /// <param name="firstArgument">值读写器实例</param>
        /// <param name="readValueMethod">值读取方法</param>
        /// <param name="writeValueMethod">值写入方法</param>
        public override void GetBestMatchInterfaceMethod(Type fieldType, out object firstArgument, out MethodInfo readValueMethod, out MethodInfo writeValueMethod)
        {
            if (typeof(IFormattable).IsAssignableFrom(fieldType))
            {
                var type = typeof(Interface<>).MakeGenericType(fieldType);

                firstArgument = Activator.CreateInstance(type, Format);

                GetBestMatchInterfaceMethod(type, fieldType, out readValueMethod, out writeValueMethod);
            }
            else
            {
                throw new TargetException($"Field type '{fieldType}' does not implement '{typeof(IFormattable)}' interface.");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed class Interface<T> : IValueInterface<T> where T : IFormattable
        {
            readonly string format;

            public Interface(string format)
            {
                this.format = format;
            }

            public T ReadValue(IValueReader valueReader)
            {
                return ValueInterface<T>.ReadValue(valueReader);
            }

            public void WriteValue(IValueWriter valueWriter, T value)
            {
                valueWriter.WriteString(value.ToString(format, CultureInfo.CurrentCulture));
            }
        }
    }
}