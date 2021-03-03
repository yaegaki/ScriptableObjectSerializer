using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptableObjectSerializer.Patchers
{
    public class ScriptableObjectPatcher : IPatcher
    {
        private readonly Type type;
        private readonly IPatcherRegistry patcherRegistry;

        private static readonly string TypeNodeName = ":Type:";
        private static readonly object lockObj = new object();
        private static readonly Dictionary<Type, IPatcher> patcherCache = new Dictionary<Type, IPatcher>();

        public ScriptableObjectPatcher(Type type, IPatcherRegistry patcherRegistry)
        {
            this.type = type;
            this.patcherRegistry = patcherRegistry;
        }

        public IObjectNode PatchFrom(object obj, string name)
        {
            if (obj == null)
            {
                return new ComplexObjectNode(name, true, null);
            }

            var scriptableObjectType = obj?.GetType() ?? this.type;
            var patcher = GetPatcher(scriptableObjectType, this.patcherRegistry);
            var patch = patcher?.PatchFrom(obj, name);
            if (patch == null) return null;
            if (patch.IsNull) return patch;


            var childrenWithType = new List<IObjectNode>(patch.Children.Count + 1);
            childrenWithType.Add(new PrimitiveObjectNode(NodeType.String, TypeNodeName, scriptableObjectType.Name));
            childrenWithType.AddRange(patch.Children);
            return new ComplexObjectNode(name, false, childrenWithType);
        }

        public void PatchTo(ref object obj, IObjectNode patch)
        {
            if (patch.Type != NodeType.Complex) return;

            var typeNode = patch.Children.FirstOrDefault(n => n.Name == TypeNodeName && n.Type == NodeType.String);
            var typeName = typeNode?.Value as string ?? this.type.Name;

            if (obj == null || obj.GetType().Name != typeName)
            {
                obj = ScriptableObject.CreateInstance(typeName);
                if (obj == null) return;
            }

            var patcher = GetPatcher(obj.GetType(), this.patcherRegistry);
            patcher?.PatchTo(ref obj, patch);
        }

        private static IPatcher GetPatcher(Type type, IPatcherRegistry patcherRegistry)
        {
            lock (lockObj)
            {
                if (patcherCache.TryGetValue(type, out var patcher)) return patcher;
                patcher = new ComplexPatcher(type, patcherRegistry);
                patcherCache[type] = patcher;
                return patcher;
            }
        }
    }
}

