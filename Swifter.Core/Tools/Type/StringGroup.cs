using System;
using System.Collections.Generic;

namespace Swifter.Tools
{
    internal class StringGroup<T>
    {
        public StringGroup(Func<T, string> func)
        {
            GetString = func;
        }

        public int SortToken { get; protected set; }

        public Func<T, string> GetString { get; protected set; }

        public int GetDepth()
        {
            if (this is StringSingleGroup<T>)
            {
                return 0;
            }

            int max = 0;

            if (this is StringDifferenceGroup<T> sdg)
            {
                foreach (var item in sdg.Groups)
                {
                    max = Math.Max(max, item.Value.GetDepth());
                }
            }
            else if (this is StringLengthGroup<T> stg)
            {
                foreach (var item in stg.Groups)
                {
                    max = Math.Max(max, item.Value.GetDepth());
                }
            }

            return max + 1;
        }
    }

    internal class StringLengthGroup<T> : StringGroup<T>
    {
        public List<KeyValuePair<int, StringGroup<T>>> Groups { get; private set; }

        public StringLengthGroup(T[] strs, Func<T, string> func) : base(func)
        {
            SortToken = strs.Length;

            var groups = new Dictionary<int, List<T>>();

            foreach (var item in strs)
            {
                if (!groups.TryGetValue(GetString(item).Length, out var list))
                {
                    groups[GetString(item).Length] = list = new List<T>();
                }

                list.Add(item);
            }

            Groups = new List<KeyValuePair<int, StringGroup<T>>>();

            foreach (var item in groups)
            {
                if (item.Value.Count == 1)
                {
                    Groups.Add(new KeyValuePair<int, StringGroup<T>>(item.Key, new StringSingleGroup<T>(item.Value[0], func)));
                }
                else
                {
                    Groups.Add(new KeyValuePair<int, StringGroup<T>>(item.Key, new StringDifferenceGroup<T>(item.Value, func)));
                }
            }

            Groups.Sort((x, y) => x.Key.CompareTo(y.Key));

            GC.Collect();
        }
    }

    internal class StringDifferenceGroup<T> : StringGroup<T>
    {
        public List<KeyValuePair<char, StringGroup<T>>> Groups { get; private set; }

        public int Index { get; set; }

        public StringDifferenceGroup(List<T> strs, Func<T, string> func)
            : base(func)
        {
            SortToken = strs.Count;

            var length = func(strs[0]).Length;

            Dictionary<char, List<T>> maxGroups = null;

            for (int i = 0; i < length; i++)
            {
                var groups = GroupBy(strs, i);

                if (maxGroups == null || groups.Count > maxGroups.Count)
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

            {
                Groups = new List<KeyValuePair<char, StringGroup<T>>>();

                foreach (var item in maxGroups)
                {
                    if (item.Value.Count == 1)
                    {
                        Groups.Add(new KeyValuePair<char, StringGroup<T>>(item.Key, new StringSingleGroup<T>(item.Value[0], func)));
                    }
                    else
                    {
                        Groups.Add(new KeyValuePair<char, StringGroup<T>>(item.Key, new StringDifferenceGroup<T>(item.Value, func)));
                    }
                }

                Groups.Sort((x, y) => y.Value.SortToken.CompareTo(x.Value.SortToken));
            }
        }

        public Dictionary<char, List<T>> GroupBy(List<T> strs, int index)
        {
            var groups = new Dictionary<char, List<T>>();

            foreach (var item in strs)
            {
                if (!groups.TryGetValue(GetString(item)[index], out var list))
                {
                    groups[GetString(item)[index]] = list = new List<T>();
                }

                list.Add(item);
            }

            return groups;
        }
    }

    internal class StringSingleGroup<T> : StringGroup<T>
    {
        public StringSingleGroup(T value, Func<T, string> func)
            : base(func)
        {
            SortToken = 1;

            Value = value;
        }

        public T Value { get; private set; }
    }
}