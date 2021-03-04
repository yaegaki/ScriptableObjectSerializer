using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityObjectSerializer.Patchers
{
    public class ScriptableObjectPatcher : IPatcher
    {
        private readonly Type type;
        private readonly IPatcherRegistry patcherRegistry;

        private static readonly string TypeNodeName = ":Type:";
        private static readonly string ReferenceIdNodeName = ":ReferenceId:";
        private static readonly string ReferenceToNodeName = ":ReferenceTo:";

        private static readonly object lockObj = new object();
        private static readonly Dictionary<Type, IPatcher> patcherCache = new Dictionary<Type, IPatcher>();

        public ScriptableObjectPatcher(Type type, IPatcherRegistry patcherRegistry)
        {
            this.type = type;
            this.patcherRegistry = patcherRegistry;
        }

        public IObjectNode PatchFrom(PatchContext context, object obj, string name)
        {
            if (obj == null)
            {
                return new ComplexObjectNode(name, true, null);
            }

            var unityPatchContext = context.Get<UnityPatchContext>();
            var id = unityPatchContext.FindReferenceId(obj);
            if (id >= 0)
            {
                return new ComplexObjectNode(name, false, new IObjectNode[]
                {
                    new PrimitiveObjectNode(NodeType.Int, ReferenceToNodeName, id),
                });
            }
            id = unityPatchContext.Register(obj);

            var scriptableObjectType = obj?.GetType() ?? this.type;
            var patcher = GetPatcher(scriptableObjectType, this.patcherRegistry);
            var patch = patcher?.PatchFrom(context, obj, name);
            if (patch == null) return null;
            if (patch.IsNull) return patch;

            var childrenWithType = new List<IObjectNode>(patch.Children.Count + 2);
            childrenWithType.Add(new PrimitiveObjectNode(NodeType.String, TypeNodeName, scriptableObjectType.Name));
            childrenWithType.Add(new PrimitiveObjectNode(NodeType.Int, ReferenceIdNodeName, id));
            childrenWithType.AddRange(patch.Children);
            return new ComplexObjectNode(name, false, childrenWithType);
        }

        public void PatchTo(PatchContext context, ref object obj, IObjectNode patch)
        {
            if (patch.Type != NodeType.Complex) return;
            if (patch.IsNull)
            {
                obj = null;
                return;
            }

            var unityPatchContext = context.Get<UnityPatchContext>();
            var (typeName, id, referenceTo) = FindMetaInfo(patch);
            if (referenceTo >= 0)
            {
                var temp = unityPatchContext.FindObject(referenceTo);
                if (temp != null && this.type.IsAssignableFrom(temp.GetType()))
                {
                    obj = temp;
                }
                return;
            }

            if (string.IsNullOrEmpty(typeName))
            {
                typeName = this.type.Name;
            }

            if (obj == null || obj.GetType().Name != typeName)
            {
                obj = ScriptableObject.CreateInstance(typeName);
                if (obj == null) return;
            }

            if (id >= 0)
            {
                unityPatchContext.Register(id, obj);
            }

            var patcher = GetPatcher(obj.GetType(), this.patcherRegistry);
            patcher?.PatchTo(context, ref obj, patch);
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

        private static (string type, int id, int referenceTo) FindMetaInfo(IObjectNode node)
        {
            var type = "";
            var id = -1;
            var referenceTo = -1;
            foreach (var child in node.Children)
            {
                if (child.Name == TypeNodeName && child.Type == NodeType.String)
                {
                    type = (string)child.Value;
                }
                else if (child.Name == ReferenceIdNodeName && child.Type == NodeType.Int)
                {
                    id = (int)child.Value;
                }
                else if (child.Name == ReferenceToNodeName && child.Type == NodeType.Int)
                {
                    referenceTo = (int)child.Value;
                }
            }

            return (type, id, referenceTo);
        }
    }

    public class GameObjectPatcher : IPatcher
    {
        public IObjectNode PatchFrom(PatchContext context, object obj, string name)
        {
            return null;
        }

        public void PatchTo(PatchContext context, ref object obj, IObjectNode patch)
        {
            throw new NotImplementedException();
        }
    }

    public class VectorIntPatcher : IPatcher
    {
        private readonly Type type;
        private readonly Dictionary<string, PropertyInfo> propDict = new Dictionary<string, PropertyInfo>();

        public VectorIntPatcher(Type type, int dimension)
        {
            this.type = type;
            if (dimension >= 1) propDict["x"] = type.GetProperty("x");
            if (dimension >= 2) propDict["y"] = type.GetProperty("y");
            if (dimension >= 3) propDict["z"] = type.GetProperty("z");
        }

        public IObjectNode PatchFrom(PatchContext context, object obj, string name)
        {
            if (obj == null) return null;
            if (obj.GetType() != this.type) return null;

            return new ComplexObjectNode(name, false, CreateObjectNodes(obj));
        }

        private IEnumerable<IObjectNode> CreateObjectNodes(object parent)
        {
            var x = CreateObjectNode(parent, "x");
            if (x != null) yield return x;
            var y = CreateObjectNode(parent, "y");
            if (y != null) yield return y;
            var z = CreateObjectNode(parent, "z");
            if (z != null) yield return z;
        }

        private IObjectNode CreateObjectNode(object parent, string name)
        {
            if (!this.propDict.TryGetValue(name, out var propInfo)) return null;
            var v = propInfo.GetValue(parent);
            if (v == null) return null;
            if (v.GetType() != typeof(int)) return null;
            return new PrimitiveObjectNode(NodeType.Int, name, v);
        }

        public void PatchTo(PatchContext context, ref object obj, IObjectNode patch)
        {
            if (obj == null) return;
            if (patch.IsNull) return;
            if (patch.Type != NodeType.Complex) return;
            if (obj.GetType() != this.type) return;

            foreach (var node in patch.Children)
            {
                if (node.Type != NodeType.Int) continue;
                if (!propDict.TryGetValue(node.Name, out var prop)) continue;
                prop.SetValue(obj, node.Value);
            }
        }
    }
}
