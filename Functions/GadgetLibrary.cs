using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.World;
using UnityEngine;

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
}