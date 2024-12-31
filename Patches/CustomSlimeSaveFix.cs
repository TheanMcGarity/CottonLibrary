using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Persist;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Il2CppMonomiPark.UnitPropertySystem;
using MelonLoader;
using static CottonLibrary.LibraryUtils;
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
        
        identTable.RefreshIfNotFound(actorModel.ident); // Need to do this so it doesnt break.
        
        var ret = new ActorDataV02();
        
        ret.ActorId = actorModel.actorId.Value;
        ret.TypeId = identTable.GetPersistenceId(actorModel.ident);
        ret.DestroyTime = actorModel.destroyTime;
        ret.Fashions = new Il2CppSystem.Collections.Generic.List<int>();
        
        ret.CycleData = new ResourceCycleDataV01()
        {
            ProgressTime = 0,
            State = ResourceCycle.State.UNRIPE
        };


        ret.Emotions = new SlimeEmotionDataV01();

        ret.Pos = new Vector3V01()
        {
            Value = actorModel.lastPosition
        };
        ret.Rot = new Vector3V01()
        {
            Value = actorModel.lastRotation.eulerAngles
        };

        ret.PrismaActorData = new PrismaActorDataV01()
        {
            UnstableBurstTime = float.NaN
        };
        
        var res = actorModel.TryCast<ProduceModel>();
        var chicken = actorModel.TryCast<AnimalModel>();
        var slime = actorModel.TryCast<SlimeModel>();
        
        if (res != null)
        {
            ret.CycleData.ProgressTime = res.progressTime;
            ret.CycleData.State = res.state;
            ret.PrismaActorData.UnstableBurstTime = res.unstableBurstTime;
        }
        else if (chicken != null)
        {
            ret.TransformTime = chicken.transformTime;
            ret.ReproduceTime = chicken.nextReproduceTime;
            ret.PrismaActorData.UnstableBurstTime = chicken.unstableBurstTime;
            
            foreach (var fashion in chicken.fashions)
                ret.Fashions.Add(identTable.GetPersistenceId(fashion));
        }
        else if (slime != null)
        {
            var dict = new Il2CppSystem.Collections.Generic.Dictionary<SlimeEmotions.Emotion, float>();
            
            dict.Add((SlimeEmotions.Emotion)0, slime.Emotions.x);
            dict.Add((SlimeEmotions.Emotion)1, slime.Emotions.y);
            dict.Add((SlimeEmotions.Emotion)2, slime.Emotions.z);
            dict.Add((SlimeEmotions.Emotion)3, slime.Emotions.w);
            
            ret.Emotions.EmotionData = dict;
            
            ret.IsGlitch = slime.isGlitch;
            ret.IsFeral = slime.isFeral;
            ret.IsStatue = slime.isStatue;
            ret.IsSleeping = slime.isSleeping;
            
            foreach (var fashion in slime.fashions)
                ret.Fashions.Add(identTable.GetPersistenceId(fashion));
            
            Il2CppSystem.Collections.Generic.List<StatusEffectV01> effects = new Il2CppSystem.Collections.Generic.List<StatusEffectV01>();

            // please support for status effects
            ret.StatusEffects = effects;
        }
        
        __result = ret;
        
        return false;
    }
}

[HarmonyPatch] // i hate patching generic method 
public class SaveWriteTooFarFix
{
    public static MethodBase TargetMethod() // guh
    {
        var methods = typeof(PersistedDataSet).GetMethods(BindingFlags.Public | BindingFlags.Static);

        foreach (var method in methods)
            if (method.Name == "WriteList" &&
                method.IsGenericMethod &&
                method.GetParameters().Length == 2
                )
                return method.MakeGenericMethod(typeof(ActorDataV02));

        throw new Exception("Target method not found!");
    }
    
    public static bool Prefix(PersistedDataSet __instance, Il2CppSystem.IO.BinaryWriter writer, Il2CppSystem.Collections.Generic.List<ActorDataV02> items)
    {
        ulong count = 0;
        foreach (var getCount in items)
            if (getCount != null)
                count++;
        
        writer.Write(count);
        
        foreach (var write in items)
        {
            if (write != null)
                write.WriteData(writer);
        }
        
        return false;
    }
}