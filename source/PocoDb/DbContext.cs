﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PocoDb.Interfaces;
using PocoDb.Providers.SqlServer;

namespace PocoDb
{
    internal class DbContext : IDbContext
    {
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly ConnectionStringSettings _connectionStringSettings;

        private IDbConnection _connection;

        public event ExceptionHandler OnException;

        public DbContext(string connectionStringName)
        {
            _connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (_connectionStringSettings == null)
                throw new InvalidOperationException("Connection string: " + connectionStringName + " could not be found");

            var providerName = "System.Data.SqlClient";
            if (!string.IsNullOrEmpty(_connectionStringSettings.ProviderName))
                providerName = _connectionStringSettings.ProviderName;

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
                _connection.ConnectionString = _connectionStringSettings.ConnectionString;
                _connection.Open();
            }

            return _connection;
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }

}
