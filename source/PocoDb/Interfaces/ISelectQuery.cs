using System.Collections.Generic;

namespace PocoDb.Interfaces
{
    public interface ISelectQuery<TTable> : IQuery<TTable>
    {
        IEnumerable<TTable> Execute();
    }
}
