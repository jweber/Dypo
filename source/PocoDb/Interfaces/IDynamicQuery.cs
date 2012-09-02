using System.Collections.Generic;

namespace PocoDb.Interfaces
{
    public interface IDynamicQuery
    {
        IEnumerable<dynamic> Query();
        IList<dynamic> ToList();
        dynamic First();
    }
}
