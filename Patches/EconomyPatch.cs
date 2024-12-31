using System.Linq;
using CottonLibrary;

using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.World;
using UnityEngine;

namespace CottonLibrary.Patches;


[HarmonyPatch(typeof(EconomyDirector), "InitModel")]
public static class EconomyPatch
{
    public static void Prefix(EconomyDirector __instance)
    {
        foreach (var entry in LibraryUtils.marketData)
        {
            var defualtValues = new PlortDefaultValues.Entry()
            {
                FullSaturation = entry.Value.SAT,
                Type = entry.Key,
                Value = entry.Value.VAL
            };
            
            __instance._plortDefaultValues.Entries.Add(defualtValues);
        }
    }
}