using System.Diagnostics;

namespace Swifter.Test.WPF
{
    public static class StopwatchExtensions
    {
        public static double ElapsedNanoseconds(this Stopwatch stopwatch)
        {
            return stopwatch.ElapsedTicks * 100.0;
        }
    }
}
