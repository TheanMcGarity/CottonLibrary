using System.Linq;
using CottonLibrary;

using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.World;
using UnityEngine;

namespace CottonLibrary.Patches;


[HarmonyPatch(typeof(EconomyDirector), "InitModel")]
public static class EconomyPatch
{
    public static void Prefix(EconomyDirector __instance)
    {
        List<PlortDefaultValues.Entry> entries = new List<PlortDefaultValues.Entry>();
        foreach (var defaultEntry in __instance._plortDefaultValues.Entries)
        {
            entries.Add(defaultEntry);
        }
        foreach (var entry in Library.marketData)
        {
            var defualtValues = new PlortDefaultValues.Entry()
            {
                FullSaturation = entry.Value.SAT,
                Type = entry.Key,
                Value = entry.Value.VAL
            };
            
            entries.Add(defualtValues);
        }
        __instance._plortDefaultValues.Entries = new Il2CppReferenceArray<PlortDefaultValues.Entry>(entries.ToArray());
    }
}