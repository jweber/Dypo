using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Dypo.Providers;

namespace Dypo.Tests.Providers
{
    [TestFixture]
    public class AbstractDialectTests
    {
        class AbstractDialectWrapper : AbstractDialect
        {}

        [Test]
        public void UnQuoteValue_Removes_Wrapping_Quotes()
        {
            var abstractDialect = new AbstractDialectWrapper();

            string actual = abstractDialect.UnQuoteValue("'test'");
            Assert.AreEqual("test", actual);
        }

        [Test]
        public void UnQuoteValue_Ignores_Unquoted_Value()
        {
            var abstractDialect = new AbstractDialectWrapper();

            string actual = abstractDialect.UnQuoteValue("test");
            Assert.AreEqual("test", actual);
        }
    }
}
