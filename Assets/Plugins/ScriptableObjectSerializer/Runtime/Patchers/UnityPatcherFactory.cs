using System;
using UnityEngine;

namespace ScriptableObjectSerializer.Patchers
{
    /// <summary>
    /// A PatcherFactory of dependent of Unity.
    /// </summary>
    public class UnityPatcherFactory : IPatcherFactory
    {
        public bool IsSerializableType(Type type)
            => typeof(ScriptableObject).IsAssignableFrom(type);

        public IPatcher CreatePatcher(Type type, IPatcherRegistry patcherRegistry)
            => new ScriptableObjectPatcher(type, patcherRegistry);
    }
}

