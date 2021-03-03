using System;

namespace ScriptableObjectSerializer.Patchers
{
    public interface IPatcherFactory
    {
        bool IsSerializableType(Type type);
        IPatcher CreatePatcher(Type type, IPatcherRegistry patcherRegistry);
    }
}
