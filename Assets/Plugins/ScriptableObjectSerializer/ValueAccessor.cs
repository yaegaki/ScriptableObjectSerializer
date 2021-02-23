using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ScriptableObjectSerializer
{
    interface IValueAccessor
    {
        string Name { get; }
        IObjectNode GetValue(object parent);
        void SetValue(object obj, IObjectNode valueNode);
    }

    static class ValueAccessor
    {
        public static IValueAccessor Create(Type type)
        {
            const string rootName = ":Root:";
            return Create(rootName, null, type);
        }

        private static IValueAccessor Create(string name, FieldInfo fieldInfo, Type fieldType)
        {
            if (PrimitiveValueAccessor.IsPrimitiveType(fieldType))
            {
                return null;
            }

            var flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var children = fieldType.GetFields(flags)
                .Where(IsSerializeField)
                .Where(f => IsSerializableType(f.FieldType))
                .OrderBy(f => f.Name)
                .Select(f => CreateValueAccessor(f))
                .ToArray();
            
            children.Select(c => c.Name).Aggregate("", (a, b) =>
            {
                if (a == b) Debug.LogWarning($"DuplicateName:{a}");
                return b;
            });

            return new ComplexValueAccessor(name, fieldInfo, children);
        }

        private static bool IsSerializeField(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPublic)
            {
                return fieldInfo.GetCustomAttribute<NonSerializedAttribute>() == null;
            }

            return fieldInfo.GetCustomAttribute<SerializeField>() != null;
        }

        private static bool IsSerializableType(Type type)
        {
            return type.GetCustomAttribute<SerializableAttribute>() != null;
        }

        private static IValueAccessor CreateValueAccessor(FieldInfo fieldInfo)
        {
            var name = fieldInfo.Name;
            if (PrimitiveValueAccessor.TryCreatPrimitiveValueAccessor(name, fieldInfo, out var accessor))
            {
                return accessor;
            }

            return Create(name, fieldInfo, fieldInfo.FieldType);
        }
    }

    class ComplexValueAccessor : IValueAccessor
    {
        public bool IsRoot => fieldInfo == null;
        public string Name { get; }
        private readonly FieldInfo fieldInfo;
        private readonly IValueAccessor[] children;

        public ComplexValueAccessor(string name, FieldInfo fieldInfo, IEnumerable<IValueAccessor> children)
        {
            this.Name = name;
            this.fieldInfo = fieldInfo;
            this.children = children.ToArray();
        }

        public IObjectNode GetValue(object obj)
        {
            var value = IsRoot ? obj : this.fieldInfo.GetValue(obj);
            var values = this.children.Select(a => a.GetValue(value)).Where(v => v != null);
            return new ComplexObjectNode(this.Name, values);
        }

        public void SetValue(object parent, IObjectNode valueNode)
        {
            var index = 0;
            var self = IsRoot ? parent : this.fieldInfo.GetValue(parent);
            foreach (var childNode in valueNode.EnumerateChildren())
            {
                var name = childNode.Name;
                for (; index < this.children.Length; index++)
                {
                    var childAccessor = this.children[index];
                    var comp = string.Compare(childAccessor.Name, name, StringComparison.OrdinalIgnoreCase);
                    if (comp < 0) break;
                    if (comp == 0)
                    {
                        index++;
                        childAccessor.SetValue(self, childNode);
                        break;
                    }
                }
            }

            if (!IsRoot && this.fieldInfo.FieldType.IsValueType)
            {
                this.fieldInfo.SetValue(parent, self);
            }
        }
    }

    static class PrimitiveValueAccessor
    {
        private static Dictionary<Type, IValueAccessor> cache = new Dictionary<Type, IValueAccessor>();
        private static readonly object lockObj = new object();

        public static bool IsPrimitiveType(Type type)
        {
            if (type == typeof(int)) return true;
            return false;
        }

        public static bool TryCreatPrimitiveValueAccessor(string name, FieldInfo fieldInfo, out IValueAccessor valueAccessor)
        {
            var type = fieldInfo.FieldType;
            valueAccessor = null;
            if (type == typeof(int)) valueAccessor = new IntValueAccessor(name, fieldInfo);

            return valueAccessor != null;;
        }
    }

    class IntValueAccessor : IValueAccessor
    {
        public string Name { get; }
        public bool IsComplex => false;
        private FieldInfo fieldInfo;
        public IntValueAccessor(string name, FieldInfo fieldInfo)
            => (this.Name, this.fieldInfo) = (name, fieldInfo);

        public IObjectNode GetValue(object obj)
            => new IntObjectNode(fieldInfo.Name, (int)this.fieldInfo.GetValue(obj));

        public void SetValue(object obj, IObjectNode valueNode)
            => this.fieldInfo.SetValue(obj, valueNode.GetInt());
    }
}