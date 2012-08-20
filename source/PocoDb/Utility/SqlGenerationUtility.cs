using System.Reflection;
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

        /// <summary>
        /// Gets the name of the table column. If the <paramref name="propertyInfo"/>
        /// has an <see cref="ColumnNameAttribute"/>, then the value from the attribute
        /// is returned. Otherwise, the name of the property is returned.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static string GetColumnName(PropertyInfo propertyInfo)
        {
            var columnNameAttribute = AttributeUtility.GetAttribute<ColumnNameAttribute>(propertyInfo);
            if (columnNameAttribute != null)
                return columnNameAttribute.ColumnName;

            return propertyInfo.Name;
        }
    }
}
