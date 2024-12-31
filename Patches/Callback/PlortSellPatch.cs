using HarmonyLib;
using Il2Cpp;

namespace CottonLibrary.Patches.Callback;

[HarmonyPatch(typeof(EconomyDirector), nameof(EconomyDirector.RegisterSold))]
static class PlortSellPatch
{
    public static void Postfix(EconomyDirector __instance, IdentifiableType id, int count)
    {
        Callbacks.Invoke_onPlortSold(count, id);
    }
}