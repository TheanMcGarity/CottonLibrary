using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.World;
using UnityEngine;
using Object = System.Object;

namespace CottonLibrary;

public static partial class Library
{
    public static GameObject SpawnGadget(this GameObject obj, Vector3 pos, Quaternion rot)
    {
        GameObject gadget =
            GadgetDirector.InstantiateGadget(obj, SystemContext.Instance.SceneLoader.CurrentSceneGroup, pos, rot);
        GameModel model = sceneContext.GameModel;
        GadgetModel gadgetModel = model.InstantiateGadgetModel(gadget.GetComponent<Gadget>().identType,
            SystemContext.Instance.SceneLoader.CurrentSceneGroup, pos);
        gadgetModel.eulerRotation = rot.eulerAngles;
        gadget.GetComponent<Gadget>()._model = gadgetModel;
        SceneContext.Instance.ActorRegistry.Register(gadget.GetComponent<Gadget>());
        return gadget;
    }

    public static GameObject SpawnGadget(this GameObject obj, Vector3 pos) =>
        SpawnGadget(obj, pos, Quaternion.identity);

    public static GameObject SpawnGadget(this GadgetDefinition obj, Vector3 pos, Quaternion rot) =>
        SpawnGadget(obj.prefab, pos, rot);

    public static GameObject SpawnGadget(this GadgetDefinition obj, Vector3 pos) =>
        SpawnGadget(obj.prefab, pos, Quaternion.identity);internal static Gadget RaycastForGadget()
    {
        if (Physics.Raycast(new Ray(Camera.main.transform.position, Camera.main.transform.forward), out var hit))
        {
            Transform currentParent = hit.collider.transform.parent;

            for (int i = 0; i < 10 && currentParent != null; i++)
            {
                Gadget gadgetComponent = currentParent.GetComponent<Gadget>();

                if (gadgetComponent != null)
                {
                    return gadgetComponent;
                }

                currentParent = currentParent.parent;
            }

            return null;
        }

        return null;
    }

    /*public static GadgetDefinition CreateGadget(string name, GameObject prefab, PurchaseCost cost, bool buyInPairs,
        bool noPickup, GadgetDefinition.Types gadgetType)
    {
        var definition = UnityEngine.Object.Instantiate(Resources.FindObjectsOfTypeAll<GadgetDefinition>().First());

        definition.CraftingCosts = cost;
        definition.name = name;
        definition.referenceId = $"GadgetDefinition.{name}";
        definition.prefab = prefab;
        definition.Type = gadgetType;
        definition.BuyInPairs = buyInPairs;
        definition.DestroyOnRemoval = noPickup;
        
        return definition;
    }*/
    
    /// <summary>
    /// Creates a new teleporter
    /// </summary>
    /// <param name="name">Name of the gadget in the code</param>
    /// <param name="cost">The cost to craft</param>
    /// <param name="color1">Example for a pink teleporter: <c>new Color(0.283f, 0.1001f, 0.1937f, 0f)</c></param>
    /// <param name="color2">Example for a pink teleporter: <c>new Color(0.8019f, 0.4861f, 0.4577f, 0.1216f)</c></param>
    /// <param name="color3">Example for a pink teleporter: <c>new Color(0.3679f, 0.1406f, 0.2364f, 0.6863f)</c></param>
    /// <param name="color4">Example for a pink teleporter: <c>new Color(0.4157f, 0.3412f, 0.3412f, 0f)</c></param>
    /// <returns></returns>
    public static GadgetDefinition CreateTeleporter(string name, PurchaseCost cost, Color color1, Color color2, Color color3, Color color4)
    {
        var definition = UnityEngine.Object.Instantiate(Get<GadgetDefinition>("TeleporterPink"));
        
        definition.name = name;
        definition.referenceId = $"GadgetDefinition.{name}";
        definition.SetupForSaving(definition.referenceId);
        definition.prefab = UnityEngine.Object.Instantiate(definition.prefab);

        definition.prefab.GetComponent<Gadget>().identType = definition;
        
        var mat = definition.prefab.transform.GetChild(0).GetChild(2).GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material;
        mat.SetColor("_Color00", color1);
        mat.SetColor("_Color01", color2);
        mat.SetColor("_Color02", color3);
        mat.SetColor("_Color10", color4);
        
        return definition;
    }
}