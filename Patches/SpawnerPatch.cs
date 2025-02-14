using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CottonLibrary.Patches;

[HarmonyPatch(typeof(DirectedActorSpawner))]
public class SpawnerPatch
{
    public static List<IntPtr> spawnerPointers = new List<IntPtr>();
    [HarmonyPostfix, HarmonyPatch(nameof(DirectedActorSpawner.Awake))]
    static void PostAwake(DirectedActorSpawner __instance)
    {
        spawnerPointers.Add(__instance.Pointer);
        __instance.gameObject.AddComponent<DestroyCatch>();
        foreach (var action in Library.executeOnSpawnerAwake)
        {
            action(__instance);
        }
    }

    [RegisterTypeInIl2Cpp(false)]
    public class DestroyCatch : MonoBehaviour
    {
        void OnDestroy()
        {
            spawnerPointers.Remove(GetComponent<DirectedActorSpawner>().Pointer);
        }
    }
    [HarmonyPrefix, HarmonyPatch(nameof(DirectedActorSpawner.MaybeReplaceId))]
    static void Replacement(DirectedActorSpawner __instance, ref IdentifiableType id)
    {
        foreach (var replacement in Library.spawnerReplacements)
        {
            try
            {
                if (Library.IsInZone(replacement.zones))
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