using Swifter.RW;
using Swifter.Test.WPF.Models;
using System.Collections.Generic;

namespace Swifter.Test.WPF.Tests
{
    public class DeviceDictionary : BaseTest<Dictionary<string,object>>
    {
        public override string TestName => "Dictionary<string,object>";

        public override Dictionary<string, object> GetObject()
        {
            return ValueCopyer.ValueOf(new RandomValueReader(1218).FastReadObject<Device>()).ReadDictionary<string, object>();
        }
    }

}