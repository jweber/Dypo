using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using PocoDb.Expressions;
using PocoDb.Interfaces;
using PocoDb.Utility;

namespace PocoDb.Select
{
    internal class SelectQuery<TModel> : AbstractSelectQuery<TModel>, ISelectQuery<TModel>
    {
        private readonly IDbContext _dbContext;
        private readonly List<string> _projectionColumns = new List<string>();
        private readonly IList<string> _wherePredicates = new List<string>();
        private readonly IList<string> _orderConditions = new List<string>();

        private readonly string _tableName;

        public SelectQuery(IDbContext dbContext, Expression<Predicate<TModel>> where = null)
            : base(dbContext)
        {
            _dbContext = dbContext;
            
            _projectionColumns.AddRange(ModelUtility.GetColumnNames<TModel>());
            _tableName = SqlGenerationUtility.GetTableName<TModel>();

            if (where != null)
            {
                var visitor = new SqlExpressionVisitor<TModel>();
                _wherePredicates.Add(visitor.VisitExpression(where));
            }
        }

        public IWhereQuery<TModel> Where(Expression<Predicate<TModel>> condition)
        {
            var visitor = new SqlExpressionVisitor<TModel>();
            _wherePredicates.Add(visitor.VisitExpression(condition));

            return this;
        }

        public ISelectQuery<TModel> OrderBy<TProperty>(Expression<Func<TModel, TProperty>> column)
        {
            _orderConditions.Add(ModelUtility.GetColumnName(column) + " ASC");
            return this;
        }

        public ISelectQuery<TModel> OrderByDescending<TProperty>(Expression<Func<TModel, TProperty>> column)
        {
            _orderConditions.Add(ModelUtility.GetColumnName(column) + " DESC");
            return this;
        }

        internal override string GetSql()
        {
            string query = string.Format("SELECT {0} FROM {1}", string.Join(", ", _projectionColumns), _tableName);
            if (_wherePredicates.Count > 0)
            {
                query += string.Format(" WHERE {0}", string.Join(" AND ", _wherePredicates));
            }

            if (_orderConditions.Count > 0)
            {
                query += string.Format(" ORDER BY {0}", string.Join(", ", _orderConditions));
            }

            return query;
        }

        protected override Func<IDataReader, TModel> GetMapper(IDataReader dataReader)
        {
            return ModelUtility.GetMapper<TModel>(dataReader);
        }

        public IList<TModel> ToList()
        {
            return Query().ToList();
        }

        public TModel First()
        {
            return Query().First();
        }
    }
}