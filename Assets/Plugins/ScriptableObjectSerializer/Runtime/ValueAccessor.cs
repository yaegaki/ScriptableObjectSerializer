using System;
using System.Collections;
using System.Reflection;

namespace ScriptableObjectSerializer
{
    interface IValueAccessor
    {
        bool MatchParentType(object parent);
        bool MatchChildType(object child);

        object GetValue(object parent);
        void SetValue(object parent, object value);
    }

    class FieldInfoValueAccessor : IValueAccessor
    {
        private readonly FieldInfo fieldInfo;

        public FieldInfoValueAccessor(FieldInfo fieldInfo)
            => this.fieldInfo = fieldInfo;

        public bool MatchParentType(object parent)
            => this.fieldInfo.DeclaringType.IsAssignableFrom(parent.GetType());

        public bool MatchChildType(object child)
            => this.fieldInfo.FieldType.IsAssignableFrom(child.GetType());

        public object GetValue(object parent)
            => this.fieldInfo.GetValue(parent);

        public void SetValue(object parent, object value)
            => this.fieldInfo.SetValue(parent, value);
    }

    class ListIndexValueAccesor : IValueAccessor
    {
        private readonly Type parentType;
        private readonly Type childType;

        public ListIndexValueAccesor(Type parentType, Type childType)
            => (this.parentType, this.childType) = (parentType, childType);

        public bool MatchParentType(object parent)
        {
            if (!(parent is ParentWithIndex p)) return false;

            return this.parentType.IsAssignableFrom(p.Parent.GetType());
        }

        public bool MatchChildType(object child)
            => this.childType.IsAssignableFrom(child.GetType());


        public object GetValue(object parent)
        {
            if (!(parent is ParentWithIndex p)) return null;
            return p.Parent[p.Index];
        }

        public void SetValue(object parent, object value)
        {
            if (!(parent is ParentWithIndex p)) return;
            p.Parent[p.Index] = value;
        }
    }

    class ParentWithIndex
    {
        public IList Parent { get; set; }
        public int Index { get; set; }
    }
}