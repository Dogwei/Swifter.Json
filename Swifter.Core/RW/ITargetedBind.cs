namespace Swifter.RW
{
    /// <summary>
    /// 表示支持针对性接口的对象。
    /// </summary>
    public interface ITargetedBind
    {
        /// <summary>
        /// 获取针对目标的 Id。
        /// </summary>
        long TargetedId { get; }

        /// <summary>
        /// 分配针对目标的 Id。
        /// </summary>
        void MakeTargetedId();
    }
}