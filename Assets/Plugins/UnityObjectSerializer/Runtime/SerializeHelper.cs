using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityObjectSerializer
{
    public static class SerializeHelper
    {
        public static bool IsSerializeField(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPublic)
            {
                return fieldInfo.GetCustomAttribute<NonSerializedAttribute>() == null;
            }

            return fieldInfo.GetCustomAttribute<SerializeField>() != null;
        }

        public static bool IsSerializableType(Type type)
            => IsSerializableType(type, false);


        public static bool IsSerializableType(Type type, bool isTypeArg)
        {
            if (type.GetCustomAttribute<SerializableAttribute>() == null) return false;

            if (type.IsGenericType)
            {
                // Can't serialize nested generic type
                // ex) List<List<int>>
                if (isTypeArg) return false;

                if (type.GetGenericTypeDefinition() != typeof(List<>)) return false;
                var typeArg = type.GenericTypeArguments[0];
                if (!IsSerializableType(typeArg, true)) return false;
            }

            if (type.IsArray)
            {
                if (isTypeArg) return false;
                var elemType = type.GetElementType();
                if (!IsSerializableType(elemType, true)) return false;
            }

            return true;
        }

        public static bool MatchType<T>(Type type)
            => typeof(T) == type || typeof(T[]) == type || typeof(List<T>) == type;

        public static bool MatchTypeForClass<T>(Type type)
        {
            if (typeof(T).IsAssignableFrom(type)) return true;
            if (type.IsArray)
            {
                return typeof(T).IsAssignableFrom(type.GetElementType());
            }
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() != typeof(List<>)) return false;
                return typeof(T).IsAssignableFrom(type.GenericTypeArguments[0]);
            }

            return false;
        }
    }
}
