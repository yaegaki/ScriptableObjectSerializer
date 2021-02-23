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
        public static ISerializer<T> GetSerializer<T>()
            where T : ScriptableObject
            => Serializer<T>.Instance;

        public static byte[] Serialize<T>(T obj)
            where T : ScriptableObject
            => Serialize(obj, JsonFormatterRegistry.Instance);

        public static byte[] Serialize<T>(T obj, IFormatterRegistry formatterRegistry)
            where T : ScriptableObject
            => GetSerializer<T>().Serialize(obj, formatterRegistry);

        public static T Deserialize<T>(byte[] bin)
            where T : ScriptableObject
            => Deserialize<T>(bin, JsonFormatterRegistry.Instance);
        
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

        private readonly IValueAccessor rootValueAccessor;

        private Serializer()
        {
            this.rootValueAccessor = ValueAccessor.Create(typeof(T));
        }

        public byte[] Serialize(T obj, IFormatterRegistry formatterRegistry)
        {
            var root = rootValueAccessor.GetValue(obj);
            return formatterRegistry.GetFormatter<T>().Serialize(root);
        }

        public T Deserialize(byte[] bin, IFormatterRegistry formatterRegistry)
        {
            var result = ScriptableObject.CreateInstance<T>();
            var root = formatterRegistry.GetFormatter<T>().Deserialize(bin);
            if (root == null) return result;

            rootValueAccessor.SetValue(result, root);
            return result;
        }
    }
}
