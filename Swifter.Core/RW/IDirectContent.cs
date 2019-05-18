namespace Swifter.RW
{
    /// <summary>
    /// 表示支持直接获取或设置数据源的数据读写器
    /// </summary>
    public interface IDirectContent
    {
        /// <summary>
        /// 直接获取或设置数据源
        /// </summary>
        object DirectContent { get; set; }
    }
}
