using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.DataModel;

namespace CottonLibrary.Patches;

[HarmonyPatch(typeof(SnareModel),nameof(SnareModel.GetGordoIdForBait))]
public class GordoCapturePatch
{
    public static void Postfix(SnareModel __instance, ref IdentifiableType __result)
    {
        var pair = Library.gordoBaitDict.FirstOrDefault(x => x.Key == __instance.baitTypeId.name);
            
        if (pair.Value)
            __result = pair.Value;
    }
}