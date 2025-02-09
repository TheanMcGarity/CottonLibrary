using System.Linq;
using CottonLibrary;

using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using MelonLoader;
using UnityEngine;

namespace CottonLibrary.Patches;


[HarmonyPatch(typeof(SavedGame), "Push", typeof(GameModel))]
public static class LoadPatch
{   
    static Exception Finalizer(Exception __exception)
    {
        if (__exception == null) return null;

        MelonLogger.Error($"Error occured while pushing saved game!\nThe error: {__exception}\n\nContinuing!");
        
        return null;
    }}
[HarmonyPatch(typeof(SavedGame), "Pull", typeof(GameModel))]
public static class SavePatch
{
    static Exception Finalizer(Exception __exception)
    {
        if (__exception == null) return null;
        MelonLogger.Error($"Error occured while pulling saved game!\nThe error: {__exception}\n\nContinuing!");
        
        return null;
    }
}