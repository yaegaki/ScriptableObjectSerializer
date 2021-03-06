using System;
using System.Collections;
using System.Reflection;

namespace UnityObjectSerializer
{
    public static class ReflectionHelper
    {
        public static object CreateInstance(Type type)
        {
            if (type == typeof(string)) return "";
            return Activator.CreateInstance(type);
        }

        public static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return CreateInstance(type);
            }

            return null;
        }

        public static IList CreateArrayOrList(Type type, int capacity)
        {
            var list = Activator.CreateInstance(type, capacity) as IList;
            if (type.IsArray) return list;

            var elemType = type.GenericTypeArguments[0];
            var defaultValue = GetDefaultValue(elemType);
            for (var i = 0; i < capacity; i++)
            {
                list.Add(defaultValue);
            }

            return list;
        }

        public static FieldInfo[] GetAllFields(Type type)
        {
            var flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            return type.GetFields(flags);
        }
    }
}
