using System;

namespace Swifter.Test.WPF.Tests
{
    public abstract class BaseTest<TObject> : ITest
    {
        public virtual Type ObjectType => typeof(TObject);

        public virtual string TestName => GetType().Name;

        public abstract TObject GetObject();

        public virtual bool Equals(TObject obj1, TObject obj2)
        {
            return TestHelper.Equals(obj1, obj2);
        }
    }

}