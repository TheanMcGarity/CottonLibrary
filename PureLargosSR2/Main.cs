using MelonLoader;
using CottonLibrary;
using HarmonyLib;
using PureLargosSR2;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using static CottonLibrary.Library;
using Object = UnityEngine.Object;


[assembly: MelonInfo(typeof(PureLargosEntry), "Blank Slime", "1.0.0", "PinkTarr & Aidanamite")]

namespace PureLargosSR2;

[HarmonyPatch(typeof(SlimeDefinition),nameof(SlimeDefinition.LoadDietFromBaseSlimes))]
public class SlimeDietFix
{
    static bool Prefix(SlimeDefinition __instance)
    {
        return !__instance.name.Contains("Blank");
    }
}

[HarmonyPatch(typeof(SlimeEat),nameof(SlimeEat.EatAndTransform))]
public class EatTransformPatch
{
    static bool Prefix(SlimeEat __instance, GameObject target, SlimeDiet.EatMapEntry em, bool immediateMode)
    {
        if (__instance.gameObject.name == "slimeBlank(Clone)")
            if (em.EatsIdent.IsPlort)
            {
                // he loves it
                for(int i = 0; i < 8; i++)
                    FXHelpers.PlayFX(FXHelpers.SpawnFX(__instance.EatFavoriteFX, __instance.transform.position, __instance.transform.rotation));
                
                Object.Destroy(target);
                return false;
            }
        
        return true;
    }
}

public class PureLargosEntry : CottonModInstance<PureLargosEntry>
{
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

        var face = Object.Instantiate(slime.AppearancesDefault[0]._face);
        
        // Code from blank slimes; it has been adjusted to fit my style.
        // Original code starting on this line inside the repository https://github.com/Aidanamite/BlankSlime/blob/master/BlankSlime/Main.cs#L119
        var faces = face.ExpressionFaces;
        var rep = new Dictionary<Material, Material>();
        for (int i = 0; i < faces.Length; i++)
        {
            if (faces[i].Eyes)
            {
                if (!rep.TryGetValue(faces[i].Eyes, out var eye))
                {
                    eye = new Material(faces[i].Eyes);
                    eye.CopyPropertiesFromMaterial(faces[i].Eyes);

                    eye.name = eye.name.Replace("(Clone)", "").Replace(" (Instance)", "") + "Blank";

                    eye.SetColor("_EyeRed", BlankFace);
                    eye.SetColor("_EyeGreen", BlankFace);
                    eye.SetColor("_EyeBlue", BlankFace);

                    rep.Add(faces[i].Eyes, eye);
                }

                faces[i].Eyes = eye;
            }

            if (faces[i].Mouth)
            {
                if (!rep.TryGetValue(faces[i].Mouth, out var mouth))
                {
                    mouth = new Material(faces[i].Mouth);
                    mouth.CopyPropertiesFromMaterial(faces[i].Mouth);

                    mouth.name = mouth.name.Replace("(Clone)", "").Replace(" (Instance)", "") + "Blank";
                    
                    mouth.SetColor("_MouthTop", BlankFace);
                    mouth.SetColor("_MouthMid", BlankFace);
                    mouth.SetColor("_MouthBot", BlankFace);
                    
                    rep.Add(faces[i].Mouth, mouth);
                }

                faces[i].Mouth = mouth;
            }
        }
        //
        // ^ Code basically ends here ^

        slime.AppearancesDefault[0]._face = face;
        face.OnEnable();
        
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
                
                largo.RefreshEatmap();
                definition.RefreshEatmap();
            }
        }
    }
}