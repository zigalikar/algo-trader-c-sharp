using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace AlgoTrader.Core.Helpers
{
    public static class ReflectionHelper
    {
        public static IList<Type> GetTypesWithAttribute<T>(string ns, Assembly assembly, Func<T, bool> attributeFilter = null) where T : Attribute
        {
            var types = assembly.GetTypes().Where(t => string.Equals(t.Namespace, ns, StringComparison.Ordinal));
            return types.Where(d =>
            {
                var attr = d.GetCustomAttribute<T>();
                if (attr != null && (attributeFilter == null || attributeFilter.Invoke(attr)))
                    return true;
                return false;
            }).ToList();
        }
    }
}
