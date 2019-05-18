namespace Swifter.Writers
{
    /// <summary>
    /// 表格数据写入器。
    /// </summary>
    public interface ITableWriter : IDataWriter<string>, IDataWriter<int>
    {
        /// <summary>
        /// 开始写入下一行数据。初始为没有行。
        /// </summary>
        void Next();
    }
}