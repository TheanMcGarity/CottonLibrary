using MelonLoader;
using CottonLibrary;
using HarmonyLib;
using PureLargosSR2;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Slime;
using UnityEngine;
using static CottonLibrary.Library;
using Object = UnityEngine.Object;


[assembly: MelonInfo(typeof(PureLargosEntry), "Blank Slime", "1.0.0", "PinkTarr & Aidanamite")]

namespace PureLargosSR2;

[HarmonyPatch(typeof(SlimeAppearanceApplicator), nameof(SlimeAppearanceApplicator.SetExpression))]
public class ExpressionFix
{
    static bool Prefix(SlimeAppearanceApplicator __instance, SlimeFace.SlimeExpression slimeExpression)
    {
        if (__instance.SlimeDefinition.name != "Blank") return true;

        SlimeExpressionFace expressionFace = __instance.Appearance.Face.GetExpressionFace(slimeExpression);
        foreach (SlimeAppearanceApplicator.FaceRenderer faceRenderer in __instance._faceRenderers)
        {
            Material[] sharedMaterials = faceRenderer.Renderer.sharedMaterials;
            int eyeMatIDX = sharedMaterials.Length - 2;
            int mouthMatIDX = sharedMaterials.Length - 1;
            if (faceRenderer.ShowEyes != faceRenderer.ShowMouth)
            {
                eyeMatIDX = mouthMatIDX;
            }
            if (faceRenderer.ShowEyes && expressionFace.Eyes != null)
            {
                sharedMaterials[eyeMatIDX] = expressionFace.Eyes;
            }
            if (faceRenderer.ShowMouth && expressionFace.Mouth != null)
            {
                sharedMaterials[mouthMatIDX] = expressionFace.Mouth;
            }
            faceRenderer.Renderer.sharedMaterials = sharedMaterials;
        }

        return false;
    }
}
[HarmonyPatch]
public class SlimeDietFix
{
    [HarmonyPrefix, HarmonyPatch(typeof(SlimeDefinition),nameof(SlimeDefinition.LoadDietFromBaseSlimes))]
    static bool LargoDietFix(SlimeDefinition __instance)
    {
        return !__instance.name.Contains("Blank");
    }
    [HarmonyPostfix, HarmonyPatch(typeof(SlimeDiet),nameof(SlimeDiet.RefreshEatMap))]
    static void PureLargoTransformEatMapFix(SlimeDiet __instance, SlimeDefinitions definitions, SlimeDefinition definition)
    {
        if (!PureLargosEntry.slimeToPureLargo.TryGetValue(definition.name, out var _)) return;
        
        if (!TryGetEatMap(__instance, PureLargosEntry.Plort, out var _2))
        {
            __instance.EatMap.Add(CreateEatmap(SlimeEmotions.Emotion.AGITATION, 0.5f, null, PureLargosEntry.Plort, PureLargosEntry.slimeToPureLargo[definition.name]));
        }
    }
}

public class PureLargosEntry : CottonModInstance<PureLargosEntry>
{
    Material SetMouthMaterial(Material original)
    {
        
        bool addBlankToName = !original.name.Contains("Blank");
        
        var material = Object.Instantiate(original);

        material.SetColor("_MouthTop", PureLargosEntry.BlankFace);
        material.SetColor("_MouthMid", PureLargosEntry.BlankFace);
        material.SetColor("_MouthBot", PureLargosEntry.BlankFace);
        
        material.name = material.name.Replace("(Clone)", "").Replace(" (Instance)", "");

        if (addBlankToName) material.name += "Blank";
        
        Object.DontDestroyOnLoad(material);
        
        return material;
    }

