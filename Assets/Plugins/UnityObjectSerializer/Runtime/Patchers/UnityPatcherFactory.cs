using System;
using UnityEngine;

namespace UnityObjectSerializer.Patchers
{
    /// <summary>
    /// A PatcherFactory of dependent of Unity.
    /// </summary>
    public class UnityPatcherFactory : IPatcherFactory
    {
        public bool IsSerializableType(Type type)
        {
            if (Match<Vector2>(type)) return true;
            if (Match<Vector3>(type)) return true;
            if (Match<Vector4>(type)) return true;
            if (Match<Quaternion>(type)) return true;
            if (Match<Vector2Int>(type)) return true;
            if (Match<Vector3Int>(type)) return true;

            return SerializeHelper.MatchTypeForClass<ScriptableObject>(type);
        }

        public IPatcher CreatePatcher(Type type, IPatcherRegistry patcherRegistry)
        {
            if (type.IsArray || type.IsGenericType)
            {
                return new ListPatcher(type, NodeType.Complex, patcherRegistry);
            }

            if (typeof(Vector2) == type ||
                typeof(Vector3) == type ||
                typeof(Vector4) == type ||
                typeof(Quaternion) == type)
            {
                return new ComplexPatcher(type, patcherRegistry);
            }

            if (typeof(Vector2Int) == type || typeof(Vector3Int) == type)
            {
                return new VectorIntPatcher(type, typeof(Vector2Int) == type ? 2 : 3);
            }

            return new ScriptableObjectPatcher(type, patcherRegistry);
        }

        public void UseContext(PatchContext context)
        {
            context.Use<UnityPatchContext>(new UnityPatchContext());
        }

        private static bool Match<T>(Type t)
            => SerializeHelper.MatchType<T>(t);
    }
}
