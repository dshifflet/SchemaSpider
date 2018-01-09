using System.Configuration;
using System.Security.Principal;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Util;
using Northwind.Infrastructure;
using Northwind.Models;

namespace Northwind.Tests
{
    [TestClass]
    public class NhTests
    {
        private static ISessionFactory _sessionFactory;

        [ClassInitialize]
        public static void StartUp(TestContext ctx)
        {
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("Test", string.Empty),
                new[]
                {
                    string.Empty
                });

            _sessionFactory =
                NhFactory.CreateNhSessionFactory(ConfigurationManager.ConnectionStrings["NORTHWIND"].ConnectionString, null,
                    false);
        }
        /// <summary>
        /// Shows the mappings are working properly
        /// </summary>
        [TestMethod]
        public void CanGetCategoriesAndProducts()
        {
            using (var sess = _sessionFactory.OpenSession())
            using (var txn = sess.BeginTransaction())
            {
                var categories = sess.QueryOver<Categories>().List();
                Assert.IsTrue(categories.Any());
                foreach (var category in categories)
                {
                    Assert.IsTrue(category.Products.Any());
                }
                txn.Commit();
            }
        }
    }
}
