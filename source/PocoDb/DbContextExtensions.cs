using System;
using System.Linq.Expressions;
using PocoDb.Interfaces;

namespace PocoDb
{
    public static class DbContextExtensions
    {
        public static ISelectQuery<TModel> Select<TModel>(this IDbContext dbContext, Expression<Predicate<TModel>> where = null, string tableName = null)
        {
            return new SelectQuery<TModel>(dbContext, where, tableName);
        }
    }
}
