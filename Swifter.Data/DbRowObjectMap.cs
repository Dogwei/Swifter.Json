using Swifter.RW;
using Swifter.Tools;
using System;

namespace Swifter.Data
{
    sealed class DbRowObjectMap : Cache<string, ValueInterface>, IEquatable<DbRowObjectMap>
    {
        public DbRowsFlags flags;

        public bool Equals(DbRowObjectMap other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Count != Count)
            {
                return false;
            }

            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].Key != other[i].Key)
                {
                    return false;
                }

                if (this[i].Value != other[i].Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}