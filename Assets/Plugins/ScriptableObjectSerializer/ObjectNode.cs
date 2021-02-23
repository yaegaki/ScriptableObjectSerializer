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

    public interface IObjectNode
    {
        NodeType Type { get; }
        string Name { get; }
        int GetInt();
        uint GetUInt();
        float GetFloat();
        double GetDouble();
        string GetString();

        IEnumerable<IObjectNode> EnumerateChildren();
    }

    public class ComplexObjectNode : IObjectNode
    {
        public NodeType Type => NodeType.Complex;
        public string Name { get; }
        private readonly IObjectNode[] values;

        public ComplexObjectNode(string name, IEnumerable<IObjectNode> values)
            => (this.Name, this.values) = (name, values.ToArray());

        public int GetInt() => default;
        public uint GetUInt() => default;
        public float GetFloat() => default;
        public double GetDouble() => default;
        public string GetString() => default;

        public IEnumerable<IObjectNode> EnumerateChildren() => this.values;
    }

    public class IntObjectNode : IObjectNode
    {
        public NodeType Type => NodeType.Int;
        public string Name { get; }
        private readonly int value;

        public IntObjectNode(string name, int value)
            => (this.Name, this.value) = (name, value);

        public int GetInt() => this.value;
        public uint GetUInt() => default;
        public float GetFloat() => default;
        public double GetDouble() => default;
        public string GetString() => default;
        public string GetObject() => default;

        public IEnumerable<IObjectNode> EnumerateChildren() => Array.Empty<IObjectNode>();
    }
}
