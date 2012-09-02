using System;
using System.Linq.Expressions;
using PocoDb.Query;

namespace PocoDb.Interfaces
{
    public interface ISelectQuery<TModel> : IWhereQuery<TModel>
    {
        ISelectQuery<TModel> OrderBy<TProperty>(Expression<Func<TModel, TProperty>> column);
        ISelectQuery<TModel> OrderByDescending<TProperty>(Expression<Func<TModel, TProperty>> column);
    }
}
