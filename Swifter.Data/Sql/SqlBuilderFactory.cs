namespace Swifter.Data.Sql
{
    /// <summary>
    /// Sql 生成器工厂类。
    /// </summary>
    public abstract class SqlBuilderFactory
    {
        /// <summary>
        /// 获取供应商名称
        /// </summary>
        public abstract string ProviderName { get; }

        /// <summary>
        /// 创建 Sql 生成器。
        /// </summary>
        /// <returns></returns>
        public abstract SqlBuilder CreateSqlBuilder();
    }
}