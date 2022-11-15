using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;



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
        
        /// <summary>
        /// 此值无效。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Type InterfaceType
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
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
        public override void GetBestMatchInterfaceMethod(Type fieldType, out object? firstArgument, out MethodInfo? readValueMethod, out MethodInfo? writeValueMethod)
        {
            if (typeof(IFormattable).IsAssignableFrom(fieldType))
            {
                var type = typeof(Interface<>).MakeGenericType(fieldType);

                firstArgument = type.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance,
                    Type.DefaultBinder, 
                    new Type[] { typeof(string) }, 
                    null)!.Invoke(new object[] { Format });

                GetBestMatchInterfaceMethod(type, fieldType, out readValueMethod, out writeValueMethod);
            }
            else
            {
                throw new TargetException($"Field type '{fieldType}' does not implement '{typeof(IFormattable)}' interface.");
            }
        }

        /// <summary>
        /// 格式化器值接口。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed class Interface<T> : IValueInterface<T> where T : IFormattable
        {
            readonly string format;

            internal Interface(string format)
            {
                this.format = format;
            }

            /// <summary>
            /// 读取值。
            /// </summary>
            /// <param name="valueReader">值读取器</param>
            /// <returns>返回值</returns>
            [return: MaybeNull]
            public T ReadValue(IValueReader valueReader)
            {
                return ValueInterface<T>.ReadValue(valueReader);
            }

            /// <summary>
            /// 写入格式化后的字符串。
            /// </summary>
            /// <param name="valueWriter">写入器</param>
            /// <param name="value">值</param>
            public void WriteValue(IValueWriter valueWriter, [AllowNull]T value)
            {
                if (value is null)
                {
                    valueWriter.DirectWrite(null);
                }
                else
                {
                    valueWriter.WriteString(value.ToString(format, CultureInfo.CurrentCulture));
                }
            }
        }
    }
}