using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;

namespace SchemaSpider.Tests
{
    [TestClass]
    public class BasicTests
    {
        private static readonly OracleConnection Connection = 
            new OracleConnection(ConfigurationManager.ConnectionStrings["NORTHWIND"].ConnectionString);

        [ClassInitialize]
        public static void Startup(TestContext ctx)
        {
            Connection.Open();
        }

        [ClassCleanup]
        public static void Finish()
        {
            Connection.Close();
        }
        
        [TestMethod]
        public void CanExplore()
        {
            Explorer.Explore(Connection, new[]
            {
                "northwind.categories",
                "northwind.products",
                "northwind.shippers"
            }, 10, new string[0]);
        }

        [TestMethod]
        public void CanExploreSerializeResults()
        {
            var result = Explorer.Explore(Connection, new[]
            {
                "NORTHWIND.CATEGORIES",
                "NORTHWIND.CUSTOMERS",
                "NORTHWIND.EMPLOYEES",
                "NORTHWIND.ORDERS",
                "NORTHWIND.ORDER_DETAILS",
                "NORTHWIND.PRODUCTS",
                "NORTHWIND.SHIPPERS",
                "NORTHWIND.SUPPLIERS"
            }, 10, new string[] {});

            var expecting = result.Tables.Length;

            var serializer = new XmlSerializer(typeof(ExplorerResults));
            var output = new FileInfo("test_out.xml");
            using (var sw = new StreamWriter(output.Create()))
            {
                serializer.Serialize(sw, result);
            }

            output.Refresh();
            using (var sr = new StreamReader(output.OpenRead()))
            {
                var test = (ExplorerResults) serializer.Deserialize(sr);
                Assert.IsTrue(expecting==test.Tables.Length);
            }
        }

        [TestMethod]
        public void CanBuildModelsMappings()
        {
            var ignore = new string[] {};

            var result = Explorer.Explore(Connection, new[]
            {
                "NORTHWIND.CATEGORIES",
                "NORTHWIND.CUSTOMERS",
                "NORTHWIND.EMPLOYEES",
                "NORTHWIND.ORDERS",
                "NORTHWIND.ORDER_DETAILS",
                "NORTHWIND.PRODUCTS",
                "NORTHWIND.SHIPPERS",
                "NORTHWIND.SUPPLIERS"
            }, 10, ignore);

            var di = new DirectoryInfo("SCHEMATEST");
            di.Create();
            //Northwind tables are plural...  So we don't want Productss, we want Products
            Explorer.BuildModelMappings(di, result, ignore, "Northwind", "");
            var models = new DirectoryInfo(string.Format("{0}\\models", di.FullName));
            var mappings = new DirectoryInfo(string.Format("{0}\\mappings", di.FullName));
            Assert.IsTrue(models.GetFiles().Any());
            Assert.IsTrue(mappings.GetFiles().Any());

        }
    }
}
