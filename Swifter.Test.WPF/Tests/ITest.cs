using System;

namespace Swifter.Test.WPF.Tests
{
    public interface ITest
    {
        string TestName { get; }
        Type ObjectType { get; }
    }

}