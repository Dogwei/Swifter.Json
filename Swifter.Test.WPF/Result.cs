using System;

namespace Swifter.Test.WPF
{
    public class Result
    {
        public readonly double avg;

        public Result(double avg)
        {
            this.avg = avg;
        }

        public override string ToString()
        {
            return $"{Math.Round(avg / 1000, 2)} us";
        }
    }
}