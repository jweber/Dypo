using System.Collections.Generic;

namespace PocoDb.Interfaces
{
    public interface IQuery<TModel>
    {
        IEnumerable<TModel> Query();
        IList<TModel> ToList();
    }
}
