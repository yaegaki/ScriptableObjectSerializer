using ScriptableObjectSerializer.Patchers;
using UnityEngine;

namespace ScriptableObjectSerializer
{
    public interface ISerializer<T> where T : ScriptableObject
    {
        byte[] Serialize(T obj, IFormatterRegistry formatterRegistry);
        T Deserialize(byte[] obj, IFormatterRegistry formatterRegistry);
    }

    public static class Serializer
    {
        public static readonly IPatcherRegistry DefaultPatcherRegistry = PatcherRegistry.Instance;
        public static readonly IFormatterRegistry DefaultFormatterRegistry = JsonFormatterRegistry.Instance;

        public static ISerializer<T> GetSerializer<T>(IPatcherRegistry patcherRegistry)
            where T : ScriptableObject
        {
            if (patcherRegistry == DefaultPatcherRegistry)
            {
                return Serializer<T>.Default;
            }

            return new Serializer<T>(patcherRegistry);
        }

        public static byte[] Serialize<T>(T obj)
            where T : ScriptableObject
            => Serialize(obj, DefaultPatcherRegistry, DefaultFormatterRegistry);

        public static byte[] Serialize<T>(T obj, IPatcherRegistry patcherRegistry, IFormatterRegistry formatterRegistry)
            where T : ScriptableObject
            => GetSerializer<T>(patcherRegistry).Serialize(obj, formatterRegistry);

        public static T Deserialize<T>(byte[] bin)
            where T : ScriptableObject
            => Deserialize<T>(bin, DefaultPatcherRegistry, DefaultFormatterRegistry);
        
        public static T Deserialize<T>(byte[] bin, IPatcherRegistry patcherRegistry, IFormatterRegistry formatterRegistry)
            where T : ScriptableObject
            => GetSerializer<T>(patcherRegistry).Deserialize(bin, formatterRegistry);
    }

    public class Serializer<T> : ISerializer<T>
        where T : ScriptableObject
    {
        public static readonly Serializer<T> Default = new Serializer<T>(PatcherRegistry.Instance);

        private readonly IPatcherRegistry patcherRegistry;
        private readonly IPatcher patcher;

        public Serializer(IPatcherRegistry patcherRegistry)
        {
            this.patcherRegistry = patcherRegistry;
            this.patcher = patcherRegistry.CreatePatcher(typeof(T));
        }

        public byte[] Serialize(T obj, IFormatterRegistry formatterRegistry)
        {
            var context = new PatchContext();
            patcherRegistry.UseContext(context);
            var patch = this.patcher.PatchFrom(context, obj, ":Root:");
            return formatterRegistry.GetFormatter<T>().Serialize(patch);
        }

        public T Deserialize(byte[] bin, IFormatterRegistry formatterRegistry)
        {
            var patch = formatterRegistry.GetFormatter<T>().Deserialize(bin);
            var instance = ScriptableObject.CreateInstance<T>();
            var obj = (object)instance;
            var context = new PatchContext();
            patcherRegistry.UseContext(context);
            this.patcher.PatchTo(context, ref obj, patch);
            return instance;
        }
    }
}
