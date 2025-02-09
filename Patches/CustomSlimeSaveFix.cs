using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Persist;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Il2CppMonomiPark.UnitPropertySystem;
using Il2CppSystem.Text;
using MelonLoader;
using static CottonLibrary.Library;
using Selections = Il2CppSystem.Collections.Generic.Dictionary<int, Il2Cpp.SlimeAppearance.AppearanceSaveSet>;
using Unlocks = Il2CppSystem.Collections.Generic.Dictionary<int, Il2CppSystem.Collections.Generic.List<Il2Cpp.SlimeAppearance.AppearanceSaveSet>>;

namespace CottonLibrary.Patches;

[HarmonyPatch(typeof(AppearancesModel),nameof(AppearancesModel.Pull))]
public class AppearancesSaveFix
{
    public static bool Prefix(AppearancesModel __instance, ref AppearancesV01 __result, IdentifiableTypePersistenceIdLookupTable identifiableTypeToPersistenceId)
    {
        
        __result = new AppearancesV01();

        Selections selections = new Selections();
        Unlocks unlocks = new Unlocks();

        foreach (var selection in __instance.AppearanceSelections._selections)
        {
            gameContext.AutoSaveDirector.SavedGame.identifiableTypeToPersistenceId.RefreshIfNotFound(selection.Key);
            
            selections.Add(gameContext.AutoSaveDirector.SavedGame.identifiableTypeToPersistenceId._reverseIndex[selection.key.ReferenceId], selection.value.SaveSet);
        }

        foreach (var unlock in __instance.AppearanceSelections._unlocks)
        {
            var list = new Il2CppSystem.Collections.Generic.List<SlimeAppearance.AppearanceSaveSet>();

            foreach (var app in unlock.value)
            {
                list.Add(app.SaveSet);
            }
            
            unlocks.Add(gameContext.AutoSaveDirector.SavedGame.identifiableTypeToPersistenceId._reverseIndex[unlock.key.ReferenceId], list);
        }

        __result.Unlocks = unlocks;
        __result.Selections = selections;
        
        return false;
    }
}

