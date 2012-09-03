using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dypo.Providers.SqlServer;
using Dypo.Utility;

namespace Dypo.Expressions
{
    internal class SqlExpressionVisitor<TModel>
    {
        private readonly SqlServerDialect _dialect;

        public SqlExpressionVisitor()
        {
            _dialect = new SqlServerDialect();
        }

        public string VisitExpression(Expression expression)
        {
            if (expression == null)
                return string.Empty;

            switch (expression.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnaryExpression(expression as UnaryExpression);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return "(" + VisitBinaryExpression(expression as BinaryExpression) + ")";
                case ExpressionType.Lambda:
                    return VisitLambdaExpression(expression as LambdaExpression);
                case ExpressionType.MemberAccess:
                    return VisitMemberExpression(expression as MemberExpression);
                case ExpressionType.Constant:
                    return VisitConstantExpression(expression as ConstantExpression);
                case ExpressionType.Parameter:
                    return VisitParameterExpression(expression as ParameterExpression);
                case ExpressionType.Call:
                    return VisitMethodCallExpression(expression as MethodCallExpression);
                case ExpressionType.New:
                    return VisitNewExpression(expression as NewExpression);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArrayExpression(expression as NewArrayExpression);
                case ExpressionType.ListInit:
                    throw new NotImplementedException("Cannot convert ListInitExpression into SQL");
                default:
                    return expression.ToString();
            }
        }

        private IList<string> VisitExpression(IEnumerable<Expression> expressions)
        {
            return expressions.Select(VisitExpression).ToList();
        }

        private string VisitUnaryExpression(UnaryExpression expression)
        {
            return VisitExpression(expression.Operand);
        }

        private string VisitBinaryExpression(BinaryExpression expression)
        {
            string left = VisitExpression(expression.Left);
            string right = VisitExpression(expression.Right);

            string binaryOperator = GetBinaryOperationFromExpressionType(expression.NodeType, 
                right != null && right.Equals("null", StringComparison.InvariantCultureIgnoreCase));

            switch (expression.NodeType)
            {
                case ExpressionType.Modulo:
                case ExpressionType.Coalesce:
                    return string.Format("{0}({1},{2})", binaryOperator, left, right);
                default:
                    return string.Concat(left, " ", binaryOperator, " ", right);
            }
        }

        private string VisitLambdaExpression(LambdaExpression lambdaExpression)
        {
            return VisitExpression(lambdaExpression.Body);
        }

        private string VisitMemberExpression(MemberExpression memberExpression)
        {
            if (memberExpression.Expression != null
                && memberExpression.Expression.NodeType == ExpressionType.Parameter)
            {
                return ModelUtility.GetColumnName<TModel>(memberExpression.Member);
            }

            object value = GetMemberExpressionValue(memberExpression);

            var enumerableValue = value as IEnumerable;
            if (enumerableValue != null)
            {
                var quotedValues = enumerableValue.Cast<object>()
                    .Select(item => _dialect.QuoteValue(item, item.GetType()));

                return string.Join(",", quotedValues);
            }

            return _dialect.QuoteValue(value, value != null ? value.GetType() : null);
        }

        private object GetMemberExpressionValue(MemberExpression memberExpression)
        {
            if (memberExpression.Expression.NodeType != ExpressionType.Constant)
                return null;

            var convertToObjectExpression = Expression.Convert(memberExpression, typeof(object));
            var getMemberExpressionValue = Expression.Lambda<Func<Object>>(convertToObjectExpression);
            var getter = getMemberExpressionValue.Compile();

            return getter();
        }

        private string VisitConstantExpression(ConstantExpression expression)
        {
            if (expression == null || expression.Value == null)
                return "NULL";
            
            return _dialect.QuoteValue(expression.Value, expression.Value.GetType());
        }

        private string VisitParameterExpression(ParameterExpression expression)
        {
            return expression.Name;
        }

        private bool ExpressionIsEnumerable(Expression expression)
        {
            if (expression == null)
                return false;

            if (expression is NewArrayExpression || expression is ListInitExpression)
                return true;

            var memberExpression = expression as MemberExpression;
            if (memberExpression == null)
                return false;

            object memberExpressionValue = GetMemberExpressionValue(memberExpression);
            return memberExpressionValue is IEnumerable;
        }

        private string VisitMethodCallExpression(MethodCallExpression expression)
        {
            var arguments = VisitExpression(expression.Arguments);
            bool firstArgumentIsEnumerable = expression.Arguments.Count > 0
                                             && ExpressionIsEnumerable(expression.Arguments[0]);

            string property;
            if (expression.Object != null)
            {
                if (ExpressionIsEnumerable(expression.Object))
                {
                    property = arguments[0];
                    arguments[0] = VisitExpression(expression.Object);
                    firstArgumentIsEnumerable = true;
                }
                else
                {
                    property = VisitExpression(expression.Object);
                }
            }
            else if (firstArgumentIsEnumerable)
            {
                property = arguments[1];
                arguments.RemoveAt(1);
            }
            else
            {
                property = arguments[0];
                arguments.RemoveAt(0);
            }

            switch (expression.Method.Name)
            {
                case "ToUpper":
                    return string.Format("upper({0})", property);
                case "ToLower":
                    return string.Format("lower({0})", property);
                case "Contains":
                    if (firstArgumentIsEnumerable)
                        return string.Format("{0} in ({1})", property, arguments[0]);

                    return string.Format("{0} like '%{1}%'", property, _dialect.UnQuoteValue(arguments[0]));

                case "StartsWith":
                    return string.Format("{0} like '{1}%'", property, _dialect.UnQuoteValue(arguments[0]));
                case "EndsWith":
                    return string.Format("{0} like '%{1}'", property, _dialect.UnQuoteValue(arguments[0]));
                case "Substring":
                    int substringStartIndex = int.Parse(arguments[0]) + 1;
                    int substringLength = int.Parse(arguments[1]);
                    
                    return string.Format("substring({0},{1},{2})", property, substringStartIndex, substringLength);
				
                // one arg
                case "Floor":
                case "Ceiling":
                case "Abs":
                case "Sum":
                    return string.Format("{0}({1})", expression.Method.Name, property);
                
                // two args
                case "Round":
                    int roundLength = int.Parse(arguments[0]);
                    return string.Format("round({0}, {1})", property, roundLength);
				
				// many args
				case "Coalesce":
                case "Concat":
                    arguments.Insert(0, property);
                    return string.Format("{0}({1})",
                                         expression.Method.Name,
                                         string.Join(",", arguments));

                case "In":
                    return string.Format("{0} in ({1})", property, arguments[0]);

                case "As":
                    return string.Format("{0} as {1}", property, _dialect.UnQuoteValue(arguments[0]));

                default:
                    return string.Empty;
            }
        }

        private string VisitNewExpression(NewExpression expression)
        {
            throw new NotImplementedException();
        }

        private string VisitNewArrayExpression(NewArrayExpression expression)
        {
            var arrayValues = VisitExpression(expression.Expressions);
            return string.Join(",", arrayValues);
        }

        private string GetBinaryOperationFromExpressionType(ExpressionType expressionType, bool rightSideIsNull)
        {
            switch (expressionType)
            {
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Equal:
                    return rightSideIsNull ? "IS" : "=";
                case ExpressionType.NotEqual:
                    return rightSideIsNull ? "IS NOT" : "<>";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "MOD";
                case ExpressionType.Coalesce:
                    return "COALESCE";
                default:
                    return expressionType.ToString();
            }
        }
    }
}
