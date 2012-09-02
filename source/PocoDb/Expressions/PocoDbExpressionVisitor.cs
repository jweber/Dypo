using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PocoDb.Expressions
{
    class PocoDbExpressionVisitor : ExpressionVisitor
    {
        private readonly StringBuilder _stringBuilder;

        public PocoDbExpressionVisitor()
        {
            _stringBuilder = new StringBuilder();
        }

        public string GetSql()
        {
            return _stringBuilder.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _stringBuilder.Append("(");

            this.Visit(node.Left);

            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _stringBuilder.Append(" AND ");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    _stringBuilder.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    _stringBuilder.Append("=");
                    break;
                case ExpressionType.NotEqual:
                    _stringBuilder.Append("<>");
                    break;
                case ExpressionType.Add:
                    _stringBuilder.Append("+");
                    break;
                case ExpressionType.Subtract:
                    _stringBuilder.Append("-");
                    break;
                case ExpressionType.Multiply:
                    _stringBuilder.Append("*");
                    break;
                case ExpressionType.Divide:
                    _stringBuilder.Append("/");
                    break;
                case ExpressionType.GreaterThan:
                    _stringBuilder.Append(">");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _stringBuilder.Append(">=");
                    break;
                case ExpressionType.LessThan:
                    _stringBuilder.Append("<");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _stringBuilder.Append("=<");
                    break;

            }

            this.Visit(node.Right);

            _stringBuilder.Append(")");

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
            {
                this.Visit(node.Object);
                _stringBuilder.Append(" LIKE ");
                this.Visit(node.Arguments);
            }

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type == typeof(string))
            {
                _stringBuilder.Append("'" + node.Value + "'");
            }
            else
            {
                _stringBuilder.Append(node.Value);
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _stringBuilder.Append(node.Member.Name);
            return node;
        }

    }
}
