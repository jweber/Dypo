using System;
using System.Linq;
using System.Reflection;

namespace Dypo.Utility
{
    /// <summary>
    /// Helper class for dealing with attributes
    /// </summary>
    internal static class AttributeUtility
    {
        /// <summary>
        /// Gets the first occurance of the <typeparam name="TAttribute">attribute</typeparam>, or
        /// <c>null</c> is none;
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="customAttributeProvider"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttribute GetAttribute<TAttribute>(ICustomAttributeProvider customAttributeProvider, bool inherit = true)
            where TAttribute : Attribute
        {
            object[] attributes = customAttributeProvider.GetCustomAttributes(typeof(TAttribute), inherit);
            return (TAttribute)attributes.FirstOrDefault();
        }
    }
}
