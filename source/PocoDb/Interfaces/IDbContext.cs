using System.Data;

namespace PocoDb.Interfaces
{
    public interface IDbContext
    {
        IDbConnection DbConnection { get; }
    }
}
