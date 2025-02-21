using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.DataModel;

namespace CottonLibrary.Patches;

[HarmonyPatch(typeof(SnareModel),nameof(SnareModel.GetGordoIdForBait))]
public class GordoCapturePatch
{
    public static void Postfix(SnareModel __instance, ref IdentifiableType __result)
    {
        try
        {
            var pair = Library.gordoBaitDict.First(x => x.Key == __instance.baitTypeId.name);
            
            __result = pair.Value;
        }
        catch
        {
            // It did not find the bait.
        }
    }
}