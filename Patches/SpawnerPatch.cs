using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CottonLibrary.Patches;

[HarmonyPatch(typeof(DirectedActorSpawner))]
public class SpawnerPatch
{
    [HarmonyPostfix, HarmonyPatch(nameof(DirectedActorSpawner.Awake))]
    static void OnAwake(DirectedActorSpawner __instance)
    {
        foreach (var action in Library.executeOnSpawnerAwake)
        {
            action(__instance);
        }
    }
    [HarmonyPrefix, HarmonyPatch(nameof(DirectedActorSpawner.MaybeReplaceId))]
    static void Replacement(DirectedActorSpawner __instance, ref IdentifiableType id)
    {
        if (__instance.WasCollected)
        {
            return;
        }
        if (!SystemContext.Instance.SceneLoader.IsCurrentSceneGroupGameplay() || SystemContext.Instance.SceneLoader.IsSceneLoadInProgress) return;
        
        if (!__instance) return;
        
        foreach (var replacement in Library.spawnerReplacements)
        {
            try
            {

                if (Library.ContainsZoneName(__instance.gameObject.scene.name, replacement.zones.ToList()))
                {
                    var chance = Randoms.SHARED.GetProbability(1f / replacement.chance);
                    if (chance)
                    {
                        id = replacement.ident;
                        return;
                    }
                }
            }
            catch { }
        }
    }
}