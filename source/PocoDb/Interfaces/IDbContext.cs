using System;
using System.Data;

namespace PocoDb.Interfaces
{
    public delegate void ExceptionHandler(Exception ex);

    public interface IDbContext
    {
        event ExceptionHandler OnException;

        IDbConnection DbConnection { get; }
    }
}
