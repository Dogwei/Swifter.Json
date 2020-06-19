using System.Data;
using System.Linq;

namespace Swifter.Test.WPF.Tests
{
    public class Int64Array : BaseTest<long[]>
    {
        public override long[] GetObject()
        {
            return Enumerable.Range(9999, 99999).Select(i=>(long)i).ToArray();
        }
    }

}