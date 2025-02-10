using MelonLoader;
using CottonLibrary;
using PureLargosSR2;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using static CottonLibrary.Library;
using Object = UnityEngine.Object;


[assembly: MelonInfo(typeof(PureLargosEntry), "Blank Slime", "1.0.0", "PinkTarr & Aidanamite")]

namespace PureLargosSR2;

public class PureLargosEntry : CottonModInstance<PureLargosEntry>
{
    private const byte BlankBrightness = 245;
    private const byte VacBrightness = 175;
    private const byte FaceBrightness = 15;
    public static readonly Color32 BlankBody = new(BlankBrightness, BlankBrightness, BlankBrightness, 255);
    public static readonly Color32 BlankFace = new(FaceBrightness, FaceBrightness, FaceBrightness, 255);
    public static readonly Color32 BlankVac = new(VacBrightness, VacBrightness, VacBrightness, 255);
    private const LargoSettings largoSettings =
        LargoSettings.KeepFirstBody |
        LargoSettings.KeepFirstColor |
        LargoSettings.KeepFirstTwinColor |
        LargoSettings.KeepFirstFace;


    private static SlimeDefinition slime;
    private static IdentifiableType plort;
    private static GameObject plortObject;
    private static GameObject slimeObject;
    
    public override void LateSaveDirectorLoaded()
    {
        if (plortObject != null) return;
        
        var pink = GetSlime("Pink");
        plortObject = CreatePrefab("plortBlank", GetPlort("PhosphorPlort").prefab);
        slimeObject = CreatePrefab("slimeBlank", pink.prefab);
        
        slime = CreateSlimeDef(
            "Blank",
            BlankVac,
            null,
            pink.AppearancesDefault[0],
            "BlankDefault",
            "SlimeDefinition.Blank");
        
        plort = CreatePlortType(
            "Blank",
            BlankVac,
            null,
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
        
        // _GlossPower
        blankMat.SetFloat(7,0);
        plortMat.SetFloat(10,0);
        
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
                "l.SunSapFoodGroup",
                "UI"),
            "SunSapFoodGroup",
            foodItems,
            new List<IdentifiableTypeGroup>(),
            true);

        slime.Diet.MajorFoodIdentifiableTypeGroups = new Il2CppReferenceArray<IdentifiableTypeGroup>(new[] { foods });
        
        slime.RefreshEatmap();

        slime.localizedName = AddTranslation("Slime", "l.BlankSlime");
        plort.localizedName = AddTranslation("Plort", "l.BlankPlort");
        
        MakeSpawnableInZones(
            slime,
            null,
            SpawnLocations.LabyrinthDreamland,
            125f,
            SpawnerTypes.Slime,
            SpawningMode.ReplacementBasedSpawning);

        foreach (var baseSlime in baseSlimes.GetAllMembersArray())
        {
            SlimeDefinition definition = baseSlime.Cast<SlimeDefinition>();
            if (definition.CanLargofy)
            {
                CreateCompleteLargo(slime, definition, largoSettings, new LargoOverrides
                {
                    mergeDiet = LargoOverrides.FavoredSlime.Two,
                    mergeAppearances = LargoOverrides.FavoredSlime.Two,
                    overrideTranslation = "{1} Pure-Largo",
                });
            }
        }
    }
}