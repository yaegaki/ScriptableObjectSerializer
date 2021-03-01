using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptableObjectSerializer
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

            if (type == typeof(int))
            {
                return NodeType.Int;
            }
            else if (type == typeof(uint))
            {
                return NodeType.UInt;
            }
            else if (type == typeof(string))
            {
                return NodeType.String;
            }

            return NodeType.Complex;
        }
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
        public bool IsNull => false;
        public IReadOnlyList<IObjectNode> Children => Array.Empty<IObjectNode>();

        public PrimitiveObjectNode(NodeType type, string name, object value)
            => (this.Type, this.Name, this.Value) = (type, name, value);
    }

    public class ComplexObjectNode : IObjectNode
    {
        public NodeType Type => NodeType.Complex;
        public string Name { get; }
        public object Value => null;
        public bool IsList { get; }
        public int ListCount { get; } 
        public bool IsNull { get; }
        public IReadOnlyList<IObjectNode> Children { get; }

        public ComplexObjectNode(string name, bool isList, int listCount, bool isNull, IEnumerable<IObjectNode> children)
        {
            this.Name = name;
            this.IsList = isList;
            this.ListCount = listCount;
            this.Children = children?.ToArray() ?? Array.Empty<IObjectNode>();
            this.IsNull = false;
        }
    }
}
