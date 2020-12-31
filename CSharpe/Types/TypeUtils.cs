using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharpe.Types
{
    public static class TypeUtils
    {
        private static Dictionary<Type, bool> _unmanagedTypesCache = new Dictionary<Type, bool>();

        public static bool IsUnManaged(this Type t)
        {
            var result = false;
            if (_unmanagedTypesCache.ContainsKey(t))
                return _unmanagedTypesCache[t];
            else if (t.IsPrimitive || t.IsPointer || t.IsEnum)
                result = true;
            else if (t.IsGenericType || !t.IsValueType)
                result = false;
            else
                result = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).All(x => x.FieldType.IsUnManaged());
            _unmanagedTypesCache.Add(t, result);
            return result;
        }
    }
}