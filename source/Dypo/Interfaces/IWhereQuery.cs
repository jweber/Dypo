using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dypo.Interfaces
{
    public interface IWhereQuery<TModel> : IQuery<TModel>
    {
        IWhereQuery<TModel> Where(Expression<Predicate<TModel>> condition);
    }
}
