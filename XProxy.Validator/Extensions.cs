using UnityEngine;

namespace XProxy.Validator;

public static class Extensions
{
    public static string GetHierarchyPath(this Transform t)
    {
        string path = t.name;
        Transform parent = t.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
