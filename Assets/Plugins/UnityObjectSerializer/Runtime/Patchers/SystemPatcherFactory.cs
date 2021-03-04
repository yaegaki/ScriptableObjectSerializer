using System;

namespace UnityObjectSerializer.Patchers
{
    /// <summary>
    /// A PatcherFactory of independent of Unity.
    /// </summary>
    public class SystemPatcherFactory : IPatcherFactory
    {
        public bool IsSerializableType(Type type)
            => SerializeHelper.IsSerializableType(type);

        public IPatcher CreatePatcher(Type type, IPatcherRegistry patcherRegistry)
        {
            var nodeType = type.ToNodeType();
            if (type.IsArray || type.IsGenericType)
            {
                return new ListPatcher(type, nodeType, patcherRegistry);
            }

            if (nodeType == NodeType.Complex)
            {
                return new ComplexPatcher(type, patcherRegistry);
            }

            return new PrimitivePatcher(type, nodeType);
        }

        public void UseContext(PatchContext context)
        {
        }
    }
}
