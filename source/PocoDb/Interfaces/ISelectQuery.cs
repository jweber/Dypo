using System;
using System.Linq.Expressions;

namespace PocoDb.Interfaces
{
    public interface ISelectQuery<TModel> : IWhereQuery<TModel>
    {
    }
}
