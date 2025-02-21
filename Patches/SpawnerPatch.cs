using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppMono.Security.X509;
using MelonLoader;
using UnityEngine;

namespace CottonLibrary.Patches;

[HarmonyPatch(typeof(DirectedActorSpawner))]
public class SpawnerPatch
{
    [HarmonyPostfix, HarmonyPatch(nameof(DirectedActorSpawner.Awake))]
    static void PostAwake(DirectedActorSpawner __instance)
    {
        foreach (var action in Library.executeOnSpawnerAwake)
        {
            action(__instance);
        }
    }

    [HarmonyPrefix, HarmonyPatch(nameof(DirectedActorSpawner.MaybeReplaceId))]
    static void Replacement(DirectedActorSpawner __instance, ref IdentifiableType id)
    {
        if (__instance.WasCollected) return;
        
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