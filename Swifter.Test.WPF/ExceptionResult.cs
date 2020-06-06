using System;

namespace Swifter.Test.WPF
{
    public class ExceptionResult
    {
        public readonly Exception e;

        public ExceptionResult(Exception e)
        {
            this.e = e;
        }

        public override string ToString()
        {
            if (e is IncorrectException)
            {
                return "Error";
            }
            else if (e is TimeoutException)
            {
                return "Timeout";
            }
            else
            {
                return "Exception";
            }
        }
    }
}