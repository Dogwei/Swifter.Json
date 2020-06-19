using System;
using System.Data;
using System.Linq;

namespace Swifter.Test.WPF.Tests
{
    public class DoubleArray : BaseTest<double[]>
    {
        public override double[] GetObject()
        {
            return Enumerable.Range(0, 99999).Select(i => Math.Pow(i, 12)).ToArray();
        }
    }

}