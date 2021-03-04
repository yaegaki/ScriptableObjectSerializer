using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityObjectSerializer
{
    public enum NodeType
    {
        Int,
        UInt,
        String,
        Complex,
    }

    public static class TypeExtensions
    {
        public static NodeType ToNodeType(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (Match<int>(type))
            {
                return NodeType.Int;
            }
            else if (Match<uint>(type))
            {
                return NodeType.UInt;
            }
            else if (Match<string>(type))
            {
                return NodeType.String;
            }

            return NodeType.Complex;
        }

        private static bool Match<T>(Type t)
            => t == typeof(T) || t == typeof(T[]) || t == typeof(List<T>);
    }

    public interface IObjectNode
    {
        NodeType Type { get; }
        string Name { get; }
        object Value { get; }
        bool IsList { get; }
        int ListCount { get; }
        bool IsNull { get; }
        IReadOnlyList<IObjectNode> Children { get; }
    }

    public class PrimitiveObjectNode : IObjectNode
    {
        public NodeType Type { get; }
        public string Name { get; }
        public object Value { get;}
        public bool IsList => false;
        public int ListCount => 0;
        public bool IsNull => Value == null;
        public IReadOnlyList<IObjectNode> Children => Array.Empty<IObjectNode>();

        public PrimitiveObjectNode(NodeType type, string name, object value)
            => (this.Type, this.Name, this.Value) = (type, name, value);
    }

    public class ComplexObjectNode : IObjectNode
    {
        /// <summary>
        /// If this node is not list, this represents SelfType.
        /// Otherwise this represents ElementType.
        /// </summary>
        public NodeType Type { get; }
        public string Name { get; }
        public object Value => null;
        public bool IsList { get; }
        public int ListCount { get; } 
        public bool IsNull { get; }
        public IReadOnlyList<IObjectNode> Children { get; }

        public ComplexObjectNode(string name, bool isNull, IEnumerable<IObjectNode> children)
            : this(NodeType.Complex, name, false, 0, isNull, children)
        {
        }

        public ComplexObjectNode(NodeType type, string name, int listCount, bool isNull, IEnumerable<IObjectNode> children)
            : this(type, name, true, listCount, isNull, children)
        {
        }

        private ComplexObjectNode(NodeType type, string name, bool isList, int listCount, bool isNull, IEnumerable<IObjectNode> children)
        {
            this.Type = type;
            this.Name = name;
            this.IsList = isList;
            this.ListCount = listCount;
            this.Children = children?.ToArray() ?? Array.Empty<IObjectNode>();
            this.IsNull = isNull;
        }
    }
}
