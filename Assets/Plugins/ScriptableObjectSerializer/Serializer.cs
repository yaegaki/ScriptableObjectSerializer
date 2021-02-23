using UnityEngine;

namespace ScriptableObjectSerializer
{
    public interface ISerializer<T> where T : ScriptableObject
    {
        byte[] Serialize(T obj, IFormatterRegistry formatterRegistry);
        T Deserialize(byte[] obj, IFormatterRegistry formatterRegistry);

        byte[] PatchFrom(T obj, string path, IFormatterRegistry formatterRegistry);
        void PatchTo(T obj, byte[] patch, IFormatterRegistry formatterRegistry);
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

        public static byte[] PatchFrom<T>(T obj, string path)
            where T : ScriptableObject
            => GetSerializer<T>().PatchFrom(obj, path, DefaultFormatterRegistry);

        public static byte[] PatchFrom<T>(T obj, string path, IFormatterRegistry formatterRegistry)
            where T : ScriptableObject
            => GetSerializer<T>().PatchFrom(obj, path, formatterRegistry);

        public static void PatchTo<T>(T obj, byte[] patch)
            where T : ScriptableObject
            => GetSerializer<T>().PatchTo(obj, patch, DefaultFormatterRegistry);

        public static void PatchTo<T>(T obj, byte[] patch, IFormatterRegistry formatterRegistry)
            where T : ScriptableObject
            => GetSerializer<T>().PatchTo(obj, patch, formatterRegistry);
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
            return PatchFrom(obj, "", formatterRegistry);
        }

        public T Deserialize(byte[] bin, IFormatterRegistry formatterRegistry)
        {
            var result = ScriptableObject.CreateInstance<T>();
            PatchTo(result, bin, formatterRegistry);
            return result;
        }

        public byte[] PatchFrom(T obj, string path, IFormatterRegistry formatterRegistry)
        {
            var patch = rootValueAccessor.GetValue(obj);
            if (!string.IsNullOrEmpty(path))
            {
                patch = patch.CreatePatch(path);
            }
            return formatterRegistry.GetFormatter<T>().Serialize(patch);
        }

        public void PatchTo(T obj, byte[] patch, IFormatterRegistry formatterRegistry)
        {
            var root = formatterRegistry.GetFormatter<T>().Deserialize(patch);
            if (root == null) return;
            rootValueAccessor.SetValue(obj, root);
        }
    }
}
