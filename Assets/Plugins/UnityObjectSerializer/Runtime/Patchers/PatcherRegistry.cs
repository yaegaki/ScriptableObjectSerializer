using System;
using System.Linq;

namespace ScriptableObjectSerializer.Patchers
{
    public interface IPatcherRegistry
    {
        IPatcherFactory FindFactory(Type type);
        void UseContext(PatchContext context);
    }

    public static class PatcherRegistryExtensions
    {
        public static IPatcher CreatePatcher(this IPatcherRegistry registry, Type type)
            => registry.FindFactory(type)?.CreatePatcher(type, registry);
    }

    public class PatcherRegistry : IPatcherRegistry
    {
        public static readonly PatcherRegistry Instance = new PatcherRegistry();

        private readonly IPatcherFactory[] patcherFactories = new IPatcherFactory[]
        {
            new SystemPatcherFactory(),
            new UnityPatcherFactory(),
        };

        public IPatcherFactory FindFactory(Type type)
            => patcherFactories.FirstOrDefault(f => f.IsSerializableType(type));
        
        public void UseContext(PatchContext context)
        {
            foreach (var f in this.patcherFactories)
            {
                f.UseContext(context);
            }
        }
    }
}
