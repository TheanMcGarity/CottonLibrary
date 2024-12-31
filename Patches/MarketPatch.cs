using Il2CppMonomiPark.SlimeRancher.UI;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using static CottonLibrary.LibraryUtils;
using HarmonyLib;
using Il2Cpp;
namespace CottonLibrary.Patches;

[HarmonyPatch(typeof(MarketUI), nameof(MarketUI.Start))]
public static class MarketPatch
{
    public static void Prefix(MarketUI __instance)
    {
        List<MarketUI.PlortEntry> marketPlortEntriesList = new List<MarketUI.PlortEntry>();
        foreach (var pair in marketPlortEntries)
            if (!pair.Value)
                marketPlortEntriesList.Add(pair.Key);

        __instance.plorts = (from x in __instance.plorts
                             where !marketPlortEntriesList.Exists
                                 ((y) => y == x)
                             select x).ToArray();


        try
        {
            __instance.plorts = (from x in __instance.plorts
                                 where !removeMarketPlortEntries.Exists((IdentifiableType y) => y.name != x.identType.name)
                                 select x).ToArray();
        }
        catch { }



        __instance.plorts = __instance.plorts.ToArray().AddRangeToArray(marketPlortEntriesList.ToArray());
        __instance.plorts = __instance.plorts.Take(34).ToArray();

    }
    public static void Postfix(MarketUI __instance)
    {
        __instance.plorts = __instance.plorts.Take(34).ToArray();
    }
}