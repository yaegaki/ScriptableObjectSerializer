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
        public byte[] Serialize(IObjectNode obj)
        {
            var json = JsonUtility.ToJson(ComplexEntries.AvoidSerializationDepthLimit(ToEntry(obj)));
            return Encoding.UTF8.GetBytes(json);
        }

        public IObjectNode Deserialize(byte[] bin)
        {
            var json = Encoding.UTF8.GetString(bin);
            var entry = JsonUtility.FromJson<ComplexEntries.ComplexEntry0>(json);
            return FromEntry(ComplexEntries.ToEntry(entry));
        }

        private IObjectNode FromEntry(ComplexEntry entry)
        {
            var intValuesCount = entry.i32?.Count ?? 0;
            var complexValuesCount = entry.c?.Count ?? 0;
            var count = intValuesCount + complexValuesCount;
            if (count == 0)
            {
                return new ComplexObjectNode(entry.n, Array.Empty<IObjectNode>());
            }

            var list = new List<IObjectNode>(count);
            if (entry.i32 != null)
            {
                foreach (var v in entry.i32)
                {
                    list.Add(new IntObjectNode(v.n, v.v));
                }
            }
            if (entry.c != null)
            {
                foreach (var v in entry.c)
                {
                    list.Add(FromEntry(v));
                }
            }
            list.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
            return new ComplexObjectNode(entry.n, list);
        }

        private ComplexEntry ToEntry(IObjectNode obj)
        {
            var dummy = new ComplexEntry
            {
                c = new List<ComplexEntry>(1),
            };
            ToEntry(ref dummy, obj);
            return dummy.c[0];
        }

        private void ToEntry(ref ComplexEntry parent, IObjectNode obj)
        {
            switch (obj.Type)
            {
                case NodeType.Int:
                    if (parent.i32 == null) parent.i32 = new List<IntEntry>();
                    parent.i32.Add(new IntEntry
                    {
                        n = obj.Name,
                        v = obj.GetInt(),
                    });
                    break;
                case NodeType.Complex:
                    var self = new ComplexEntry
                    {
                        n = obj.Name,
                    };
                    foreach (var n in obj.EnumerateChildren())
                    {
                        ToEntry(ref self, n);
                    }
                    if (parent.c == null) parent.c = new List<ComplexEntry>();
                    parent.c.Add(self);
                    break;
            }
        }

        [Serializable]
        struct ComplexEntry
        {
            /// <summary>
            /// Name
            /// </summary>
            public string n;
            /// <summary>
            /// ComplexValues
            /// </summary>
            public List<ComplexEntry> c;
            /// <summary>
            /// IntValues
            /// </summary>
            public List<IntEntry> i32;
        }


        // avoid unity serialization depth limit...
        static class ComplexEntries
        {
            [Serializable]
            public struct ComplexEntry0
            {
                public string n;
                public List<ComplexEntry1> c;
                public List<IntEntry> i32;
            }

            [Serializable]
            public struct ComplexEntry1
            {
                public string n;
                public List<ComplexEntry2> c;
                public List<IntEntry> i32;
            }

            [Serializable]
            public struct ComplexEntry2
            {
                public string n;
                // public List<ComplexEntry> c;
                public List<IntEntry> i32;
            }

            public static ComplexEntry0 AvoidSerializationDepthLimit(ComplexEntry d0)
            {
                return new ComplexEntry0
                {
                    n = d0.n,
                    i32 = d0.i32,
                    c = d0.c?.Select(d1 => new ComplexEntry1
                    {
                        n = d1.n,
                        i32 = d1.i32,
                        c = d1.c?.Select(d2 => new ComplexEntry2
                        {
                            n = d2.n,
                            i32 = d2.i32,
                        }).ToList(),
                    }).ToList(),
                };
            }

            public static ComplexEntry ToEntry(ComplexEntry0 d0)
            {
                return new ComplexEntry
                {
                    n = d0.n,
                    i32 = d0.i32,
                    c = d0.c?.Select(d1 => new ComplexEntry
                    {
                        n = d1.n,
                        i32 = d1.i32,
                        c = d1.c?.Select(d2 => new ComplexEntry
                        {
                            n = d2.n,
                            i32 = d2.i32,
                        }).ToList(),
                    }).ToList(),
                };
            }
        }

        [Serializable]
        struct IntEntry
        {
            public string n;
            public int v;
        }
    }
}