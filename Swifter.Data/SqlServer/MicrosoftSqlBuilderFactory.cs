namespace Swifter.Data.SqlServer
{
    sealed class MicrosoftSqlBuilderFactory : Sql.SqlBuilderFactory
    {
        public override string ProviderName => "Microsoft.Data.SqlClient";

        public override Sql.SqlBuilder CreateSqlBuilder()
        {
            return new SqlBuilder();
        }
    }
}