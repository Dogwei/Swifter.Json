using System.Collections.Generic;
using System.Linq;

namespace Swifter.Test.WPF.Tests
{
    public class Int32Array : BaseTest<int[]>
    {
        public override int[] GetObject()
        {
            return Enumerable.Range(1, 9999).ToArray();
        }
    }

    public class Int32Enumerable : BaseTest<IEnumerable<int>>
    {
        public override IEnumerable<int> GetObject()
        {
            return Enumerable.Range(1, 1000);
        }
    }
}