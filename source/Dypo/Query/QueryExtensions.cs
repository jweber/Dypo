using System.Linq;

namespace Dypo.Query
{
    public static class QueryExtensions
    {
        public static bool In<T>(this T value, params T[] arguments)
        {
            return arguments
                .Any(arg => value.ToString() == arg.ToString());
        }

        public static T Coalesce<T>(this T value, params T[] arguments)
        {
            return value;
        }

        public static T Sum<T>(this T value)
        {
            return value;
        }

        public static T As<T>(this T value, string alias)
        {
            return value;
        }
    }
}
