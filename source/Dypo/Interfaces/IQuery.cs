using System.Collections.Generic;

namespace Dypo.Interfaces
{
    public interface IQuery<TModel>
    {
        IEnumerable<TModel> Query();
        IList<TModel> ToList();
        TModel First();
    }
}
