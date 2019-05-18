namespace Swifter.RW
{
    /// <summary>
    /// 对象读写器的字段可访问性。
    /// </summary>
    public enum RWFieldAccess : byte
    {
        /// <summary>
        /// 表示此字段允许读写。
        /// </summary>
        RW = ReadOnly | WriteOnly,
        /// <summary>
        /// 表示忽略此字段。
        /// </summary>
        Ignore = 0,
        /// <summary>
        /// 表示此字段只能读。
        /// </summary>
        ReadOnly = 1,
        /// <summary>
        /// 表示此字段只能写。
        /// </summary>
        WriteOnly = 2
    }
}