using PocoDb.Interfaces;

namespace PocoDb
{
    public static class DbContextExtensions
    {
        public static ISelectQuery<TTable> Select<TTable>(this IDbContext dbContext, string tableName = null)
        {
            return new SelectQuery<TTable>(dbContext, tableName);
        }
    }
}
