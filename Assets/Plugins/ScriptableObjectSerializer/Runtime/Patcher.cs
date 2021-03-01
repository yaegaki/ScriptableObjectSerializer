using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ScriptableObjectSerializer
{
    public interface IPatcher
    {
        void PatchTo(object obj, IObjectNode patch);
        IObjectNode PatchFrom(object obj);
    }

    public static class Patcher
    {
        public static IPatcher Create<T>()
            => Create(typeof(T));

        public static IPatcher Create(Type type)
            => Create(":Root:", null, type);

        private static IReflectionPatcher Create(string name, FieldInfo fieldInfo, Type fieldType)
        {
            var nodeType = fieldType.ToNodeType();
            if (nodeType != NodeType.Complex) return null;

            var flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var children = fieldType.GetFields(flags)
                .Where(IsSerializeField)
                .Where(f => IsSerializableType(f.FieldType, false))
                .OrderBy(f => f.Name)
                .Select(CreateChildPatcher)
                .Where(p => p != null);
            
            return new ComplexPatcher(name, fieldInfo, nodeType, children);
        }

        private static bool IsSerializeField(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPublic)
            {
                return fieldInfo.GetCustomAttribute<NonSerializedAttribute>() == null;
            }

            return fieldInfo.GetCustomAttribute<SerializeField>() != null;
        }

        private static bool IsSerializableType(Type type, bool isTypeArg)
        {
            if (type.GetCustomAttribute<SerializableAttribute>() == null) return false;

            if (type.IsGenericType)
            {
                // Can't serialize nested generic type
                // ex) List<List<int>>
                if (isTypeArg) return false;

                if (type.GetGenericTypeDefinition() != typeof(List<>)) return false;
                var typeArg = type.GenericTypeArguments[0];
                if (!IsSerializableType(typeArg, true)) return false;
            }

            if (type.IsArray)
            {
                if (isTypeArg) return false;
                var elemType = type.GetElementType();
                if (!IsSerializableType(elemType, true)) return false;
            }

            return true;
        }

        private static IReflectionPatcher CreateChildPatcher(FieldInfo fieldInfo)
        {
            var type = fieldInfo.FieldType;
            // TODO: support Array
            if (type.IsArray) return null;
            // TODO: support List
            if (type.IsGenericType) return null;

            var nodeType = type.ToNodeType();
            if (nodeType == NodeType.Complex)
            {
                return Create(fieldInfo.Name, fieldInfo, fieldInfo.FieldType);
            }

            return new PrimitivePatcher(fieldInfo.Name, fieldInfo, nodeType);
        }
    }

    interface IReflectionPatcher : IPatcher
    {
        string Name { get; }
        object GetValue(object parent);
        void SetValue(object parent, IObjectNode node);
    }

    class PrimitivePatcher : IReflectionPatcher
    {
        public string Name { get; }
        private readonly FieldInfo fieldInfo;
        private readonly NodeType nodeType;

        public PrimitivePatcher(string name, FieldInfo fieldInfo, NodeType nodeType)
            => (this.Name, this.fieldInfo, this.nodeType) = (name, fieldInfo, nodeType);

        public void PatchTo(object obj, IObjectNode patch)
        {
            // not supported
            return;
        }

        public IObjectNode PatchFrom(object obj)
        {
            if (obj == null) return null;
            if (this.fieldInfo.FieldType != obj.GetType()) return null;

            return new PrimitiveObjectNode(this.nodeType, this.Name, obj);
        }

        public object GetValue(object parent)
        {
            if (parent == null) return null;
            if (this.fieldInfo.DeclaringType != parent.GetType()) return null;
            return this.fieldInfo.GetValue(parent);
        }


        public void SetValue(object parent, IObjectNode node)
        {
            var v = node.Value;
            if (v == null) return;
            if (!this.fieldInfo.FieldType.IsAssignableFrom(v.GetType())) return;

            this.fieldInfo.SetValue(parent, v);
        }
    }

    class ComplexPatcher : IReflectionPatcher
    {
        public string Name { get; }
        private readonly FieldInfo fieldInfo;
        private readonly IReflectionPatcher[] children;

        public ComplexPatcher(string name, FieldInfo fieldInfo, NodeType nodeType, IEnumerable<IReflectionPatcher> children)
        {
            this.Name = name;
            this.fieldInfo = fieldInfo;
            this.children = children?.ToArray() ?? Array.Empty<IReflectionPatcher>();
        }

        public void PatchTo(object obj, IObjectNode patch)
        {
            if (patch == null) return;
            if (patch.Type != NodeType.Complex) return;

            var index = 0;
            foreach (var childNode in patch.Children)
            {
                var childPatcher = FindChildPatcher(ref index, childNode.Name);
                childPatcher?.SetValue(obj, childNode);
            }
        }

        public IObjectNode PatchFrom(object obj)
        {
            if (obj == null)
            {
                return new ComplexObjectNode(this.Name, false, 0, true, null);
            }

            var childNodes = this.children
                .Select(p => p.PatchFrom(p.GetValue(obj)));

            return new ComplexObjectNode(this.Name, false, 0, false, childNodes);
        }

        public object GetValue(object parent)
        {
            if (parent == null) return null;
            if (this.fieldInfo == null) return null;
            if (!this.fieldInfo.DeclaringType.IsAssignableFrom(parent.GetType())) return null;
            return this.fieldInfo.GetValue(parent);
        }

        public void SetValue(object parent, IObjectNode patch)
        {
            if (this.fieldInfo == null) return;

            if (patch.IsNull)
            {
                this.fieldInfo.SetValue(parent, null);
                return;
            }

            var instance = this.fieldInfo.GetValue(parent);
            var create = instance == null;
            if (create)
            {
                instance = Activator.CreateInstance(this.fieldInfo.FieldType);
            }

            PatchTo(instance, patch);

            if (create || this.fieldInfo.FieldType.IsValueType)
            {
                this.fieldInfo.SetValue(parent, instance);
            }
        }

        private IReflectionPatcher FindChildPatcher(ref int index, string name)
        {
            for (; index < this.children.Length; index++)
            {
                var childPatcher = this.children[index];
                var comp = string.CompareOrdinal(childPatcher.Name, name);
                if (comp > 0) break;
                if (comp == 0)
                {
                    index++;
                    return childPatcher;
                }
            }

            return null;
        }
    }

    class ListPatcher
    {
    }
}
