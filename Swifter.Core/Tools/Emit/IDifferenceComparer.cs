using System.Reflection.Emit;

namespace Swifter.Tools
{
    /// <summary>
    /// 差异位比较器。
    /// </summary>
    /// <typeparam name="T">指定类型</typeparam>
    public interface IDifferenceComparer<T>:IHashComparer<T>
    {
        /// <summary>
        /// 获取类型实例的长度。
        /// </summary>
        /// <param name="value">类型实例</param>
        /// <returns>返回一个长度</returns>
        int GetLength(T value);

        /// <summary>
        /// 获取类型实例指定索引处的 Int32 值。
        /// </summary>
        /// <param name="value">类型实例</param>
        /// <param name="index">指定索引</param>
        /// <returns>返回一个 Int32 值</returns>
        int ElementAt(T value, int index);

        /// <summary>
        /// 生成 获取类型实例的长度 的 IL 代码。
        /// </summary>
        /// <param name="ilGen">IL 生成器</param>
        void EmitGetLength(ILGenerator ilGen);

        /// <summary>
        /// 生成 获取类型实例指定索引处值 的 IL 代码。
        /// </summary>
        /// <param name="ilGen">IL 生成器</param>
        void EmitElementAt(ILGenerator ilGen);
    }
}