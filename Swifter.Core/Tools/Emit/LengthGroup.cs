using System.Collections.Generic;
using System.Linq;

namespace Swifter.Tools
{
    internal class LengthGroup<T> : BaseGroup<T>
    {
        public readonly List<(int length, BaseGroup<T> group)> Groups;

        public LengthGroup(CaseInfo<T>[] strs, IDifferenceComparer<T> comparer) : base(comparer, strs.Length)
        {
            var groups = new Dictionary<int, List<CaseInfo<T>>>();

            foreach (var item in strs)
            {
                if (!groups.TryGetValue(Comparer.GetLength(item.Value), out var list))
                {
                    groups[Comparer.GetLength(item.Value)] = list = new List<CaseInfo<T>>();
                }

                list.Add(item);
            }

            Groups = new List<(int, BaseGroup<T>)>();

            foreach (var item in groups)
            {
                var group = item.Value.Count == 1 ? (BaseGroup<T>)new SingleGroup<T>(item.Value[0], comparer) : new DifferenceGroup<T>(item.Value, comparer);

                Groups.Add((item.Key, group));
            }

            Groups.Sort((x, y) => x.length.CompareTo(y.length));
        }

        public override int GetDepth() => Groups.Max(item => item.group.GetDepth()) + 1;
    }
}