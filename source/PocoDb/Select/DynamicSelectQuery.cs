using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PocoDb.Interfaces;
using PocoDb.Utility;

namespace PocoDb.Select
{
    internal class DynamicSelectQuery : AbstractSelectQuery<dynamic>, IDynamicQuery
    {
        private static readonly Regex ParameterRegex = new Regex(@"(\@\w+)", RegexOptions.Compiled);

        private readonly IDbContext _dbContext;
        private readonly string _queryText;
        private readonly object[] _sqlParameters;

        private List<object> _processedSqlParameters;

        public DynamicSelectQuery(IDbContext dbContext, string queryText, params object[] parameters)
            : base(dbContext)
        {
            _dbContext = dbContext;
            _queryText = queryText;
            _sqlParameters = parameters;
        }

        protected override IDbCommand CreateCommand()
        {
            var command = base.CreateCommand();
            
            for (int i = 0; i < _processedSqlParameters.Count; i++)
            {
                var p = command.CreateParameter();
                p.Value = _processedSqlParameters[i];
                p.ParameterName = i.ToString(CultureInfo.InvariantCulture);
                
                command.Parameters.Add(p);
            }

            return command;
        }

        internal override string GetSql()
        {
            _processedSqlParameters = new List<object>();
            string processedSql = ProcessQueryParameters();
            
            return processedSql;
        }

        protected override Func<IDataReader, dynamic> GetMapper(IDataReader dataReader)
        {
            return ModelUtility.GetDynamicMapper(dataReader, _queryText);
        }

        public IList<dynamic> ToList()
        {
            return Query().ToList();
        }

        public dynamic First()
        {
            return Query().First();
        }

        private string ProcessQueryParameters()
        {
            return ParameterRegex.Replace(_queryText, m =>
            {
                string parameter = m.Value.Substring(1);
                object parameterValue = null;

                int parameterIndex;
                // index based parameter
                if (int.TryParse(parameter, out parameterIndex))
                {
                    if (parameterIndex > _sqlParameters.Length)
                        throw new ArgumentOutOfRangeException(string.Format("Parameter '@{0}' does not have a value supplied.", parameterIndex));

                    parameterValue = _sqlParameters[parameterIndex];
                }
                // name based parameter
                else
                {
                    parameterValue = (from param in _sqlParameters
                                      let propertyInfo = param.GetType().GetProperty(parameter)
                                      where propertyInfo != null
                                      select propertyInfo.GetValue(param, null)).FirstOrDefault();

                    if (parameterValue == null)
						throw new ArgumentException(string.Format("Parameter '@{0}' does not have a value supplied", parameter));
                }

                var enumerableParameterValue = parameterValue as IEnumerable;
                if (!(parameterValue is string) && enumerableParameterValue != null)
                {
                    var sb = new StringBuilder();
                    foreach (var value in enumerableParameterValue)
                    {
                        sb.AppendFormat("{0}@{1}", sb.Length != 0 ? "," : "", _processedSqlParameters.Count);
                        _processedSqlParameters.Add(value);
                    }
                    return sb.ToString();
                }

                _processedSqlParameters.Add(parameterValue);
                return "@" + (_processedSqlParameters.Count - 1);
            });
        }
    }
}