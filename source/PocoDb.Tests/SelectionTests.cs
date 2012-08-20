using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PocoDb.Attributes;

namespace PocoDb.Tests
{
    [TestFixture]
    public class SelectionTests
    {
        public class Account
        {
            public int Id { get; set; }
            public string Username { get; set; }
            
            [ColumnName("EmailAddress")]
            public string Email { get; set; }
        }

        [Test]
        public void Test2()
        {
            var db = Db.Connect("EmbeddedTest");
            var results = db.Select<Account>()
                .Execute()
                .ToList();
            
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(1, results[0].Id);
            Assert.AreEqual("TestAccount1", results[0].Username);
            Assert.AreEqual("test@example.com", results[0].Email);
        }
    }
}
