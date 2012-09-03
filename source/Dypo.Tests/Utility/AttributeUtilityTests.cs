using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Dypo.Utility;

namespace Dypo.Tests.Utility
{
    [TestFixture]
    class AttributeUtilityTests
    {
        [Test]
        public void AttributeUtility_Returns_Null_When_Attribute_Does_Not_Exists_On_Class()
        {
            var attribute = AttributeUtility.GetAttribute<TestAttributeAttribute>(typeof(ClassWithNoAttribute));
            Assert.IsNull(attribute);
        }

        [Test]
        public void AttributeUtility_Returns_Attribute_When_Exists_On_Class()
        {
            var attribute = AttributeUtility.GetAttribute<TestAttributeAttribute>(typeof(ClassWithAttribute));
            Assert.IsNotNull(attribute);
        }

        [Test]
        public void AttributeUtility_Returns_Null_When_Attribute_Does_Not_Exist_On_Method()
        {
            var method = typeof(ClassWithNoAttribute).GetMethod("MethodWithoutAttribute");
            var attribute = AttributeUtility.GetAttribute<TestAttributeAttribute>(method);
            Assert.IsNull(attribute);
        }

        [Test]
        public void AttributeUtility_Returns_Attribute_When_Attribute_Exists_On_Method()
        {
            var method = typeof(ClassWithAttribute).GetMethod("MethodWithAttribute");
            var attribute = AttributeUtility.GetAttribute<TestAttributeAttribute>(method);
            Assert.IsNotNull(attribute);
        }

        #region Test Objects

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        private class TestAttributeAttribute : Attribute
        {
            public string Data { get; set; }
        }

        private class ClassWithNoAttribute
        {
            public void MethodWithoutAttribute()
            {}
        }

        [TestAttributeAttribute]
        private class ClassWithAttribute
        {
            [TestAttributeAttribute]
            public void MethodWithAttribute()
            {}
        }

        #endregion
    }
}
