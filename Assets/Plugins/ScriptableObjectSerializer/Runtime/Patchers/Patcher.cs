using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace ScriptableObjectSerializer.Patchers
{
    public interface IPatcher
    {
        void PatchTo(ref object obj, IObjectNode patch);
        IObjectNode PatchFrom(object obj, string name);
    }

    public static class Patcher
    {
        public static IPatcher Create<T>(IPatcherRegistry patcherRegistry)
            => Create(typeof(T), patcherRegistry);

        public static IPatcher Create(Type type, IPatcherRegistry patcherRegistry)
            => new ComplexPatcher(type, patcherRegistry);
    }
}
