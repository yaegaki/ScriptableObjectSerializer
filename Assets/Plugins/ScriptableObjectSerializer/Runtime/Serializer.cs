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
        public static readonly IFormatterRegistry DefaultFormatterRegistry = JsonFormatterRegistry.Instance;

        public static ISerializer<T> GetSerializer<T>()
            where T : ScriptableObject
            => Serializer<T>.Instance;

        public static byte[] Serialize<T>(T obj)
            where T : ScriptableObject
            => Serialize(obj, DefaultFormatterRegistry);

        public static byte[] Serialize<T>(T obj, IFormatterRegistry formatterRegistry)
            where T : ScriptableObject
            => GetSerializer<T>().Serialize(obj, formatterRegistry);

        public static T Deserialize<T>(byte[] bin)
            where T : ScriptableObject
            => Deserialize<T>(bin, DefaultFormatterRegistry);
        
        public static T Deserialize<T>(byte[] bin, IFormatterRegistry formatterRegistry)
            where T : ScriptableObject
            => GetSerializer<T>().Deserialize(bin, formatterRegistry);
    }

    class Serializer<T> : ISerializer<T>
        where T : ScriptableObject
    {
        public static readonly Serializer<T> Instance;

        static Serializer()
        {
            Instance = new Serializer<T>();
        }

        private readonly IPatcher patcher;


        private Serializer()
            => this.patcher = Patcher.Create<T>();

        public byte[] Serialize(T obj, IFormatterRegistry formatterRegistry)
        {
            var patch = this.patcher.PatchFrom(obj, ":Root:");
            return formatterRegistry.GetFormatter<T>().Serialize(patch);
        }

        public T Deserialize(byte[] bin, IFormatterRegistry formatterRegistry)
        {
            var patch = formatterRegistry.GetFormatter<T>().Deserialize(bin);
            var instance = ScriptableObject.CreateInstance<T>();
            var obj = (object)instance;
            this.patcher.PatchTo(ref obj, patch);
            return instance;
        }
    }
}
