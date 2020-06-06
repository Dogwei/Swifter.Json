using Swifter.Data.Sql;

namespace Swifter.Data.MySql
{
    sealed class SqlBuilderFactory : Sql.SqlBuilderFactory
    {
        public override string ProviderName => "MySql.Data.MySqlClient";

        public override Sql.SqlBuilder CreateSqlBuilder()
        {
            return new SqlBuilder();
        }
    }
}