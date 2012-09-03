using System;
using System.Linq.Expressions;
using Dypo.Query;

namespace Dypo.Interfaces
{
    public interface ISelectQuery<TModel> : IWhereQuery<TModel>
    {
        ISelectQuery<TModel> OrderBy<TProperty>(Expression<Func<TModel, TProperty>> column);
        ISelectQuery<TModel> OrderByDescending<TProperty>(Expression<Func<TModel, TProperty>> column);
    }
}
