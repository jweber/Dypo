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

        #region Test Objects

        [TableName("TableName")]
        private class PocoWithTableNameAttribute
        {}

        private class PocoWithoutTableNameAttribute
        {}

        #endregion
    }
}
