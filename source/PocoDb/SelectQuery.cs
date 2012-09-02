using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using PocoDb.Expressions;
using PocoDb.Interfaces;
using PocoDb.Utility;

namespace PocoDb
{
    internal class SelectQuery<TModel> : ISelectQuery<TModel>
    {
        private readonly IDbContext _dbContext;
        private readonly List<string> _projectionColumns = new List<string>();
        private readonly IList<string> _wherePredicates = new List<string>();

        private readonly string _tableName;

        public SelectQuery(IDbContext dbContext, Expression<Predicate<TModel>> where = null,  string tableName = null)
        {
            _dbContext = dbContext;
            _tableName = SqlGenerationUtility.GetTableName<TModel>(tableName);

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

        public IList<TModel> ToList()
        {
            return Query().ToList();
        }

        public IEnumerable<TModel> Query()
        {
            _projectionColumns.AddRange(ModelUtility.GetColumnNames<TModel>());

            using (var command = _dbContext.DbConnection.CreateCommand())
            {
                command.CommandText = GenerateSql();

                IDataReader reader = command.ExecuteReader();
                var mapper = ModelUtility.GetMapper<TModel>(reader);

                using (reader)
                {
                    while (true)
                    {
                        TModel output;
                        try
                        {
                            if (!reader.Read())
                                yield break;

                            output = mapper(reader);
                        }
                        catch (Exception ex)
                        {
                            var context = _dbContext as DbContext;
                            if (context != null)
                                context.HandleException(ex);

                            throw;
                        }

                        yield return output;
                    }
                }
            }
        }

        private string GenerateSql()
        {
            string query = string.Format("SELECT {0} FROM {1}", string.Join(", ", _projectionColumns), _tableName);
            if (_wherePredicates.Count > 0)
            {
                query += string.Format(" WHERE {0}", string.Join(" AND ", _wherePredicates));
            }

            return query;
        }
    }
}