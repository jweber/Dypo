using System;
using System.Data;
using System.Linq.Expressions;

namespace Dypo.Interfaces
{
    public delegate void ExceptionHandler(Exception ex);

    public interface IDbContext : IDisposable
    {
        event ExceptionHandler OnException;

        IDbConnection DbConnection { get; }

        ISelectQuery<TModel> Select<TModel>(Expression<Predicate<TModel>> where = null);
        IDynamicQuery Select(string query, params object[] parameters);
    }
}
