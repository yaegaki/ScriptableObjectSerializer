using System;

namespace ScriptableObjectSerializer.Patchers
{
    public interface IPatcher
    {
        void PatchTo(PatchContext context, ref object obj, IObjectNode patch);
        IObjectNode PatchFrom(PatchContext context, object obj, string name);
    }
}
