using System.Reflection.Emit;

namespace Swifter.Tools
{
    /// <summary>
    /// Emit Switch 比较器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHashComparer<T>
    {
        /// <summary>
        /// 获取值的哈希值。
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        int GetHashCode(T val);

        /// <summary>
        /// 比较两个值是否一致。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool Equals(T x, T y);

        /// <summary>
        /// 生成获取值的哈希值的 IL 指令。
        /// </summary>
        /// <param name="ilGen"></param>
        void EmitGetHashCode(ILGenerator ilGen);

        /// <summary>
        /// 生成比较两个值是否一致的 IL 指令。
        /// </summary>
        /// <param name="ilGen"></param>
        void EmitEquals(ILGenerator ilGen);
    }
}