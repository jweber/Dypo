using System.Collections.Generic;

namespace Dypo.Interfaces
{
    public interface IDynamicQuery
    {
        IEnumerable<dynamic> Query();
        IList<dynamic> ToList();
        dynamic First();
    }
}
