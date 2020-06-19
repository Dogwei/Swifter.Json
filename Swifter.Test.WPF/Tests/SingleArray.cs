using System;
using System.Data;
using System.Linq;

namespace Swifter.Test.WPF.Tests
{
    public class SingleArray : BaseTest<float[]>
    {
        public override float[] GetObject()
        {
            return Enumerable.Range(0, 1630).Select(i => (float)Math.Pow(i, 12)).ToArray();
        }
    }

}