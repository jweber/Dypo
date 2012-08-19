using PocoDb.Attributes;

namespace PocoDb.Utility
{
    /// <summary>
    /// Helper class for generalized SQL generation
    /// </summary>
    internal static class SqlGenerationUtility
    {
        /// <summary>
        /// Gets the name of the table. If <paramref name="tableName"/> is <c>null</c>,
        /// then the table name is derived from either usage of the <see cref="TableNameAttribute"/>
        /// or simply the name of <typeparamref name="TTable"/>
        /// </summary>
        /// <typeparam name="TTable"></typeparam>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string GetTableName<TTable>(string tableName = null)
        {
            if (!string.IsNullOrWhiteSpace(tableName))
                return tableName;

            var tableNameAttribute = AttributeUtility.GetAttribute<TableNameAttribute>(typeof(TTable));
            if (tableNameAttribute != null)
                return tableNameAttribute.TableName;

            return typeof(TTable).Name;
        }
    }
}
