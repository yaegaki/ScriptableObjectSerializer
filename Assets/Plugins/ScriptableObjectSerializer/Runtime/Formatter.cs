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
                .OrderBy(e => e.n, StringComparer.Ordinal)
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
                        void AddPrimitiveNode<T>(List<T> entry, NodeType nodeType) where T : IValueEntry
                        {
                            if (entry == null) return;
                            list.AddRange(entry.Select(e => new PrimitiveObjectNode(nodeType, e.Name, e.Value)));
                        }

                        void AddListNodes<T>(List<T> entry, NodeType nodeType) where T : IListEntry
                        {
                            if (entry == null) return;
                            var children = entry.Select(e =>
                            {
                                var v = e.Value;
                                if (e.IsNull || v == null)
                                {
                                    return new ComplexObjectNode(nodeType, e.Name, 0, true, null);
                                }

                                var elemNodes = v.Select(ee => new PrimitiveObjectNode(nodeType, ee.Name, ee.Value)).OrderBy(n => n.Name, StringComparer.Ordinal);
                                return new ComplexObjectNode(nodeType, e.Name, e.Count, false, elemNodes);
                            });
                            list.AddRange(children);
                        }

                        AddPrimitiveNode(currentEntry.i32, NodeType.Int);
                        AddPrimitiveNode(currentEntry.s, NodeType.String);
                        AddListNodes(currentEntry.i32a, NodeType.Int);
                        AddListNodes(currentEntry.sa, NodeType.String);
                    }

                    list.AddRange(Grouping(group.Where(e => e.entry != currentEntry), nextDepth));
                    list.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
                    var firstEntry = group[0];
                    if (currentEntry == null)
                    {
                        return new ComplexObjectNode(g.Key, false, list);
                    }
                    else if (currentEntry.list)
                    {
                        return new ComplexObjectNode(NodeType.Complex, g.Key, currentEntry.listc, currentEntry.nil, list);
                    }
                    else
                    {
                        return new ComplexObjectNode(g.Key, currentEntry.nil, list);
                    }
                });
        }


        private RootEntry ToEntry(IObjectNode obj)
        {
            var entries = new List<ComplexEntry>();
            ToEntry(entries, null, obj);
            return new RootEntry
            {
                entries = entries,
            };
        }

        private void ToEntry(List<ComplexEntry> entries, ComplexEntry parent, IObjectNode obj)
        {
            if (obj.IsList)
            {
                ToEntryForList(entries, parent, obj);
                return;
            }

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
                case NodeType.String:
                    if (parent.s == null) parent.s = new List<StringEntry>();
                    parent.s.Add(new StringEntry
                    {
                        n = obj.Name,
                        v = (string)obj.Value,
                    });
                    break;
                case NodeType.Complex:
                    var name = parent == null ? RootName : (parent.n + "/" + obj.Name);
                    var self = new ComplexEntry
                    {
                        n = name,
                        nil = obj.IsNull,
                    };
                    entries.Add(self);
                    foreach (var n in obj.Children)
                    {
                        ToEntry(entries, self, n);
                    }
                    break;
            }
        }

        private void ToEntryForList(List<ComplexEntry> entries, ComplexEntry parent, IObjectNode obj)
        {
            switch (obj.Type)
            {
                case NodeType.Int:
                    if (parent.i32a == null) parent.i32a = new List<IntListEntry>();
                    parent.i32a.Add(new IntListEntry
                    {
                        n = obj.Name,
                        c = obj.ListCount,
                        nil = obj.IsNull,
                        v = obj.Children.Select(c => new IntEntry
                        {
                            n = c.Name,
                            v = (int)c.Value,
                        }).ToList(),
                    });
                    break;
                case NodeType.String:
                    if (parent.sa == null) parent.sa = new List<StringListEntry>();
                    parent.sa.Add(new StringListEntry
                    {
                        n = obj.Name,
                        c = obj.ListCount,
                        nil = obj.IsNull,
                        v = obj.Children.Select(c => new StringEntry
                        {
                            n = c.Name,
                            v = (string)c.Value,
                        }).ToList(),
                    });
                    break;
                case NodeType.Complex:
                    var name = parent == null ? RootName : (parent.n + "/" + obj.Name);
                    var self = new ComplexEntry
                    {
                        n = name,
                        nil = obj.IsNull,
                        list = obj.IsList,
                        listc = obj.ListCount,
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
            public List<ComplexEntry> entries;
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
            /// IsList
            /// </summary>
            public bool list;

            /// <summary>
            /// ListCount
            /// </summary>
            public int listc;

            /// <summary>
            /// IntValues
            /// </summary>
            public List<IntEntry> i32;

            /// <summary>
            /// IntList
            /// </summary>
            public List<IntListEntry> i32a;

            /// <summary>
            /// StringValues
            /// </summary>
            public List<StringEntry> s;

            /// <summary>
            /// StringList
            /// </summary>
            public List<StringListEntry> sa;
        }

        interface IValueEntry
        {
            string Name { get; }
            object Value { get; }
        }


        [Serializable]
        struct IntEntry : IValueEntry
        {
            /// <summary>
            /// Name
            /// </summary>
            public string n;

            /// <summary>
            /// Value
            /// </summary>
            public int v;

            public string Name => n;
            public object Value => v;
        }

        interface IListEntry
        {
            string Name { get; }
            int Count { get; }
            bool IsNull { get; }
            IEnumerable<IValueEntry> Value { get; }
        }

        [Serializable]
        struct IntListEntry : IListEntry
        {
            /// <summary>
            /// Name
            /// </summary>
            public string n;

            /// <summary>
            /// Count
            /// </summary>
            public int c;

            /// <summary>
            /// IsNull(nil)
            /// </summary>
            public bool nil;

            /// <summary>
            /// Value
            /// </summary>
            public List<IntEntry> v;

            public string Name => n;
            public int Count => c;
            public bool IsNull => nil;
            public IEnumerable<IValueEntry> Value => v?.OfType<IValueEntry>();
        }

        [Serializable]
        struct StringEntry : IValueEntry
        {
            /// <summary>
            /// Name
            /// </summary>
            public string n;

            /// <summary>
            /// Value
            /// </summary>
            public string v;

            public string Name => n;
            public object Value => v;
        }

        [Serializable]
        struct StringListEntry : IListEntry
        {
            /// <summary>
            /// Name
            /// </summary>
            public string n;

            /// <summary>
            /// Count
            /// </summary>
            public int c;

            /// <summary>
            /// IsNull(nil)
            /// </summary>
            public bool nil;

            /// <summary>
            /// Value
            /// </summary>
            public List<StringEntry> v;

            public string Name => n;
            public int Count => c;
            public bool IsNull => nil;
            public IEnumerable<IValueEntry> Value => v?.OfType<IValueEntry>();
        }
    }
}