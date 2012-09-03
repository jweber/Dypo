using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Dypo.Attributes;

namespace Dypo.Tests.Attributes
{
    [TestFixture]
    class ColumnNameAttributeTests
    {
        [Test]
        public void ColumnName_Is_As_Expected()
        {
            const string columnName = "TestColumnName";
            var attribute = new ColumnNameAttribute(columnName);

            Assert.AreEqual(columnName, attribute.ColumnName);
        }
    }
}
