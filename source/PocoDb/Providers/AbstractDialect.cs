using System;
using PocoDb.Utility;

namespace PocoDb.Providers
{
    internal abstract class AbstractDialect
    {
        public virtual string QuoteValue(object value, Type valueType)
        {
            if (value == null)
                return "NULL";

            if (TypeShouldBeQuoted(valueType))
                return "'" + EscapeValue(value) + "'";

            return value.ToString();
        }

        public virtual string UnQuoteValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            if (value.StartsWith("'") && value.EndsWith("'"))
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
        }

        protected virtual bool TypeShouldBeQuoted(Type valueType)
        {
            Type underlyingType = valueType.GetUnderlyingType();

            return !underlyingType.IsNumericType()
                   && underlyingType != typeof(bool);
        }

        protected virtual string EscapeValue(object value)
        {
            if (value == null)
                return string.Empty;

            return value.ToString().Replace("'", "''");
        }
    }
}
