using System;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    sealed class TargetPathInfo : IEquatable<TargetPathInfo>
    {
        /* Name Model */
        public string Name;

        /* Number Model */
        public int Index;

        public bool IsFinish;

        public TargetPathInfo Parent;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public TargetPathInfo(string Name, TargetPathInfo Parent)
        {
            this.Name = Name;
            this.Parent = Parent;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public TargetPathInfo(int Index, TargetPathInfo Parent)
        {
            this.Index = Index;
            this.Parent = Parent;
        }

        public bool IsRoot => Name == "#" && Parent == null;

        public bool Equals(TargetPathInfo other)
        {
            return other != null && Name == other.Name && Index == other.Index && (Parent == null ? other.Parent == null : Parent.Equals(other.Parent));
        }

        public override string ToString() => (Parent == null ? "" : (Parent + "/")) + (Name ?? Index.ToString());
    }
}