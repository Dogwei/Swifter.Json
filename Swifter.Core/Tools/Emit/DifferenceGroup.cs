using System;
using System.Collections.Generic;
using System.Linq;

namespace Swifter.Tools
{
    internal class DifferenceGroup<T> : BaseGroup<T>
    {
        public readonly List<(int chr, BaseGroup<T> group)> Groups;

        public readonly int Index;

        public DifferenceGroup(List<CaseInfo<T>> strs, IDifferenceComparer<T> comparer)
            : base(comparer, strs.Count)
        {
            var length = Comparer.GetLength(strs[0].Value);

            Dictionary<int, List<CaseInfo<T>>> maxGroups = null;

            for (int i = 0; i < length; i++)
            {
                var groups = GroupBy(strs, i);

                if (maxGroups is null || groups.Count > maxGroups.Count)
                {
                    Index = i;
                    maxGroups = groups;
                }

                if (groups.Count == strs.Count)
                {
                    break;
                }
            }

            if (maxGroups.Count == 1)
            {
                throw new ArgumentException("Duplicate keys.");
            }

            Groups = new List<(int, BaseGroup<T>)>();

            foreach (var item in maxGroups)
            {
                var group = item.Value.Count == 1 ? (BaseGroup<T>)new SingleGroup<T>(item.Value[0], comparer) : new DifferenceGroup<T>(item.Value, comparer);

                Groups.Add((item.Key, group));
            }

            Groups.Sort((x, y) => y.group.SortToken.CompareTo(x.group.SortToken));
        }

        public Dictionary<int, List<CaseInfo<T>>> GroupBy(List<CaseInfo<T>> strs, int index)
        {
            var groups = new Dictionary<int, List<CaseInfo<T>>>();

            foreach (var item in strs)
            {
                if (!groups.TryGetValue(Comparer.ElementAt(item.Value, index), out var list))
                {
                    groups[Comparer.ElementAt(item.Value, index)] = list = new List<CaseInfo<T>>();
                }

                list.Add(item);
            }

            return groups;
        }

        public override int GetDepth() => Groups.Max(item => item.group.GetDepth()) + 1;
    }
}