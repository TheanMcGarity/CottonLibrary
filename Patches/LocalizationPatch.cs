﻿using Il2CppMonomiPark.SlimeRancher.UI.Localization;
using System.Collections;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
namespace CottonLibrary.Patches;

[HarmonyPatch(typeof(LocalizationDirector), "LoadTables")]
public static class LocalizationDirectorLoadTablePatch
{
    public static void Postfix(LocalizationDirector __instance)
    {
        MelonCoroutines.Start(LoadTable(__instance));
    }
    private static IEnumerator LoadTable(LocalizationDirector director)
    {
        yield return null;
        yield return null;
        foreach (var keyValuePair in director.Tables)
        {
            Dictionary<string, string> dictionary;
            if (Library.addedTranslations.TryGetValue(keyValuePair.Key, out dictionary))
            {
                foreach (KeyValuePair<string, string> keyValuePair2 in dictionary)
                {
                    keyValuePair.Value.AddEntry(keyValuePair2.Key, keyValuePair2.Value);
                }
            }
        }
        yield break;
    }
}