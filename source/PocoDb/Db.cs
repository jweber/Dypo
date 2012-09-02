using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
