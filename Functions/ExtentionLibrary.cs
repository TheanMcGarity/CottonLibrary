using System.Reflection;
using static CottonLibrary.Library;

using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppSystem.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CottonLibrary;

public static class ExtentionLibrary
{
    public static Il2CppSystem.Type CPPTypeOf(this Type type)
    {
        string typeName = type.AssemblyQualifiedName;

        if (typeName.ToLower().StartsWith("il2cpp"))
        {
            typeName = typeName.Substring("il2cpp".Length);
        }

        Il2CppSystem.Type il2cppType = Il2CppSystem.Type.GetType(typeName);

        return il2cppType;
    }

    public static T getObjRec<T>(this GameObject obj, string name) where T : class
    {
        var transform = obj.transform;

        List<GameObject> totalChildren = GetAllChildren(transform);
        for (int i = 0; i < totalChildren.Count; i++)
            if (totalChildren[i].name == name)
            {
                if (typeof(T) == typeof(GameObject))
                    return totalChildren[i] as T;
                if (typeof(T) == typeof(Transform))
                    return totalChildren[i].transform as T;
                if (totalChildren[i].GetComponent<T>() != null)
                    return totalChildren[i].GetComponent<T>();
            }

        return null;
    }

    public static List<GameObject> GetAllChildren(this GameObject obj)
    {
        var container = obj.transform;
        List<GameObject> allChildren = new List<GameObject>();
        for (int i = 0; i < container.childCount; i++)
        {
            var child = container.GetChild(i);
            allChildren.Add(child.gameObject);
            allChildren.AddRange(GetAllChildren(child));
        }

        return allChildren;
    }

    public static T[] GetAllChildrenOfType<T>(this GameObject obj) where T : Component
    {
        List<T> children = new List<T>();
        foreach (var child in obj.GetAllChildren())
        {
            if (child.GetComponent<T>() != null)
            {
                children.Add(child.GetComponent<T>());
            }
        }

        return children.ToArray();
    }

    public static T[] GetAllChildrenOfType<T>(this Transform obj) where T : Component
    {
        List<T> children = new List<T>();
        foreach (var child in obj.GetAllChildren())
        {
            if (child.GetComponent<T>() != null)
            {
                children.Add(child.GetComponent<T>());
            }
        }

        return children.ToArray();
    }

    public static T GetObjRec<T>(this Transform transform, string name) where T : class
    {
        List<GameObject> totalChildren = transform.GetAllChildren();
        for (int i = 0; i < totalChildren.Count; i++)
            if (totalChildren[i].name == name)
            {
                if (typeof(T) == typeof(GameObject))
                    return totalChildren[i] as T;
                if (typeof(T) == typeof(Transform))
                    return totalChildren[i].transform as T;
                if (totalChildren[i].GetComponent<T>() != null)
                    return totalChildren[i].GetComponent<T>();
            }

        return null;
    }

    public static List<GameObject> GetAllChildren(this Transform container)
    {
        List<GameObject> allChildren = new List<GameObject>();
        for (int i = 0; i < container.childCount; i++)
        {
            var child = container.GetChild(i);
            allChildren.Add(child.gameObject);
            allChildren.AddRange(GetAllChildren(child.gameObject));
        }

        return allChildren;
    }

    public static void SetupForSaving(this IdentifiableType ident, string RefID)
    {
        savedIdents.TryAdd(RefID, ident);
    }

    public static void AddToGroup(this IdentifiableType type, string groupName)
    {
        var group = Get<IdentifiableTypeGroup>(groupName);
        group._memberTypes.Add(type);
        group.GetRuntimeObject()._memberTypes.Add(type);
    }

    internal static void RefreshIfNotFound(this IdentifiableTypePersistenceIdLookupTable table,
        IdentifiableType ident)
    {
        try
        {
            table.GetPersistenceId(ident);
        }
        catch
        {
            foreach (var refresh in savedIdents)
                INTERNAL_SetupSaveForIdent(refresh.Key, refresh.Value);
        }
    }

    public static Il2CppArrayBase<IdentifiableType> GetAllMembersArray(this IdentifiableTypeGroup group) => group.GetAllMembers().ToArray();


