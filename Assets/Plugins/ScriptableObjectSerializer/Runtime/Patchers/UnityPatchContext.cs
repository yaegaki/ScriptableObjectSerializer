using System.Collections.Generic;

namespace ScriptableObjectSerializer.Patchers
{
    public class UnityPatchContext
    {
        private Dictionary<object, int> objectToId = new Dictionary<object, int>();
        private Dictionary<int, object> idToObject = new Dictionary<int, object>();
        private int nextId = 1;

        public int FindReferenceId(object obj)
        {
            if (!this.objectToId.TryGetValue(obj, out var id))
            {
                return -1;
            }

            return id;
        }

        public int Register(object obj)
        {
            var id = this.nextId;
            this.nextId++;
            this.objectToId[obj] = id;
            return id;
        }

        public object FindObject(int id)
        {
            this.idToObject.TryGetValue(id, out var obj);
            return obj;
        }

        public void Register(int id, object obj)
        {
            this.idToObject[id] = obj;
        }
    }
}
