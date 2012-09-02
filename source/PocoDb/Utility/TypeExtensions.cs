using System;

namespace PocoDb.Utility
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Determines whether [is nullable type] [the specified type to check].
        /// </summary>
        /// <param name="typeToCheck">The type to check.</param>
        /// <returns>
        /// 	<c>true</c> if [is nullable type] [the specified type to check]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullableType(this Type typeToCheck)
        {
            return (typeToCheck.IsGenericType && typeToCheck.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static Type GetUnderlyingType(this Type type)
        {
            if (type.IsNullableType())
                return Nullable.GetUnderlyingType(type);

            return type;
        }

        /// <summary>
        /// Determines whether <c>type</c> of the <paramref name="type"/>
        /// is a numeric type (i.e. int, decimal, float, etc.)
        /// </summary>
        /// <param name="type">The obj.</param>
        /// <returns>
        /// 	<c>true</c> if [is numeric type] [the specified obj]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumericType(this Type type)
        {
            Type compare = type.GetUnderlyingType();

            return compare == typeof(short)
                   || compare == typeof(int)
                   || compare == typeof(long)
                   || compare == typeof(decimal)
                   || compare == typeof(float)
                   || compare == typeof(double);
        }
    }
}