using System;
using System.Collections;
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

        private static IReflectionPatcher Create(string name, IValueAccessor valueAccessor, Type fieldType)
        {
            var nodeType = fieldType.ToNodeType();
            if (nodeType != NodeType.Complex) return null;

            var flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var children = fieldType.GetFields(flags)
                .Where(IsSerializeField)
                .Where(f => IsSerializableType(f.FieldType, false))
                .OrderBy(f => f.Name, StringComparer.Ordinal)
                .Select(CreateChildPatcher)
                .Where(p => p != null);
            
            return new ComplexPatcher(name, valueAccessor, fieldType, nodeType, children);
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
            var nodeType = type.ToNodeType();
            if (type.IsArray || type.IsGenericType)
            {
                var elemType = type.IsArray ? type.GetElementType() : type.GenericTypeArguments.First();
                var valueAccessor = new ListIndexValueAccesor(type, elemType);
                IReflectionPatcher childPatcher;
                if (nodeType == NodeType.Complex)
                {
                    childPatcher = Create(fieldInfo.Name, valueAccessor, elemType);
                }
                else
                {
                    childPatcher = new PrimitivePatcher(fieldInfo.Name, valueAccessor, nodeType);
                }

                return new ListPatcher(fieldInfo.Name, fieldInfo, nodeType, childPatcher);
            }

            if (nodeType == NodeType.Complex)
            {
                return Create(fieldInfo.Name, new FieldInfoValueAccessor(fieldInfo), fieldInfo.FieldType);
            }

            return new PrimitivePatcher(fieldInfo.Name, new FieldInfoValueAccessor(fieldInfo), nodeType);
        }
    }

    interface IReflectionPatcher : IPatcher
    {
        string Name { get; }
        object GetValue(object parent);
        void SetValue(object parent, IObjectNode node);
        IObjectNode PatchFrom(object obj, string name);
    }

    class PrimitivePatcher : IReflectionPatcher
    {
        public string Name { get; }
        private readonly IValueAccessor valueAccessor;
        private readonly NodeType nodeType;

        public PrimitivePatcher(string name, IValueAccessor valueAccessor, NodeType nodeType)
        {
            this.Name = name;
            this.valueAccessor = valueAccessor;
            this.nodeType = nodeType;
        }

        public void PatchTo(object obj, IObjectNode patch)
        {
            // not supported
            return;
        }

        public IObjectNode PatchFrom(object obj)
            =>  PatchFrom(obj, this.Name);

        public IObjectNode PatchFrom(object obj, string name)
        {
            if (obj == null) return null;
            if (!this.valueAccessor.MatchChildType(obj)) return null;

            return new PrimitiveObjectNode(this.nodeType, name, obj);
        }

        public object GetValue(object parent)
        {
            if (parent == null) return null;
            if (!this.valueAccessor.MatchParentType(parent)) return null;
            return this.valueAccessor.GetValue(parent);
        }


        public void SetValue(object parent, IObjectNode node)
        {
            if (this.nodeType != node.Type) return;
            if (!this.valueAccessor.MatchParentType(parent)) return;
            var v = node.Value;
            if (v == null) return;
            if (!this.valueAccessor.MatchChildType(v)) return;

            this.valueAccessor.SetValue(parent, v);
        }
    }

    class ComplexPatcher : IReflectionPatcher
    {
        public string Name { get; }
        private readonly IValueAccessor valueAccessor;
        private readonly Type type;
        private readonly IReflectionPatcher[] children;

        public ComplexPatcher(string name, IValueAccessor valueAccessor, Type type, NodeType nodeType, IEnumerable<IReflectionPatcher> children)
        {
            this.Name = name;
            this.valueAccessor = valueAccessor;
            this.type = type;
            this.children = children?.ToArray() ?? Array.Empty<IReflectionPatcher>();
        }

        public void PatchTo(object obj, IObjectNode patch)
        {
            if (patch == null) return;
            if (patch.Type != NodeType.Complex) return;
            if (patch.IsList) return;
            if (patch.IsNull) return;

            var index = 0;
            foreach (var childNode in patch.Children)
            {
                var childPatcher = FindChildPatcher(ref index, childNode.Name);
                childPatcher?.SetValue(obj, childNode);
            }
        }

        public IObjectNode PatchFrom(object obj)
            => PatchFrom(obj, this.Name);

        public IObjectNode PatchFrom(object obj, string name)
        {
            if (obj == null)
            {
                return new ComplexObjectNode(name, true, null);
            }

            var childNodes = this.children
                .Select(p => p.PatchFrom(p.GetValue(obj)))
                .Where(c => c != null);

            return new ComplexObjectNode(name, false, childNodes);
        }

        public object GetValue(object parent)
        {
            if (parent == null) return null;
            if (this.valueAccessor == null) return null;
            if (!this.valueAccessor.MatchParentType(parent)) return null;
            return this.valueAccessor.GetValue(parent);
        }

        public void SetValue(object parent, IObjectNode patch)
        {
            if (this.valueAccessor == null) return;
            if (patch.Type != NodeType.Complex) return;

            if (patch.IsNull)
            {
                this.valueAccessor.SetValue(parent, null);
                return;
            }

            var instance = this.valueAccessor.GetValue(parent);
            var create = instance == null;
            if (create)
            {
                instance = ReflectionHelper.CreateInstance(this.type);
            }

            PatchTo(instance, patch);

            if (create || this.type.IsValueType)
            {
                this.valueAccessor.SetValue(parent, instance);
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

    class ListPatcher : IReflectionPatcher
    {
        public string Name { get; }
        private readonly FieldInfo fieldInfo;
        private readonly NodeType nodeType;
        private readonly IReflectionPatcher childPatcher;

        public ListPatcher(string name, FieldInfo fieldInfo, NodeType nodeType, IReflectionPatcher childPatcher)
        {
            this.Name = name;
            this.fieldInfo = fieldInfo;
            this.nodeType = nodeType;
            this.childPatcher = childPatcher;
        }

        public IObjectNode PatchFrom(object obj)
            => PatchFrom(obj, this.Name);

        public IObjectNode PatchFrom(object obj, string name)
        {
            if (obj == null)
            {
                return new ComplexObjectNode(this.nodeType, name, 0, true, null);
            }

            if (!(obj is IList list)) return null;

            var children = list
                .OfType<object>()
                .Select((c, i) => this.childPatcher.PatchFrom(c, i.ToString()))
                .Where(c => c != null);
            
            return new ComplexObjectNode(this.nodeType, name, list.Count, false, children);
        }

        public void PatchTo(object obj, IObjectNode patch)
        {
            if (patch == null) return;
            if (patch.Type != this.nodeType) return;
            if (!patch.IsList) return;
            if (patch.ListCount < 0) return;
            if (patch.IsNull) return;
            if (!(obj is IList list)) return;

            ParentWithIndex p = null;
            foreach (var childNode in patch.Children)
            {
                if (!(int.TryParse(childNode.Name, out var index))) continue;
                if (index < 0 || index >= list.Count) continue;

                if (p == null) p = new ParentWithIndex();
                p.Index = index;
                this.childPatcher.SetValue(p, childNode);
            }
        }

        public object GetValue(object parent)
        {
            if (parent == null) return null;
            if (!this.fieldInfo.DeclaringType.IsAssignableFrom(parent.GetType())) return null;
            return this.fieldInfo.GetValue(parent);
        }

        public void SetValue(object parent, IObjectNode node)
        {
            if (!node.IsList) return;
            if (node.ListCount < 0) return;
            if (!this.fieldInfo.DeclaringType.IsAssignableFrom(parent.GetType())) return;

            if (node.IsNull)
            {
                this.fieldInfo.SetValue(parent, null);
                return;
            }

            var list = this.fieldInfo.GetValue(parent) as IList;
            if (list == null || list.Count != node.ListCount)
            {
                list = ReflectionHelper.CreateArrayOrList(this.fieldInfo.FieldType, node.ListCount);
                if (list == null) return;
                this.fieldInfo.SetValue(parent, list);
            }

            ParentWithIndex p = null;
            foreach (var child in node.Children)
            {
                if (!int.TryParse(child.Name, out var index)) continue;
                if (index < 0 || index >= list.Count) continue;
                if (p == null)
                {
                    p = new ParentWithIndex();
                    p.Parent = list;
                }

                p.Index = index;
                this.childPatcher.SetValue(p, child);
            }
        }
    }
}
