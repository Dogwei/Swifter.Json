using Swifter.Tools;
using System;
using System.Data;

namespace Swifter.Data
{
    class ProviderClasses
    {
        public Type tConnection;
        public Type tCommand;
        public Type tDataAdapter;
        public Type tParameter;
        public Type tParameterCollection;
        public Type tDataReader;
        public Type tTransaction;

        public string tProviderName;

        public ProviderClasses(string tProviderName)
        {
            this.tProviderName = tProviderName;
        }

        public Type GetDynamicProviderFactoryType()
        {
            var tConnection = this.tConnection ?? throw new NotSupportedException($"No Type Implement '{typeof(IDbConnection).FullName}' In Package '{tProviderName}'.");
            var tCommand = this.tCommand ?? throw new NotSupportedException($"No Type Implement '{typeof(IDbCommand).FullName}' In Package '{tProviderName}'.");
            var tDataAdapter = this.tDataAdapter ?? typeof(object);
            var tParameter = this.tParameter ?? throw new NotSupportedException($"No Type Implement '{typeof(IDataParameter).FullName}' In Package '{tProviderName}'.");
            var tParameterCollection = this.tParameterCollection ?? throw new NotSupportedException($"No Type Implement '{typeof(IDataParameterCollection).FullName}' In Package '{tProviderName}'.");
            var tDataReader = this.tDataReader ?? throw new NotSupportedException($"No Type Implement '{typeof(IDataReader).FullName}' In Package '{tProviderName}'.");
            var tTransaction = this.tTransaction ?? throw new NotSupportedException($"No Type Implement '{typeof(IDbTransaction).FullName}' In Package '{tProviderName}'.");

            var type = typeof(ProxyProviderFactory<,,,,,,>);

            type = type.MakeGenericType(tConnection, tCommand, tDataAdapter, tParameter, tParameterCollection, tDataReader, tTransaction);

            return type;
        }
    }
}