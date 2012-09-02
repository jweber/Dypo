using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PocoDb.Utility
{
    internal static class ReflectionUtility
    {
        public static string GetNameFromExpression<T, TProperty>(Expression<Func<T, TProperty>> property, bool onlyRenderLastExpressionPart = false)
        {
            var nameParts = new Stack<string>();
            GetNameFromExpression(nameParts, property.Body);

            string name = string.Join(".", nameParts);

            if (onlyRenderLastExpressionPart)
            {
                return name.Split('.')[nameParts.Count - 1];
            }

            return name;
        }

        private static void GetNameFromExpression(Stack<string> nameParts, Expression expression)
        {
            string name = null;
            Expression parentExpression = null;

            var methodExpression = expression as MethodCallExpression;
            if (methodExpression != null)
            {
                name = methodExpression.Method.Name;
                parentExpression = methodExpression.Object;
            }

            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
            {
                name = memberExpression.Member.Name;
                parentExpression = memberExpression.Expression;
            }

            var unaryExpression = expression as UnaryExpression;
            if (unaryExpression != null)
            {
                name = ((MemberExpression) unaryExpression.Operand).Member.Name;
                parentExpression = ((MemberExpression) unaryExpression.Operand).Expression;
            }

            if (parentExpression != null)
            {
                nameParts.Push(name);
                GetNameFromExpression(nameParts, parentExpression);
            }
        }
    }
}
