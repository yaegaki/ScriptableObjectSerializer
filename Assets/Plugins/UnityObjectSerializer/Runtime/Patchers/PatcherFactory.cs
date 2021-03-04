using System;

namespace UnityObjectSerializer.Patchers
{
    public interface IPatcherFactory
    {
        bool IsSerializableType(Type type);
        IPatcher CreatePatcher(Type type, IPatcherRegistry patcherRegistry);
        void UseContext(PatchContext context);
    }
}