    Material SetEyesMaterial(Material original)
    {
        bool addBlankToName = !original.name.Contains("Blank");
        
        var material = Object.Instantiate(original);

        material.SetColor("_EyeRed", PureLargosEntry.BlankFace);
        material.SetColor("_EyeGreen", PureLargosEntry.BlankFace);
        material.SetColor("_EyeBlue", PureLargosEntry.BlankFace);
        
        material.name = material.name.Replace("(Clone)", "").Replace(" (Instance)", "");

        if (addBlankToName) material.name += "Blank";
        
        Object.DontDestroyOnLoad(material);
        
        return material;
    }
    void CreateFace(SlimeFace face)
    {
        var faces = new List<SlimeExpressionFace>();
        foreach (var faceEx in face.ExpressionFaces)
        {
            var newEx = new SlimeExpressionFace();
            if (faceEx.Eyes)
            {
                newEx.Eyes = SetEyesMaterial(faceEx.Eyes);
            }

            if (faceEx.Mouth)
            {
                newEx.Mouth = SetMouthMaterial(faceEx.Mouth);
            }
            
            newEx.SlimeExpression = faceEx.SlimeExpression;
            
            faces.Add(newEx);
        }

        face.ExpressionFaces = new Il2CppReferenceArray<SlimeExpressionFace>(faces.ToArray());
        
        face.OnEnable();
    }
    public static Dictionary<string, SlimeDefinition> slimeToPureLargo =
        new Dictionary<string, SlimeDefinition>();
    
    internal static IdentifiableType Plort => plort;
    
    private const byte BlankBrightness = 245;
    private const byte VacBrightness = 175;
    private const byte FaceBrightness = 15;
    public static readonly Color32 BlankBody = new(BlankBrightness, BlankBrightness, BlankBrightness, 255);
    public static readonly Color32 BlankFace = new(FaceBrightness, FaceBrightness, FaceBrightness, 255);
    public static readonly Color32 BlankVac = new(VacBrightness, VacBrightness, VacBrightness, 255);
    private const LargoSettings largoSettings =
        LargoSettings.KeepSecondBody |
        LargoSettings.KeepSecondColor |
        LargoSettings.KeepSecondTwinColor |
        LargoSettings.KeepSecondFace;


    private static SlimeDefinition slime;
    private static IdentifiableType plort;
    private static GameObject plortObject;
    private static GameObject slimeObject;

    private static Sprite slimeIcon;
    private static Sprite plortIcon;

    private static SlimeFace face;
    private static SlimeFace defaultFace;
    
