using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Dypo.Expressions;
using Dypo.Query;

namespace Dypo.Tests.Expressions
{
    public static class ExpressionVisitorTestExtensions
    {
        public static void AssertEqual<TModel, TResult>(this Expression<Func<TModel, TResult>> expression, string expected)
        {
            var visitor = new SqlExpressionVisitor<TModel>();
            string actual = visitor.VisitExpression(expression);

            Assert.That(actual, Is.EqualTo(expected).IgnoreCase);
        }
    }

    [TestFixture]
    class SqlExpressionTests
    {
        class TestAccount
        {
            public string Username { get; set; }
            public string Alternate { get; set; }
            public int Comments { get; set; }
            public decimal DecimalValue { get; set; }
        }

        private Expression<Func<TestAccount, TResult>> AsExpression<TResult>(Expression<Func<TestAccount, TResult>> expression)
        {
            return expression;
        }

        #region Constants

        [Test]
        public void Constant_Integer()
        {
            AsExpression(a => (int) 10)
                .AssertEqual("10");
        }

        [Test]
        public void Constant_Decimal()
        {
            AsExpression(a => (decimal) 10.505)
                .AssertEqual("10.505");
        }

        [Test]
        public void Constant_Float()
        {
            AsExpression(a => (float) 10.10)
                .AssertEqual("10.1");
        }

        [Test]
        public void Constant_String()
        {
            AsExpression(a => "test")
                .AssertEqual("'test'");
        }

        [Test]
        public void Constant_Bool()
        {
            AsExpression(a => true)
                .AssertEqual("true");
        }

        #endregion

        #region Binary Operations

        [Test]
        public void BinaryExpression_Equals()
        {
            AsExpression(a => a.Username == "test")
                .AssertEqual("(Username = 'test')");
        }

        [Test]
        public void BinaryExpression_IsNull()
        {
            AsExpression(a => a.Username == null)
                .AssertEqual("(Username is null)");
        }

        [Test]
        public void BinaryExpression_IsNotNull()
        {
            AsExpression(a => a.Username != null)
                .AssertEqual("(Username is not null)");
        }

        [Test]
        public void BinaryExpression_NotEquals()
        {
            AsExpression(a => a.Username != "test")
                .AssertEqual("(Username <> 'test')");
        }

        [Test]
        public void BinaryExpression_GreaterThan()
        {
            AsExpression(a => a.Comments > 1)
                .AssertEqual("(Comments > 1)");
        }

        [Test]
        public void BinaryExpression_GreaterThanOrEqual()
        {
            AsExpression(a => a.Comments >= 1)
                .AssertEqual("(Comments >= 1)");
        }

        [Test]
        public void BinaryExpression_LessThan()
        {
            AsExpression(a => a.Comments < 1)
                .AssertEqual("(Comments < 1)");
        }
        
        [Test]
        public void BinaryExpression_LessThanOrEqual()
        {
            AsExpression(a => a.Comments <= 1)
                .AssertEqual("(Comments <= 1)");
        }

        [Test]
        public void BinaryExpression_And()
        {
            AsExpression(a => a.Username == "test" && a.Comments == 1)
                .AssertEqual("((Username = 'test') and (Comments = 1))");
        }

        [Test]
        public void BinaryExpression_Or()
        {
            AsExpression(a => a.Username == "test" || a.Username == "test 2")
                .AssertEqual("((Username = 'test') or (Username = 'test 2'))");
        }

        [Test]
        public void BinaryExpression_Add()
        {
            AsExpression(a => a.Comments + 10)
                .AssertEqual("(Comments + 10)");
        }

        [Test]
        public void BinaryExpression_Subtract()
        {
            AsExpression(a => a.Comments - 10)
                .AssertEqual("(Comments - 10)");
        }

        [Test]
        public void BinaryExpression_Multiply()
        {
            AsExpression(a => a.Comments * 10)
                .AssertEqual("(Comments * 10)");
        }

        [Test]
        public void BinaryExpression_Divide()
        {
            AsExpression(a => a.Comments / 10)
                .AssertEqual("(Comments / 10)");
        }

