using System;
using System.Collections.Generic;

namespace ScriptableObjectSerializer.Patchers
{
    public class PatchContext
    {
        private readonly Dictionary<Type, object> values = new Dictionary<Type, object>();

        public void Use<T>(T value)
        {
            if (this.values.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"{typeof(T).Name} is already in use.");
            }

            this.values[typeof(T)] = value;
        }

        public T Get<T>()
        {
            if (!this.values.TryGetValue(typeof(T), out var obj))
            {
                throw new InvalidOperationException($"{typeof(T).Name} is not used.");
            }
            return (T)obj;
        }
    }
}
