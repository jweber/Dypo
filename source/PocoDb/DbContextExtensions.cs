using System;
using System.Linq.Expressions;
using PocoDb.Interfaces;
using PocoDb.Query;
using PocoDb.Select;

namespace PocoDb
{
    public static class DbContextExtensions
    {
        public static ISelectQuery<TModel> Select<TModel>(this IDbContext dbContext, Expression<Predicate<TModel>> where = null)
        {
            return new SelectQuery<TModel>(dbContext, where);
        }

        public static IDynamicQuery Select(this IDbContext dbContext, string query, params object[] parameters)
        {
            return new DynamicSelectQuery(dbContext, query, parameters);
        }
    }
}
