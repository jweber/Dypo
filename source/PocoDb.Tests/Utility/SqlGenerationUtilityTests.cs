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
    class SqlGenerationUtilityTests
    {
        [Test]
        public void GetTableName_Returns_TableName_Defined_By_Argument()
        {
            var generatedTableName = SqlGenerationUtility.GetTableName<PocoWithTableNameAttribute>("test_table_name");
            Assert.AreEqual("test_table_name", generatedTableName);
        }

        [Test]
        public void GetTableName_Returns_TableName_Defined_By_Attribute()
        {
            var generatedTableName = SqlGenerationUtility.GetTableName<PocoWithTableNameAttribute>();
            Assert.AreEqual("TableName", generatedTableName);
        }

        [Test]
        public void GetTableName_Returns_TableName_Defined_By_Name_Of_Class()
        {
            var generatedTableName = SqlGenerationUtility.GetTableName<PocoWithoutTableNameAttribute>();
            Assert.AreEqual("PocoWithoutTableNameAttribute", generatedTableName);
        }

        [Test]
        public void GetColumnName_Returns_ColumnName_Defined_By_Attribute()
        {
            var property = typeof(PocoWithTableNameAttribute).GetProperty("PropertyWithColumnNameAttribute");
            var columnName = SqlGenerationUtility.GetColumnName(property);
            Assert.AreEqual("TestColumnName", columnName);
        }

        [Test]
        public void GetColumnName_Returns_ColumnName_Defined_By_PropertyName()
        {
            var property = typeof(PocoWithoutTableNameAttribute).GetProperty("PropertyWithoutColumnNameAttribute");
            var columnName = SqlGenerationUtility.GetColumnName(property);
            Assert.AreEqual("PropertyWithoutColumnNameAttribute", columnName);
        }

        #region Test Objects

        [TableName("TableName")]
        private class PocoWithTableNameAttribute
        {
            [ColumnName("TestColumnName")]
            public string PropertyWithColumnNameAttribute { get; set; }
        }

        private class PocoWithoutTableNameAttribute
        {
            public string PropertyWithoutColumnNameAttribute { get; set; }
        }

        #endregion
    }
}
