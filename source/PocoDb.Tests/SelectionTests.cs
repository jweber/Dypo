using System;
using NUnit.Framework;
using PocoDb.Attributes;
using PocoDb.Interfaces;
using PocoDb.Select;

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
            var generatedSql = ((SelectQuery<TModel>) query).GetSql();
            Assert.That(generatedSql, Is.EqualTo(expected).IgnoreCase);
        }

        [Test]
        public void DynamicSelect()
        {
            using (var db = Db.Connect())
            {
                var query = db.Select("select Id, Username, Created from account where Id = @0", 1);
                var result = query.First();

                Assert.AreEqual(1, result.Id);
                Assert.AreEqual("TestAccount1", result.Username);
                Assert.AreEqual(new DateTime(2012, 1, 25), result.Created);
            }
        }

        [Test]
        public void DynamicSelect2()
        {
            using (var db = Db.Connect())
            {
                var query = db.Select("select Id from account where (Id in (@0))", new[] { 1, 2 });
                var result = query.First();

                Assert.AreEqual(1, result.Id);
            }
        }

        [Test]
        public void DynamicSelect3()
        {
            using (var db = Db.Connect())
            {
                var query = db.Select("select id, username from account where Id = @id", new { id = 1 });
                var result = query.First();

                Assert.AreEqual(1, result.Id);
                Assert.AreEqual("TestAccount1", result.Username);
            }
        }

        [Test]
        public void DynamicSelect4()
        {
            using (var db = Db.Connect())
            {
                var query = db.Select("select id, username as Name from account where Id in (@ids)", new { ids = new[] { 1, 2 } });
                var result = query.First();

                Assert.AreEqual(1, result.Id);
                Assert.AreEqual("TestAccount1", result.Name);
            }
        }

        [Test]
        public void DynamicSelect5()
        {
            using (var db = Db.Connect())
            {
                var query = db.Select("select id, username from account where username = @username", new { id = 1, username = "TestAccount1" });
                var result = query.First();

                Assert.AreEqual(1, result.Id);
                Assert.AreEqual("TestAccount1", result.Username);
            }
        }

        [Test]
        public void DynamicSelect6()
        {
            using (var db = Db.Connect())
            {
                var query = db.Select("select id from account where username = @1", 1, "TestAccount1");
                var result = query.First();

                Assert.AreEqual(1, result.Id);
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
