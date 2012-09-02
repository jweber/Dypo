using PocoDb.Interfaces;

namespace PocoDb
{
    public static class Db
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionStringName">
        /// If <c>null</c>, then we will use the first connection string present in the configuration</param>
        /// <returns></returns>
        public static IDbContext Connect(string connectionStringName = null)
        {
            return new DbContext(connectionStringName);
        }

        public static IDbContext Connect(string connectionString, string providerName)
        {
            return new DbContext(connectionString, providerName);
        }
    }
}