        [Test]
        public void BinaryExpression_Modulo()
        {
            AsExpression(a => a.Comments % 10)
                .AssertEqual("(mod(Comments,10))");
        }

        [Test]
        public void BinaryExpression_Coalesce()
        {
            AsExpression(a => a.Username ?? "" )
                .AssertEqual("(coalesce(Username,''))");
        }

        #endregion

        #region MethodCall Operations

        [Test]
        public void MethodCall_ToUpper()
        {
            AsExpression(a => a.Username.ToUpper())
                .AssertEqual("upper(Username)");
        }

        [Test]
        public void MethodCall_ToLower()
        {
            AsExpression(a => a.Username.ToLower())
                .AssertEqual("lower(Username)");
        }

        [Test]
        public void MethodCall_Contains()
        {
            AsExpression(a => a.Username.Contains("test"))
                .AssertEqual("Username like '%test%'");
        }

        [Test]
        public void MethodCall_Array_Contains()
        {
            var array = new[] { 1, 2 };
            AsExpression(a => array.Contains(a.Comments))
                .AssertEqual("Comments in (1,2)");
        }

        [Test]
        public void MethodCall_Array_Contains2()
        {
            AsExpression(a => new[] { 1, 2 }.Contains(a.Comments))
                .AssertEqual("Comments in (1,2)");
        }

        [Test]
        public void MethodCall_List_Contains()
        {
            var list = new List<string>() { "test1", "test2" };
            AsExpression(a => list.Contains(a.Username))
                .AssertEqual("Username in ('test1','test2')");
        }

        [Test, Description("ListInit expressions not currently supported")]
        [ExpectedException(typeof(NotImplementedException))]
        public void MethodCall_List_Contains2()
        {
            AsExpression(a => new List<string> { "test1", "test2" }.Contains(a.Username))
                .AssertEqual("Username in ('test1', 'test2')");
        }

        [Test]
        public void MethodCall_StartsWith()
        {
            AsExpression(a => a.Username.StartsWith("test"))
                .AssertEqual("Username like 'test%'");
        }

        [Test]
        public void MethodCall_EndsWith()
        {
            AsExpression(a => a.Username.EndsWith("test"))
                .AssertEqual("Username like '%test'");
        }

        [Test]
        public void MethodCall_Substring()
        {
            AsExpression(a => a.Username.Substring(1, 2))
                .AssertEqual("substring(Username,2,2)");
        }

        [Test]
        public void MethodCall_Floor()
        {
            AsExpression(a => Math.Floor(a.DecimalValue))
                .AssertEqual("floor(DecimalValue)");
        }

        [Test]
        public void MethodCall_Ceiling()
        {
            AsExpression(a => Math.Ceiling(a.DecimalValue))
                .AssertEqual("ceiling(DecimalValue)");
        }

        [Test]
        public void MethodCall_Absolute()
        {
            AsExpression(a => Math.Abs(a.DecimalValue))
                .AssertEqual("abs(DecimalValue)");
        }

        [Test]
        public void MethodCall_Sum()
        {
            AsExpression(a => a.DecimalValue.Sum())
                .AssertEqual("sum(DecimalValue)");
        }

        [Test]
        public void MethodCall_Round()
        {
            AsExpression(a => Math.Round(a.DecimalValue, 3))
                .AssertEqual("round(DecimalValue, 3)");
        }

        [Test]
        public void MethodCall_Coalesce()
        {
            AsExpression(a => a.Comments.Coalesce(2, 3))
                .AssertEqual("coalesce(Comments,2,3)");
        }

        [Test]
        public void MethodCall_Concat()
        {
            AsExpression(a => string.Concat(a.Username, "test1", "test2"))
                .AssertEqual("concat(Username,'test1','test2')");
        }

        [Test]
        public void MethodCall_In()
        {
            AsExpression(a => a.Comments.In(1, 2))
                .AssertEqual("Comments in (1,2)");
        }

        [Test]
        public void MethodCall_As()
        {
            AsExpression(a => a.Username.As("AliasedUsername"))
                .AssertEqual("Username as AliasedUsername");
        }

        #endregion
    }
}