[HarmonyPatch(typeof(SavedGame), nameof(SavedGame.BuildActorData))]
public class CustomActorSaveFix
{
    public static bool Prefix(
        SavedGame __instance, 
        ref ActorDataV02 __result,
        ActorModel actorModel, 
        IdentifiableTypePersistenceIdLookupTable identToPersistenceId,
        PersistenceIdLookupTable<SceneGroup> sceneToPersistenceId,
        PersistenceIdLookupTable<StatusEffectDefinition> statusEffectToPersistenceId)
    {
        var identTable = gameContext._AutoSaveDirector_k__BackingField._savedGame
            .IdentifiableTypePersistenceIdLookupTable;
        
        identTable.RefreshIfNotFound(actorModel.ident);
        
        ActorDataV02 actorDataV = new ActorDataV02();

        IdentifiableType ident = actorModel.ident;
        actorDataV.TypeId = identTable.GetPersistenceId(ident);

        actorDataV.ActorId = actorModel.actorId.Value;

        Vector3V01 position = new Vector3V01();
        position.Value = actorModel.lastPosition;
        actorDataV.Pos = position;

        Vector3V01 rotation = new Vector3V01();
        rotation.Value = actorModel.lastRotation.eulerAngles;
        actorDataV.Rot = rotation;

        SceneGroup sceneGroup = actorModel.SceneGroup;
        actorDataV.SceneGroup = sceneToPersistenceId.GetPersistenceId(sceneGroup);

        var emotions = new SlimeEmotionDataV01();
        emotions.EmotionData = new Il2CppSystem.Collections.Generic.Dictionary<SlimeEmotions.Emotion, float>();
        emotions.EmotionData.Add(SlimeEmotions.Emotion.FEAR, actorModel.TryCast<SlimeModel>() != null ? actorModel.Cast<SlimeModel>().Emotions.x : 0f);
        emotions.EmotionData.Add(SlimeEmotions.Emotion.HUNGER, actorModel.TryCast<SlimeModel>() != null ? actorModel.Cast<SlimeModel>().Emotions.y : 0f);
        emotions.EmotionData.Add(SlimeEmotions.Emotion.AGITATION, actorModel.TryCast<SlimeModel>() != null ? actorModel.Cast<SlimeModel>().Emotions.z : 0f);
        emotions.EmotionData.Add(SlimeEmotions.Emotion.SLEEPINESS, actorModel.TryCast<SlimeModel>() != null ? actorModel.Cast<SlimeModel>().Emotions.w : 0f);
        actorDataV.Emotions = emotions;
       
        
        Il2CppSystem.Collections.Generic.List<StatusEffectV01> statusEffects = new Il2CppSystem.Collections.Generic.List<StatusEffectV01>();
        foreach (var effect in actorModel.statusEffects)
        {
            var effectV01 = new StatusEffectV01()
            {
                ExpirationTime = effect.value.ExpirationTime,
                ID = statusEffectToPersistenceId.GetPersistenceId(effect.key)
            };
            statusEffects.Add(effectV01);
        }
        actorDataV.StatusEffects = statusEffects;
        
        actorDataV.CycleData = new ResourceCycleDataV01();

        if (actorModel is SlimeModel slimeModel)
        {
            slimeModel.Pull(ref actorDataV, identTable);
        }
        else if (actorModel is AnimalModel animalModel)
        {
            animalModel.Pull(ref actorDataV, identTable);
        }
        else if (actorModel is ProduceModel produceModel)
        {
            produceModel.Pull(out var state, out var time);
            actorDataV.CycleData.State = state;
            actorDataV.CycleData.ProgressTime = time;
        }
        else if (actorModel is StatueFormModel statueFormModel)
        {
            actorDataV.IsStatue = true;
        }
        
        __result = actorDataV;
        
        return false;
    }
}
/*
[HarmonyPatch]
public class GameV06WriteFix
{
    private static int NullableListCount<T>(Il2CppSystem.Collections.Generic.List<T> list)
    {
        int i = 0;
        
        foreach (var item in list)
        {
            if (item != null)
                i++;
        }

        return i;
    }
    public static void WriteNullableList<T>(Il2CppSystem.Collections.Generic.List<T> list, Il2CppSystem.IO.BinaryWriter writer) where T : PersistedDataSet
    {
        
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));
    
        if (list == null)
        {
            writer.Write(0);
            return;
        }

        writer.Write(NullableListCount(list));

        foreach (T item in list)
        {
            item?.Write(writer.BaseStream);
        }
    }
    
    // thanks to ChatGPT for restoring the C# based off of the ISIL code. I dont know how i would be able to read this otherwise (look below)
    //
    // 134 Move rdx, rax
    // 135 Move rcx, rsi
    // 136 Call PersistedDataSet.Write, rcx, rdx
    // 137 Move rdx, [rbx]
    // 138 Move rcx, rbx
    // 139 Move rsi, [rdi+176]

    [HarmonyPrefix, HarmonyPatch(typeof(GameV06), nameof(GameV06.WriteGameData))]
    static bool GameDataWriteFix(GameV06 __instance, Il2CppSystem.IO.BinaryWriter writer)
    {
        
        // Writing Scene Group Index
        __instance.SceneGroupIndex?.Write(writer.BaseStream);

        // Writing Weather Index
        __instance.WeatherIndex?.Write(writer.BaseStream);

        // Writing World Data
        __instance.World?.Write(writer.BaseStream);

        // Writing Player Data
        __instance.Player?.Write(writer.BaseStream);

        // Writing Ranch Data
        __instance.Ranch?.Write(writer.BaseStream);

        // Writing Actors List
        if (__instance.Actors != null)
        {
            PersistedDataSet.WriteList(writer, __instance.Actors);
        }

        // Writing Pedia Data
        __instance.Pedia?.Write(writer.BaseStream);

        // Writing Appearances Data
        __instance.Appearances?.Write(writer.BaseStream);

        // Writing Secret Style Discoveries
        __instance.SecretStyleDiscoveries?.Write(writer.BaseStream);

        // Writing Event Record
        __instance.EventRecord?.Write(writer.BaseStream);

        // Writing Component Index Table
        __instance.ComponentIndex?.Write(writer.BaseStream);

        // Writing Status Effect Index Table
        __instance.StatusEffectIndex?.Write(writer.BaseStream);

        // Writing Disruption Area Index Table
        __instance.DisruptionAreaIndex?.Write(writer.BaseStream);

        // Writing Weather Data
        __instance.Weather?.Write(writer.BaseStream);

        return false;
    }

    // Here is how this version of the load function works:
    [HarmonyPrefix, HarmonyPatch(typeof(PersistedDataSet), nameof(PersistedDataSet.Load), typeof(Il2CppSystem.IO.Stream), typeof(bool))]
    static bool DataPayloadError(PersistedDataSet __instance, Il2CppSystem.IO.Stream stream, bool skipPayloadEnd)
    {
        // Materialize a binary reader
        Il2CppSystem.IO.BinaryReader binaryReader = new Il2CppSystem.IO.BinaryReader(stream, Encoding.UTF8);
        
        // read the Identifier (string) and Version (UInt)
        if (__instance.IsValidHeader(binaryReader))
        {
            // idfk
            short num = binaryReader.ReadInt16();
            
            // Load the actual save data
            __instance.LoadData(binaryReader);

            if (!skipPayloadEnd)
            {
                // Its a real mystery
                short num2 = binaryReader.ReadInt16();
            }
        }
        else
        {
            throw new InvalidDataException("Data Payload Error - Patch failed to fix it.");
        }
        
        binaryReader.Dispose();
        
        return false;
    }
    
    // It does give some errors when you try to load it but it doesnt seem to cause problems so im just muting it.
    [HarmonyFinalizer, HarmonyPatch(typeof(GameV06), nameof(GameV06.LoadGameData))]
    static Exception GameV06RemoveError() => null;
    
    [HarmonyFinalizer, HarmonyPatch(typeof(PersistedDataSet), nameof(PersistedDataSet.Load), typeof(Il2CppSystem.IO.Stream), typeof(bool))]
    static Exception LoadRemoveError() => null;
}*/