    public override void LateSaveDirectorLoaded()
    {
        if (plortObject != null) return;
        
        var pink = GetSlime("Pink");
        plortObject = CreatePrefab("plortBlank", GetPlort("PhosphorPlort").prefab);
        slimeObject = CreatePrefab("slimeBlank", pink.prefab);

        slimeIcon = LoadPNG("slimeBlankIcon").ConvertToSprite();
        plortIcon = LoadPNG("plortBlankIcon").ConvertToSprite();
        
        slime = CreateSlimeDef(
            "Blank",
            BlankVac,
            slimeIcon,
            pink.AppearancesDefault[0],
            "BlankDefault",
            "SlimeDefinition.Blank");
        
        
        
        slime.MakeVaccable();
        
        plort = CreatePlortType(
            "Blank",
            BlankVac,
            plortIcon,
            "IdentifiableType.BlankPlort",
            0,
            0);
        
        
        slime.SetObjectPrefab(slimeObject);
        slimeObject.SetObjectIdent(slime);
        
        plort.SetObjectPrefab(plortObject);
        plortObject.SetObjectIdent(plort);

        SetPlortColor(BlankBody, BlankBody, BlankBody, plortObject);

        // make it so its built into the library. i dont have time rn
        var plortMat = plortObject.GetComponent<MeshRenderer>().material;
        plortMat.SetColor("_GlowTop", BlankFace);
        plortMat.SetColor("_GlowMiddle", BlankFace);
        plortMat.SetColor("_GlowBottom", BlankFace);
        
        var blankMat = Object.Instantiate(GetSlime("Phosphor").AppearancesDefault[0]._structures[0].DefaultMaterials[0]);

        blankMat.SetSlimeMatColors(BlankBody, BlankBody, BlankBody);
        
        blankMat.SetColor("_GlowTop", BlankFace);
        blankMat.SetColor("_GlowMiddle", BlankFace);
        blankMat.SetColor("_GlowBottom", BlankFace);

        defaultFace = slime.AppearancesDefault[0]._face;
        
        face = Object.Instantiate(defaultFace);

        face.name = "BlankSlimeFace";

        slime.AppearancesDefault[0]._face = face;
        
        slime.AppearancesDefault[0]._splatColor = BlankBody;
        
        blankMat.SetFloat("_GlossPower", 0);
        plortMat.SetFloat("_GlossPower", 0);
        
        slime.AppearancesDefault[0]._structures[0].DefaultMaterials[0] = blankMat;
        
        slime.AddProduceIdent(plort);


        var foodItems = new List<IdentifiableType>();
        
        foodItems.Add(GetCraft("SunSapCraft"));
        //foodItems.Add(GetCraft("StrangeDiamondCraft"));
        //foodItems.Add(GetCraft("DriftCrystalCraft"));
        //foodItems.Add(GetCraft("LightningMoteCraft"));
        //foodItems.Add(GetCraft("StormGlassCraft"));

        var foods = CreateIdentifiableGroup(
            AddTranslation(
                //"Rare Materials",
                "Sun Sap",
                "l.sun_sap_food_group",
                "UI"),
            "SunSapFoodGroup",
            foodItems,
            new List<IdentifiableTypeGroup>(),
            true);

        slime.Diet.MajorFoodIdentifiableTypeGroups = new Il2CppReferenceArray<IdentifiableTypeGroup>(new[] { foods });
        
        slime.RefreshEatmap();

        slime.localizedName = AddTranslation("Slime", "l.blank_slime");
        plort.localizedName = AddTranslation("Plort", "l.blank_plort");
        
        MakeSpawnableInZones(
            slime,
            null,
            SpawnLocations.LabyrinthDreamland,
            175f,
            SpawnerTypes.Slime,
            SpawningMode.ReplacementBasedSpawning);
        
        slime.CanLargofy = true;
        
        foreach (var baseSlime in baseSlimes.GetAllMembersArray())
        {
            SlimeDefinition definition = baseSlime.Cast<SlimeDefinition>();
            if (definition.CanLargofy)
            {
                var largo = CreateCompleteLargo(slime, definition, largoSettings, new LargoOverrides
                {
                    overrideTranslation = "{1} Largo",
                    overridePediaSuffix = "{1}_largo",
                });
                List<IdentifiableTypeGroup> foodGroups = new List<IdentifiableTypeGroup>();
                bool passed = false;
                foreach (var fg in largo.Diet.MajorFoodIdentifiableTypeGroups)
                {
                    if (!passed)
                    {
                        passed = true;
                        continue;
                    }
                    foodGroups.Add(fg);
                }

                largo.Diet.MajorFoodIdentifiableTypeGroups = new Il2CppReferenceArray<IdentifiableTypeGroup>(foodGroups.ToArray());
                
                largo.Diet.ProduceIdents = new Il2CppReferenceArray<IdentifiableType>(new[] { largo.Diet.ProduceIdents[1], largo.Diet.ProduceIdents[1] });

                if (!TryGetEatMap(definition.Diet, plort, out var tryGetEatMap))
                {
                    definition.AddEatmapToSlime(CreateEatmap(SlimeEmotions.Emotion.AGITATION, 0, null, plort, largo));
                }
                
                slimeToPureLargo.Add(definition.name, largo);
            }
        }
        foreach (var plortEatmap in GetEatMapsByName(slime.Diet, "Plort"))
        {
            plortEatmap.BecomesIdent = GetBaseSlime(plortEatmap.EatsIdent.name.Replace("Plort", ""));
        }
        CreateFace(slime.AppearancesDefault[0]._face);
    }
}