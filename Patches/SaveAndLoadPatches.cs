using System.Linq;
using CottonLibrary;

using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Persist;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
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
[HarmonyPatch(typeof(SavedGame))]
public static class SavePatch
{
    [HarmonyFinalizer, HarmonyPatch("Pull", typeof(GameModel))]
    static Exception MainSaveFinalizer(Exception __exception)
    {
        if (__exception == null) return null;
        MelonLogger.Error($"Error occured while pulling saved game!\nThe error: {__exception}\n\nContinuing!");
        
        return null;
    }

    [HarmonyPrefix, HarmonyPatch("Pull", typeof(GameModel), typeof(WorldV05), typeof(IdentifiableTypePersistenceIdLookupTable), typeof(PersistenceIdLookupTable<SceneGroup>))]
    static void WorldPullReplaceIdentTable(
        GameModel gameModel,
        WorldV05 world,
        ref IdentifiableTypePersistenceIdLookupTable identToPersistenceId,
        PersistenceIdLookupTable<SceneGroup> sceneToPersistenceId) 
        => identToPersistenceId = GameContext.Instance.AutoSaveDirector.SavedGame.IdentifiableTypePersistenceIdLookupTable;
}