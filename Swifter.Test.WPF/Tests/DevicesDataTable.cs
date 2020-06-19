using Swifter.RW;
using Swifter.Test.WPF.Models;
using System.Data;

namespace Swifter.Test.WPF.Tests
{
    public class DevicesDataTable : BaseTest<DataTable>
    {
        public override string TestName => "DataTable";

        public override DataTable GetObject()
        {
            return ValueCopyer.ValueOf(new RandomValueReader(1218).ReadList<Device>()).ReadDataTable().IdentifyColumnTypes();
        }
    }

}