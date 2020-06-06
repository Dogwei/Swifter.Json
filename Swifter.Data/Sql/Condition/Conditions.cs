using System.Collections.Generic;

namespace Swifter.Data.Sql
{
    /// <summary>
    /// 条件集合
    /// </summary>
    public sealed class Conditions : List<Condition>
    {
        /// <summary>
        /// 添加一个条件，并排序。
        /// </summary>
        /// <param name="condition"></param>
        public new void Add(Condition condition)
        {
            base.Add(condition);

            Sort();
        }

        private new void Sort()
        {
            for (int i = 1, j = 0; i < Count; j = i, ++i)
            {
                var swap = this[i];

                while (swap.CompareTo(this[j]) < 0)
                {
                    this[j + 1] = this[j];

                    --j;

                    if (j == -1)
                    {
                        break;
                    }
                }

                this[j + 1] = swap;
            }
        }
    }
}