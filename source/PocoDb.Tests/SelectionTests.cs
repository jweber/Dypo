using System;
using NUnit.Framework;
using PocoDb.Attributes;
using PocoDb.Interfaces;

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

            public DateTime Created { get; set; }
        }

        private void AssertQuery<TModel>(ISelectQuery<TModel> query, string expected)
        {
            var generatedSql = ((SelectQuery<TModel>) query).GenerateSql();
            Assert.That(generatedSql, Is.EqualTo(expected).IgnoreCase);
        }

        [Test]
        public void DynamicSelect()
        {
            using (var db = Db.Connect())
            {
                var query = db.Select("select Id, Username, Created from account where Id = 1");
                var result = query.First();

                Assert.AreEqual(1, result.Id);
                Assert.AreEqual("TestAccount1", result.Username);
                Assert.AreEqual(new DateTime(2012, 1, 25), result.Created);
            }
        }

        [Test]
        public void Select_OrderBy()
        {
            using (var db = Db.Connect())
            {
                var query = db.Select<Account>().OrderBy(a => a.Username);
                AssertQuery(query, "select id, username, emailaddress, created from account order by username asc");
            }
        }

        [Test]
        public void Select_OrderBy2()
        {
            using (var db = Db.Connect())
            {
                var query = db.Select<Account>(where: a => a.Id == 1 || a.Id > 10)
                    .OrderBy(a => a.Username).OrderByDescending(a => a.Email);

                AssertQuery(query, "select id, username, emailaddress, created from account where ((id = 1) or (id > 10)) order by Username asc, EmailAddress desc");
            }
        }

        [Test, Category(TestCategory.Database)]
        public void Select_Returns_Expected_Data()
        {
            var db = Db.Connect("EmbeddedTest");
            var results = db.Select<Account>(a => a.Id == 1 || a.Id == 2)
                .ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(1, results[0].Id);
            Assert.AreEqual("TestAccount1", results[0].Username);
            Assert.AreEqual("test@example.com", results[0].Email);
            Assert.AreEqual(new DateTime(2012, 1, 25), results[0].Created);
        }
    }
}
