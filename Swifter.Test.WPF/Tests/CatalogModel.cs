using Swifter.Json;
using Swifter.Test.WPF.Models;
using System.IO;

namespace Swifter.Test.WPF.Tests
{
    public class CatalogModel : BaseTest<Catalog>
    {
        public override string TestName => "Catalog";

        public override Catalog GetObject()
        {
            return JsonFormatter.DeserializeObject<Catalog>(File.ReadAllText(@"..\..\..\..\Swifter.Test\Resources\catalog.json"));
        }
    }

}