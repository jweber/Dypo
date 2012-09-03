using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using Dypo.Interfaces;
using Dypo.Select;

namespace Dypo
{
    internal class DbContext : IDbContext
    {
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly string _connectionString;

        private IDbConnection _connection;

        public event ExceptionHandler OnException;

        public DbContext(string connectionStringName)
        {
            if (string.IsNullOrEmpty(connectionStringName))
            {
                connectionStringName = ConfigurationManager.ConnectionStrings[1].Name;
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionStringSettings == null)
                throw new InvalidOperationException("Connection string: " + connectionStringName + " could not be found");

            _connectionString = connectionStringSettings.ConnectionString;

            var providerName = "System.Data.SqlClient";
            if (!string.IsNullOrEmpty(connectionStringSettings.ProviderName))
                providerName = connectionStringSettings.ProviderName;

            _dbProviderFactory = DbProviderFactories.GetFactory(providerName);
        }

        public DbContext(string connectionString, string providerName)
        {
            _connectionString = connectionString;
            _dbProviderFactory = DbProviderFactories.GetFactory(providerName);
        }

        public IDbConnection DbConnection { get { return OpenConnection(); } }

        internal void HandleException(Exception ex)
        {
            if (OnException != null)
                OnException(ex);

            System.Diagnostics.Debug.WriteLine(ex.Message);
        }

        private IDbConnection OpenConnection()
        {
            if (_connection == null)
            {
                _connection = _dbProviderFactory.CreateConnection();
                _connection.ConnectionString = _connectionString;
                _connection.Open();
            }

            return _connection;
        }

        public ISelectQuery<TModel> Select<TModel>(Expression<Predicate<TModel>> where = null)
        {
            return new SelectQuery<TModel>(this, where);
        }

        public IDynamicQuery Select(string query, params object[] parameters)
        {
            return new DynamicSelectQuery(this, query, parameters);
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
                _connection.Close();
        }
    }

}
