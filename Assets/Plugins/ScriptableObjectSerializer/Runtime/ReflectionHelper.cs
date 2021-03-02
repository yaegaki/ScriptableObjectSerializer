using System;
using System.Collections;

namespace ScriptableObjectSerializer
{
    public static class ReflectionHelper
    {
        public static object CreateInstance(Type type)
        {
            if (type == typeof(string)) return "";
            return Activator.CreateInstance(type);
        }

        public static IList CreateArrayOrList(Type type, int capacity)
        {
            var list = Activator.CreateInstance(type, capacity) as IList;
            if (list == null) return null;
            if (type.IsArray)
            {
                var elemType = type.GetElementType();
                for (var i = 0; i < capacity; i++)
                {
                    list[i] = CreateInstance(elemType);
                }
            }
            else
            {
                var elemType = type.GenericTypeArguments[0];
                for (var i = 0; i < capacity; i++)
                {
                    list.Add(CreateInstance(elemType));
                }
            }

            return list;
        }
    }
}
