using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScriptableObjectSerializer
{
    public interface IFormatterRegistry
    {
        IFormatter GetFormatter<T>();
    }

    public interface IFormatter
    {
        byte[] Serialize(IObjectNode obj);
        IObjectNode Deserialize(byte[] bin);
    }

    public class JsonFormatterRegistry : IFormatterRegistry
    {
        public static readonly JsonFormatterRegistry Instance = new JsonFormatterRegistry();

        private JsonFormatter formatter = new JsonFormatter();
        public IFormatter GetFormatter<T>() => formatter;
    }

    class JsonFormatter : IFormatter
    {
        private readonly static string RootName = ":Root:";

        public byte[] Serialize(IObjectNode obj)
        {
            var json = JsonUtility.ToJson(ToEntry(obj));
            return Encoding.UTF8.GetBytes(json);
        }

        public IObjectNode Deserialize(byte[] bin)
        {
            var json = Encoding.UTF8.GetString(bin);
            var entry = JsonUtility.FromJson<RootEntry>(json);
            return FromEntry(entry);
        }

        private IObjectNode FromEntry(RootEntry entry)
        {
            if (entry.entries == null) return null;

            var entries = entry.entries
                .Select(e => new ComplexEntryWithPath
                {
                    path = e.n.Split('/'),
                    entry = e,
                });

            return Grouping(entries, 0).FirstOrDefault();
        }

        /// <summary>
        /// FlattenHierarchy to TreeHierarchy
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private IEnumerable<IObjectNode> Grouping(IEnumerable<ComplexEntryWithPath> entries, int depth)
        {
            return entries
                .Where(e => e.path.Length > depth)
                .GroupBy(e => e.path[depth])
                .Select(g =>
                {
                    var group = g.ToArray();
                    var nextDepth = depth + 1;
                    var currentEntry = group.FirstOrDefault(e => e.path.Length <= nextDepth).entry;
                    var list = new List<IObjectNode>();
                    if (currentEntry != null)
                    {
                        if (currentEntry.nil)
                        {
                            return new ComplexObjectNode(g.Key, false, 0, true, null);
                        }

                        if (currentEntry.i32 != null)
                        {
                            list.AddRange(currentEntry.i32.Select(e => new PrimitiveObjectNode(NodeType.Int, e.n, e.v)));
                        }
                    }

                    list.AddRange(Grouping(group.Where(e => e.entry != currentEntry), nextDepth));
                    list.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
                    return new ComplexObjectNode(g.Key, false, 0, false, list);
                });
        }


        private RootEntry ToEntry(IObjectNode obj)
        {
            var entries = new List<ComplexEntry>();
            ToEntry(entries, null, obj);
            return new RootEntry
            {
                entries = entries.ToArray(),
            };
        }

        private void ToEntry(List<ComplexEntry> entries, ComplexEntry parent, IObjectNode obj)
        {
            switch (obj.Type)
            {
                case NodeType.Int:
                    if (parent.i32 == null) parent.i32 = new List<IntEntry>();
                    parent.i32.Add(new IntEntry
                    {
                        n = obj.Name,
                        v = (int)obj.Value,
                    });
                    break;
                case NodeType.Complex:
                    var name = parent == null ? RootName : (parent.n + "/" + obj.Name);
                    var self = new ComplexEntry
                    {
                        n = name,
                    };
                    entries.Add(self);
                    foreach (var n in obj.Children)
                    {
                        ToEntry(entries, self, n);
                    }
                    break;
            }
        }

        struct ComplexEntryWithPath
        {
            public string[] path;
            public ComplexEntry entry;
        }

        [Serializable]
        struct RootEntry
        {
            public ComplexEntry[] entries;
        }

        [Serializable]
        class ComplexEntry
        {
            /// <summary>
            /// Name
            /// </summary>
            public string n;
            /// <summary>
            /// IsNull(nil)
            /// </summary>
            public bool nil;
            /// <summary>
            /// IntValues
            /// </summary>
            public List<IntEntry> i32;
        }


        [Serializable]
        struct IntEntry
        {
            /// <summary>
            /// Name
            /// </summary>
            public string n;
            /// <summary>
            /// Value
            /// </summary>
            public int v;
        }
    }
}