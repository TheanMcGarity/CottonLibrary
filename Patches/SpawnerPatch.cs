using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppMono.Security.X509;
using MelonLoader;
using UnityEngine;

namespace CottonLibrary.Patches;

[HarmonyPatch(typeof(DirectedActorSpawner), nameof(DirectedActorSpawner.Awake))]
public class SpawnerPatch
{
    [HarmonyPostfix]
    static void PostAwake(DirectedActorSpawner __instance)
    {
        foreach (var action in Library.executeOnSpawnerAwake)
        {
            action(__instance);
        }
    }
}
//[HarmonyPatch(typeof(DirectedActorSpawner), nameof(DirectedActorSpawner.MaybeReplaceId))]
public class SpawnerPatch2
{
    //[HarmonyPrefix]
    static bool Replacement(DirectedActorSpawner __instance, ref IdentifiableType __result, IdentifiableType id)
    {
        if (!__instance) return false;
        if (__instance.WasCollected) return false;
        
        foreach (var replacement in Library.spawnerReplacements)
        {
            try
            {
                if (Library.IsInZone(replacement.zones))
                {
                    var chance = Randoms.SHARED.GetProbability(1f / replacement.chance);
                    if (chance)
                    {
                        __result = replacement.ident;
                        return false;
                    }
                }
            }
            catch { }
        }

        __result = id;
        
        return false;
    }
}