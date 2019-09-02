
using Swifter.RW;
using System;

namespace Swifter.Json
{
    sealed class JsonReference
    {
        internal readonly IDataReader Root;
        internal readonly RWPathInfo Reference;

        internal object value;

        public JsonReference(IDataReader root, RWPathInfo reference)
        {
            Root = root;
            Reference = reference;
        }

        public object Value => this.value ?? (TryGetValue(out var value) ? value : throw new InvalidOperationException("The value is not ready."));

        internal bool TryGetValue(out object value)
        {
            try
            {
                this.value = value = Reference.GetValue(Root);

                return value != null;
            }
            catch (Exception)
            {
                value = null;

                return false;
            }
        }
    }
}