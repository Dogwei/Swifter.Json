namespace Swifter.RW
{
    /// <summary>
    /// DataTable 读写器的配置。
    /// </summary>
    public enum DataTableRWOptions
    {
        /// <summary>
        /// 默认配置项。
        /// </summary>
        None = 0,

        /// <summary>
        /// 设置第一行的数据类型为各个 Column 的类型。否则将设置 Object 为各个 Column 的类型。默认不开启。
        /// </summary>
        SetFirstRowsTypeToColumnTypes = 1,

        /// <summary>
        /// 设置第二行开始的数据写入为数组。
        /// </summary>
        WriteToArrayFromBeginningSecondRows = 2
    }
}