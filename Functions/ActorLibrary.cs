using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CottonLibrary;

public static partial class Library
{

    public static IdentifiableType GetIdent(this GameObject obj)
    {
        try
        {
            return obj.GetComponent<IdentifiableActor>().identType;
        }
        catch
        {
            return null;
        }
    }

    public static IdentifiableType GetIdentifiableType(this GameObject obj)
    {
        var comp = obj.GetComponent<IdentifiableActor>();

        if (comp != null)
        {
            return comp.identType;
        }

        return null;
    }

    public static GameObject SpawnActor(this GameObject obj, Vector3 pos) =>
        SpawnActor(obj, pos, Quaternion.identity);

    public static GameObject SpawnActor(this GameObject obj, Vector3 pos, Vector3 rot) =>
        SpawnActor(obj, pos, Quaternion.Euler(rot));

    public static GameObject SpawnActor(this GameObject obj, Vector3 pos, Quaternion rot)
    {
        return InstantiationHelpers.InstantiateActor(obj,
            SRSingleton<SceneContext>.Instance.RegionRegistry.CurrentSceneGroup,
            pos,
            rot,
            false,
            SlimeAppearance.AppearanceSaveSet.NONE,
            SlimeAppearance.AppearanceSaveSet.NONE);
    }

    public static GameObject SpawnDynamic(this GameObject obj, Vector3 pos, Quaternion rot)
    {
        return InstantiationHelpers.InstantiateDynamic(obj, pos, rot);
    }

    public static void SetObjectPrefab(this IdentifiableType Object, GameObject prefab)
    {
        Object.prefab = prefab;
    }

    public static void SetObjectIdent(this GameObject prefab, IdentifiableType Object)
    {
        if (Object is SlimeDefinition)
        {
            prefab.GetComponent<SlimeEat>().SlimeDefinition = (SlimeDefinition)Object;
            prefab.GetComponent<SlimeAppearanceApplicator>().SlimeDefinition = (SlimeDefinition)Object;
        }

        prefab.GetComponent<IdentifiableActor>().identType = Object;
    }

    public static IdentifiableType CreatePlortType(string Name, Color32 VacColor, Sprite Icon, string RefID,
        float marketValue, float marketSaturation)
    {
        var plort = ScriptableObject.CreateInstance<IdentifiableType>();
        Object.DontDestroyOnLoad(plort);
        plort.hideFlags = HideFlags.HideAndDontSave;
        plort.name = Name + "Plort";
        plort.color = VacColor;
        plort.icon = Icon;
        plort.IsPlort = true;
        MakeSellable(plort, marketValue, marketSaturation);
        plort.AddToGroup("VaccableBaseSlimeGroup");
        INTERNAL_SetupLoadForIdent(RefID, plort);
        return plort;
    }

    public static void MakeVaccable(this IdentifiableType ident)
    {
        if (!ident.prefab.GetComponent<Vacuumable>())
            throw new NullReferenceException(
                "This object cannot be made vaccable, it's missing a Vacuumable component, you need to add one.");

        ident.AddToGroup("VaccableNonLiquids");
    }

    public static void SetPlortColor(Color32 Top, Color32 Middle, Color32 Bottom, GameObject Prefab)
    {
        var material = Prefab.GetComponent<MeshRenderer>().material;
        material.SetColor("_TopColor", Top);
        material.SetColor("_MiddleColor", Middle);
        material.SetColor("_BottomColor", Bottom);
    }

    public static void SetPlortTwinColor(Color32 Top, Color32 Middle, Color32 Bottom, GameObject Prefab)
    {
        var material = Prefab.GetComponent<MeshRenderer>().material;
        material.SetColor("_TwinTopColor", Top);
        material.SetColor("_TwinMiddleColor", Middle);
        material.SetColor("_TwinBottomColor", Bottom);
    }

    public static IdentifiableType GetPlort(string name)
    {
        foreach (IdentifiableType type in plorts.GetAllMembersArray())
            if (type.name.ToUpper() == name.ToUpper())
                return type;

        return null;
    }

    public static IdentifiableType GetCraft(string name)
    {
        foreach (IdentifiableType type in crafts.GetAllMembersArray())
            if (type.name.ToUpper() == name.ToUpper())
                return type;

        return null;
    }

    public static IdentifiableTypeGroup MakeNewGroup(IdentifiableType[] types, string groupName,
        IdentifiableTypeGroup[] subGroups = null)
    {
        var group = new IdentifiableTypeGroup();
        var typesList = new Il2CppSystem.Collections.Generic.List<IdentifiableType>();
        foreach (var type in types)
        {
            try
            {
                typesList.Add(type);
            }
            catch
            {
            }
        }

        var subGroupsList = new Il2CppSystem.Collections.Generic.List<IdentifiableTypeGroup>();
        foreach (var subGroup in subGroups)
        {
            try
            {
                subGroupsList.Add(subGroup);
            }
            catch
            {
            }
        }

        group._memberTypes = typesList;
        group._memberGroups = subGroupsList;
        return group;
    }
}