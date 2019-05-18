namespace Swifter.RW
{
    /// <summary>
    /// 表示支持针对性接口的值读写器。
    /// </summary>
    public interface ITargetedBind
    {
        /// <summary>
        /// 获取针对目标的 Id。
        /// </summary>
        long Id { get; }
    }
}