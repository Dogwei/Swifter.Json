namespace Swifter.Data.SqlServer
{
    sealed class SqlBuilderFactory : Sql.SqlBuilderFactory
    {
        public override string ProviderName => "System.Data.SqlClient";

        public override Sql.SqlBuilder CreateSqlBuilder()
        {
            return new SqlBuilder();
        }
    }
}