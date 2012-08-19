using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PocoDb.Attributes;

namespace PocoDb.Tests.Attributes
{
    [TestFixture]
    class TableNameAttributeTests
    {
        [Test]
        public void TableName_Is_As_Expected()
        {
            const string tableName = "TestTableName";
            var attribute = new TableNameAttribute(tableName);

            Assert.AreEqual(tableName, attribute.TableName);
        }
    }
}
