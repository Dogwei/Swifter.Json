

using Swifter.Tools;
using System.Collections.Generic;

namespace Swifter.RW
{
    sealed class CreateAuthorizedKeysRWInvoker<TOutput, TValue> : IGenericInvoker
    {
        readonly object DataRW;
        readonly Dictionary<TOutput, TValue> AuthorizedKeys;

        public object AuthorizedKeysRW;

        public CreateAuthorizedKeysRWInvoker(object dataRW, Dictionary<TOutput, TValue> authorizedKeys)
        {
            DataRW = dataRW;
            AuthorizedKeys = authorizedKeys;
        }

        public void Invoke<TKey>()
        {
            AuthorizedKeysRW = new AuthorizedKeysRW<TKey, TOutput, TValue>(DataRW, AuthorizedKeys);
        }
    }
}