    public static Sprite ConvertToSprite(this Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0f, 0f, (float)texture.width, (float)texture.height),
            new Vector2(0.5f, 0.5f), 1f);
    }

    public static GameObject CopyObject(this GameObject obj) => Object.Instantiate(obj, rootOBJ.transform);


    /// <summary>
    /// Use this for copying components, please make sure you are copying the same types!
    /// </summary>
    public static void CopyFields(this Object target, Object source)
    {
        foreach (var field in source.GetIl2CppType().GetFields((Il2CppSystem.Reflection.BindingFlags)60))
        {
            if (field.IsLiteral && !field.IsInitOnly)
                continue;
            try
            {
                target.GetIl2CppType().GetField(field.Name, (Il2CppSystem.Reflection.BindingFlags)60).SetValue(target, field.GetValue(source));
            }
            // Errors when encountering `const` or `readonly`!
            catch { }
        }
    }
    
    public static Il2CppReferenceArray<T> Add<T>(this Il2CppReferenceArray<T> array, T obj)
        where T : Il2CppObjectBase
    {
        var list = new Il2CppSystem.Collections.Generic.List<T>();
        foreach (var item in array)
        {
            list.Add(item);
        }

        list.Add(obj);

        array = list.ToArray().Cast<Il2CppReferenceArray<T>>();
        return array;
    }

    public static Il2CppReferenceArray<T> AddRange<T>(this Il2CppReferenceArray<T> array,
        Il2CppReferenceArray<T> obj) where T : Il2CppObjectBase
    {
        var list = new Il2CppSystem.Collections.Generic.List<T>();
        foreach (var item in array)
        {
            list.Add(item);
        }

        foreach (var item in obj)
        {
            list.Add(item);
        }

        array = list.ToArray().Cast<Il2CppReferenceArray<T>>();
        return array;
    }

    public static Il2CppReferenceArray<T> AddRangeNoMultiple<T>(this Il2CppReferenceArray<T> array,
        Il2CppReferenceArray<T> obj) where T : Il2CppObjectBase
    {
        var list = new Il2CppSystem.Collections.Generic.List<T>();
        foreach (var item in array)
        {
            if (!list.Contains(item))
                list.Add(item);
        }

        foreach (var item in obj)
        {
            if (!list.Contains(item))
                list.Add(item);
        }

        array = list.ToArray().Cast<Il2CppReferenceArray<T>>();
        return array;
    }

    public static void AddListRange<T>(this Il2CppSystem.Collections.Generic.List<T> list,
        Il2CppSystem.Collections.Generic.List<T> obj) where T : Il2CppObjectBase
    {
        foreach (var iobj in obj)
        {
            list.Add(iobj);
        }
    }

    public static void AddListRangeNoMultiple<T>(this Il2CppSystem.Collections.Generic.List<T> list,
        Il2CppSystem.Collections.Generic.List<T> obj) where T : Il2CppObjectBase
    {
        foreach (var iobj in obj)
        {
            if (!list.Contains(iobj))
            {
                list.Add(iobj);
            }
        }
    }

    public static Il2CppStringArray AddString(this Il2CppStringArray array, string str)
    {
        string[] strArray = new string[array.Count + 1];

        int i = 0;
        foreach (var item in array)
        {
            strArray[i] = item;
            i++;
        }

        strArray[i] = str;

        return new Il2CppStringArray(strArray);
    }

    public static void MakePrefab(this GameObject obj)
    {
        UnityEngine.Object.DontDestroyOnLoad(obj);
        obj.transform.parent = rootOBJ.transform;
    }

    public static void MakeNotSellable(this IdentifiableType ident) => MakeNOTSellable(ident);
    public static bool IsSellable(this IdentifiableType ident) => IsSellable(ident);

    public static bool MakeSellable(this IdentifiableType ident,
        float marketValue,
        float marketSaturation,
        bool hideInMarket = false)
        => MakeSellable(ident, marketValue, marketSaturation, hideInMarket);

    public static bool RemoveComponent<T>(this Transform obj) where T : Component
    {
        try
        {
            T comp = obj.GetComponent<T>();
            var check = comp.gameObject;
            Object.Destroy(comp);
            return true;
        }
        catch
        {
            return false;
        }
    }
    public static bool RemoveComponent<T>(this GameObject obj) where T : Component
    {
        try
        {
            T comp = obj.GetComponent<T>();
            var check = comp.gameObject;
            Object.Destroy(comp);
            return true;
        }
        catch
        {
            return false;
        }
    }
    public static bool RemoveComponent<T>(this Component obj) where T : Component
    {
        try
        {
            T comp = obj.GetComponent<T>();
            var check = comp.gameObject;
            Object.Destroy(comp);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static T AddComponent<T>(this Component obj) where T : Component
    {
        return obj.gameObject.AddComponent<T>();
    }
    
    public static bool IsInsideRange(this int number, int rangeMin, int rangeMax) =>
        number >= rangeMin && number <= rangeMax;

    internal static bool TryAddComponentTypeIfNull(this GameObject obj, Il2CppSystem.Type type, out Component comp)
    {
        if (!obj.GetComponent(type))
        {
            comp = obj.AddComponent(type);
            return true;
        }
        
        comp = null;
        return false;
    }

    public static bool ContainsAny(this string str, params string[] check)
    {
        foreach (var c in check)
            if (str.Contains(c))
                return true;
        
        return false;
    }
}