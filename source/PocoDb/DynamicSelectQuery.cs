using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PocoDb.Interfaces;
using PocoDb.Utility;

namespace PocoDb
{
    class DynamicSelectQuery : IDynamicQuery
    {
        private readonly IDbContext _dbContext;
        private readonly string _queryText;

        public DynamicSelectQuery(IDbContext dbContext, string queryText)
        {
            _dbContext = dbContext;
            _queryText = queryText;
        }

        public IList<dynamic> ToList()
        {
            return Query().ToList();
        }

        public dynamic First()
        {
            return Query().First();
        }

        public IEnumerable<dynamic> Query()
        {
            using (var command = _dbContext.DbConnection.CreateCommand())
            {
                command.CommandText = _queryText;

                IDataReader reader = command.ExecuteReader();
                Func<IDataReader, dynamic> mapper = ModelUtility.GetDynamicMapper(reader, _queryText);

                using (reader)
                {
                    while (true)
                    {
                        dynamic output;
                        try
                        {
                            if (!reader.Read())
                                yield break;

                            output = mapper(reader);
                        }
                        catch (Exception ex)
                        {
                            var context = _dbContext as DbContext;
                            if (context != null)
                                context.HandleException(ex);

                            throw;
                        }

                        yield return output;
                    }
                }
            }
        }
    }
}