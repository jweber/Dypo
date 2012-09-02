using System;
using System.Collections.Generic;
using System.Data;
using PocoDb.Interfaces;

namespace PocoDb.Select
{
    internal abstract class AbstractSelectQuery<TModel>
    {
        private readonly IDbContext _dbContext;

        protected AbstractSelectQuery(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        internal abstract string GetSql();
        protected abstract Func<IDataReader, TModel> GetMapper(IDataReader dataReader);
        
        protected virtual IDbCommand CreateCommand()
        {
            var command = _dbContext.DbConnection.CreateCommand();
            command.CommandText = GetSql();
            
            return command;
        }

        public IEnumerable<TModel> Query()
        {
            using (var command = CreateCommand())
            {
                IDataReader reader = command.ExecuteReader();
                var mapper = GetMapper(reader);

                using (reader)
                {
                    while (true)
                    {
                        TModel output;
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
