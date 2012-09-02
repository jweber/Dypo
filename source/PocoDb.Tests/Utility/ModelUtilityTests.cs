using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PocoDb.Attributes;
using PocoDb.Utility;

namespace PocoDb.Tests.Utility
{
    [TestFixture]
    public class ModelUtilityTests
    {
        class TestAccount
        {
            [ColumnName("TestAccountId")]
            public int Id { get; set; }
            public string EmailAddress { get; set; }
        }

        [Test]
        public void GetColumnName_Expression()
        {
            var actual = ModelUtility.GetColumnName((TestAccount a) => a.EmailAddress);
            Assert.That(actual, Is.EqualTo("EmailAddress"));
        }

        [Test]
        public void GetColumnName_Expression2()
        {
            var actual = ModelUtility.GetColumnName((TestAccount a) => a.Id);
            Assert.That(actual, Is.EqualTo("TestAccountId"));
        }
    }
}
