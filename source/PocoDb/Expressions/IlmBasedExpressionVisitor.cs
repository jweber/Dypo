using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PocoDb.Expressions
{
    class IlmBasedExpressionVisitor : ExpressionVisitor
    {
        private StringBuilder _stringBuilder;

        public IlmBasedExpressionVisitor()
        {
            _stringBuilder = new StringBuilder();
        }

        public void ProcessExpression(Expression expression)
        {
            VisitExpression(expression);
        }

        private void VisitExpression(Expression expression)
        {
            if (expression.NodeType == ExpressionType.AndAlso)
            {
                VisitAndAlso((BinaryExpression)expression);
            }
            else if (expression.NodeType == ExpressionType.Equal)
            {
                VisitEqual((BinaryExpression)expression);
            }
            else if (expression.NodeType == ExpressionType.LessThanOrEqual)
            {
                VisitLessThanOrEqual((BinaryExpression)expression);
            }
            else if (expression is MethodCallExpression)
            {
                VisitMethodCall((MethodCallExpression)expression);
            }
            else if (expression is LambdaExpression)
            {
                VisitExpression(((LambdaExpression)expression).Body);
            }
        }

        private void VisitAndAlso(BinaryExpression andAlso)
        {
            VisitExpression(andAlso.Left);
            VisitExpression(andAlso.Right);
        }

        private void VisitEqual(BinaryExpression expression)
        {
            if (expression.Left.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression) expression.Left;
                _stringBuilder.AppendFormat("{0} = ", memberExpression.Member.Name);
            }

            if (expression.Right.NodeType == ExpressionType.Constant)
                _stringBuilder.Append("'" + (string) ((ConstantExpression)expression.Right).Value + "'");
            else if (expression.Right.NodeType == ExpressionType.MemberAccess)
                _stringBuilder.Append("'" + (string) GetMemberValue((MemberExpression)expression.Right) + "'");
            else
                throw new NotSupportedException("Expression type not supported for APINumber: " + expression.Right.NodeType.ToString());

        }

        private void VisitLessThanOrEqual(BinaryExpression expression)
        {

        }

        private void VisitMethodCall(MethodCallExpression expression)
        {

            if (expression.Method.DeclaringType == typeof(Queryable) 
                && expression.Method.Name == "Where")
            {
                VisitExpression(((UnaryExpression)expression.Arguments[1]).Operand);
            }
            else if ((expression.Method.DeclaringType == typeof(Queryable)) &&
              (expression.Method.Name == "OrderBy"))
            {
            }
            else
            {
                throw new NotSupportedException("Method not supported: " + expression.Method.Name);
            }

        }

        #region Helpers

        private Object GetMemberValue(MemberExpression memberExpression)
        {
            MemberInfo memberInfo;
            Object obj;

            if (memberExpression == null)
                throw new ArgumentNullException("memberExpression");

            // Get object
            if (memberExpression.Expression is ConstantExpression)
                obj = ((ConstantExpression)memberExpression.Expression).Value;
            else if (memberExpression.Expression is MemberExpression)
                obj = GetMemberValue((MemberExpression)memberExpression.Expression);
            else
                throw new NotSupportedException("Expression type not supported: " + memberExpression.Expression.GetType().FullName);

            // Get value
            memberInfo = memberExpression.Member;
            if (memberInfo is PropertyInfo)
            {
                PropertyInfo property = (PropertyInfo)memberInfo;
                return property.GetValue(obj, null);
            }
            else if (memberInfo is FieldInfo)
            {
                FieldInfo field = (FieldInfo)memberInfo;
                return field.GetValue(obj);
            }
            else
            {
                throw new NotSupportedException("MemberInfo type not supported: " + memberInfo.GetType().FullName);
            }
        }

        #endregion Helpers
    }
}