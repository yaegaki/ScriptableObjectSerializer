using UnityEngine;

namespace ScriptableObjectSerializer
{
    public interface ISerializer<T> where T : ScriptableObject
    {
        byte[] Serialize(T obj, IFormatter formatter);
        T Deserialize(byte[] obj, IFormatter formatter);
    }

    public static class Serializer
    {
        public static ISerializer<T> GetSerializer<T>()
            where T : ScriptableObject
            => Serializer<T>.Instance;

        public static byte[] Serialize<T>(T obj, IFormatter formatter)
            where T : ScriptableObject
            => GetSerializer<T>().Serialize(obj, formatter);
        
        public static T Deserialize<T>(byte[] bin, IFormatter formatter)
            where T : ScriptableObject
            => GetSerializer<T>().Deserialize(bin, formatter);
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

        public byte[] Serialize(T obj, IFormatter formatter)
        {
            var root = rootValueAccessor.GetValue(obj);
            return formatter.Serialize(root);
        }

        public T Deserialize(byte[] bin, IFormatter formatter)
        {
            var result = ScriptableObject.CreateInstance<T>();
            var root = formatter.Deserialize(bin);
            if (root == null) return result;

            rootValueAccessor.SetValue(result, root);
            return result;
        }
    }
}
