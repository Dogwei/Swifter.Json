using Swifter.RW;
using System.Runtime.InteropServices;

using static Swifter.Reflection.SerializationBox;

namespace Swifter.Reflection
{
    /// <summary>
    /// 对象序列化盒子。
    /// </summary>
    /// <typeparam name="T">序列化的声明类型</typeparam>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct SerializationBox<T>
    {
        /// <summary>
        /// 需要序列化的对象。
        /// </summary>
        public readonly T Value;

        sealed class ValueInterface : IValueInterface<SerializationBox<T>>
        {
            public SerializationBox<T> ReadValue(IValueReader valueReader)
            {
                return (T)Read(valueReader, typeof(T));
            }

            public void WriteValue(IValueWriter valueWriter, SerializationBox<T> value)
            {
                Write(valueWriter, value.Value, typeof(T));
            }
        }

        /// <summary>
        /// 将对象隐式转换为对象序列化盒子。
        /// </summary>
        /// <param name="value">对象</param>
        public static implicit operator SerializationBox<T>(T value)
        {
            return Underlying.As<T, SerializationBox<T>>(ref value);
        }

        /// <summary>
        /// 将对象序列化盒子隐式转换为对象。
        /// </summary>
        /// <param name="value">对象序列化盒子</param>
        public static implicit operator T(SerializationBox<T> value)
        {
            return value.Value;
        }
    }
}
