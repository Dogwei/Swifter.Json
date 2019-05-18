namespace Swifter.Readers
{
    /// <summary>
    /// 表格数据读取器。
    /// </summary>
    public interface ITableReader : IDataReader<string>, IDataReader<int>
    {
        /// <summary>
        /// 开始读取下一行数据，开始为没有行。
        /// </summary>
        /// <returns>返回是否存在下一行数据。</returns>
        bool Read();
    }
}