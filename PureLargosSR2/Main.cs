// This is broken!

using MelonLoader;
using CottonLibrary;
using HarmonyLib;
using PureLargosSR2;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Il2CppMonomiPark.SlimeRancher.World;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Localization;
using static CottonLibrary.Library;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(PureLargosEntry), "Blank Slime", "1.0.0", "PinkTarr & Aidanamite")]

namespace PureLargosSR2;

[HarmonyPatch(typeof(VacuumItem), nameof(VacuumItem.ConsumeVacuumable))]
public class PediaGiveFix
{
    static void Postfix(VacuumItem __instance, GameObject gameObj)
    {
        try
        {
            PureLargosEntry.slimeToPureLargo.Where(x => x.Value.name == gameObj.GetIdent().name);
            __instance._pediaDir.Unlock(PureLargosEntry.pureLargoPedia);
        }
        catch
        {
            
        }
    }
}


[HarmonyPatch(typeof(SlimeAppearanceApplicator), nameof(SlimeAppearanceApplicator.SetExpression))]
public class ExpressionFix
{
    static bool Prefix(SlimeAppearanceApplicator __instance, SlimeFace.SlimeExpression slimeExpression)
    {
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
    public static IdentifiableType customFood;
    private static GameObject plortObject;
    private static GameObject slimeObject;
    private static GameObject customFoodObject;

    private static Sprite slimeIcon;
    private static Sprite plortIcon;
    
    public static FixedPediaEntry pureLargoPedia;

    private static SlimeFace face;
    private static SlimeFace defaultFace;

    private static GameObject foodObject;
    public override void LateSaveDirectorLoaded()
    {
        if (plortObject != null) return;
        
        var pink = GetSlime("Pink");
        plortObject = CreatePrefab("plortBlank", GetPlort("PhosphorPlort").prefab);
        slimeObject = CreatePrefab("slimeBlank", pink.prefab);

        slimeIcon = LoadPNG("slimeBlankIcon").ConvertToSprite();
        var gordoIcon = LoadPNG("iconGordoBlank").ConvertToSprite();
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

        var foodIcon = LoadPNG("iconVeggieBlank").ConvertToSprite();
        
        customFood = CreateBlankType("BlankVeggie", BlankVac, foodIcon, "IdentifiableType.BlankVeggie");
        var bundle = LoadBundle("food");
        var tex = bundle.LoadAsset<Texture2D>("foodTex");
        var mesh = bundle.LoadAsset<Mesh>("food");
        customFoodObject = CreateFoodObject(customFood, mesh, tex, Texture2D.blackTexture, 0.16f, new CapsuleColliderData() { length = 2.5f, radius = .8f }, out var baitObj);
        customFood.prefab = customFoodObject;
        
        customFood.localizedName = AddTranslation("<alpha=#55>Blank <alpha=#FF>Carrot", "t.blank_carrot");
        
        foodItems.Add(customFood);

        var foodGroup = CreateIdentifiableGroup(
            AddTranslation(
                "[[ <size=133%><color=red>Unknown</size></color> ]]",
                "t.blank_food_group",
                "UI"),
            "BlankFoodGroup",
            foodItems,
            new List<IdentifiableTypeGroup>(),
            true);
        
        foodGroup._icon = LoadPNG("iconFoodUnknown").ConvertToSprite();
        
        food._memberGroups.Add(foodGroup);
        GameContext.Instance.LookupDirector._identifiableTypeGroupMap._entries.FirstOrDefault(x => x.key.name == food.name)?.value._memberGroups.Add(foodGroup);
        
        customFood.groupType = foodGroup;
        
        slime.Diet.MajorFoodIdentifiableTypeGroups = new Il2CppReferenceArray<IdentifiableTypeGroup>(new[] { foodGroup });

        slime.Diet.FavoriteProductionCount = 1;
        slime.Diet.FavoriteIdents = new Il2CppReferenceArray<IdentifiableType>(new[] { customFood });
        
        slime.RefreshEatmap();

        slime.localizedName = AddTranslation("Slime", "t.blank_slime");
        plort.localizedName = AddTranslation("Plort", "t.blank_plort");
        
        MakeSpawnableInZones(
            slime,
            null,
            SpawnLocations.LabyrinthDreamland,
            175f,
            SpawnerTypes.Slime,
            SpawningMode.ReplacementBasedSpawning,
            RequiredConditions.None);
        
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

                if (!largo) continue;

                largo.Diet.MajorFoodIdentifiableTypeGroups = definition.Diet.MajorFoodIdentifiableTypeGroups;
                try
                {
                    largo.Diet.ProduceIdents = new Il2CppReferenceArray<IdentifiableType>(new[]
                        { definition.Diet.ProduceIdents[0], definition.Diet.ProduceIdents[0] });
                }
                catch
                {
                    MelonLogger.Error("Error with largo diet! Skipping produce idents.");
                }


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

        pureLargoPedia = CreateFixedPediaEntry(
            Get<FixedPediaEntry>("Largo"),
            "pure_largo",
            AddTranslation(
                "Pure Largo Slimes",
                "m.pure_largos.title",
                "Pedia"
            ),
            AddTranslation(
                "Twice the size, twice the love!",
                "m.pure_largos.intro",
                "Pedia"
            ),
            LoadPNG("iconPureLargoPedia").ConvertToSprite(),
            PediaCategoryType.Slimes,
            PediaDetail.Params(
                PediaDetail.Create(
                    0,
                    AddTranslation(
                        "A Pure Largo is a extremely rare variant of the Largo Slime. It is a largo of two of the same slime, or in simpler terms, an example would be 'Pink Pink Largo.' To make reading this easier it is automatically shortened to 'Pink Largo' by the Slimepedia identification tool.\n\nYou may be wondering how these form, and so are we. All we know is that when a slime eats a <b><u>Blank</u></b> Plort, (labeled as 'Plort') it transforms into a Pure Largo. It will not work the other way around!",
                        "m.pure_largos.slimeology",
                        "PediaPage"
                    ),
                    PediaDetailType.Slimeology
                ),
                PediaDetail.Create(
                    1,
                    AddTranslation(
                        "The risks have not been researched as of the time of writting...",
                        "m.pure_largos.risks",
                        "PediaPage"
                    ),
                    PediaDetailType.Risk
                ),
                PediaDetail.Create(
                    2,
                    AddTranslation(
                        "When a pure largo eats, it will produce 2x as much as the original slime did, however it will not produce blank plorts. This fact makes blank plorts even more rare, which is annoying to some because they need to get lots of <b>Sun Sap</b>...",
                        "m.pure_largos.plortonomics",
                        "PediaPage"
                    ),
                    PediaDetailType.Plort
                )
            )
        );

        CreateIdentPediaEntry(
            customFood, 
            "blank_carrot",
            Get<IdentifiablePediaEntry>("CarrotVeggie"),
            AddTranslation(
                "Some sort of... Carrot?",
                "m.blank_carrot.intro",
                "Pedia"),
            PediaCategoryType.Resources,
            PediaDetail.Params(
                PediaDetail.Create(
                    0, 
                    AddTranslation(
                        "This strange carrot-like food contains chemicals deadly to ranchers and <b>most</b> slimes," +
                        " however a slime that somebody forgot to correctly define it's name is somehow able to eat it." +
                        " That slime isn't known to eat anything else, making this a annoying food to use." +
                        " Some have noted that when enough of this food is around, the world around it becomes a slideshow.", 
                        "m.blank_carrot.desc", 
                        "PediaPage"),
                    PediaDetailType.About
                    ),
                PediaDetail.Create(
                    1, 
                    AddTranslation(
                        "This \"food\" is obviously dangerous, so slimes know not to eat it." +
                        " You can only feed it to \"Slime.\" (What kind of name is that???)" +
                        "\n\nSadly, this resource cannot be grown, only found in the Grey Labyrinth.", 
                        "m.blank_carrot.how_to_use", 
                        "PediaPage"),
                    PediaDetailType.HowToUse
                    )
                )
            );   
        
        
        CreatePediaEntryForSlime(
            slime,
            "blank_slime",
            AddTranslation(
                "It's a.... slime?",
                "m.intro.blank_slime",
                "Pedia"),
            AddTranslation(
                "This slime has not yet been recorded exhibiting any behaviours unique to it's kind aside from an apparent lack of interest in most foods." +
                " It seems to be a strange creation of the Grey Labyrinth's Dreamland that is just a template of a slime," +
                " some theorists believe all other slimes originated from this singular slime.",
                "m.slimeology.blank_slime",
                "PediaPage"),
            AddTranslation(
                "[[ Unknown ]]",
                "m.risks.blank_slime",
                "PediaPage"),
            AddTranslation(
                "On rare occasion a plort resembling this slime has been found inside the Grey Labyrinth's Dreamland and is assumed to be produced by the slime." +
                " These plorts only known use on the ranch is in the formation of pure largos, however back on earth they are used in research." +
                " Slime Scientists get closer and closer every year to discovering how slimes work, and this slime's plort seems to be the final key!",
                "m.plortonomics.blank_slime",
                "PediaPage")
        );

        var slimeFace = slime.AppearancesDefault[0]._face;
        
        var gordo = CreateGordoType("Blank", gordoIcon, AddTranslation("Gordo", "t.blank_gordo", "Pedia"), "IdentifiableType.BlankGordo");
        var gordoFace = new GordoFaceData()
        {
            eyesBlink = slimeFace.GetExpressionFace(SlimeFace.SlimeExpression.BLINK).Eyes,
            eyesNormal = slimeFace.GetExpressionFace(SlimeFace.SlimeExpression.HAPPY).Eyes,
            mouthChompOpen = slimeFace.GetExpressionFace(SlimeFace.SlimeExpression.ELATED).Mouth,
            mouthHappy = slimeFace.GetExpressionFace(SlimeFace.SlimeExpression.HAPPY).Mouth,
            mouthEating = slimeFace.GetExpressionFace(SlimeFace.SlimeExpression.CHOMP_CLOSED).Mouth,
        };
        var gordoObj = CreateGordoObject(Get<IdentifiableType>("PinkGordo").prefab, gordo, slime, gordoFace, slime.AppearancesDefault[0]._structures[0].DefaultMaterials[0]);
        gordo.prefab = gordoObj;

        gordoObj.GetComponent<GordoEat>().TargetCount = 6;
        
        gordoObj.SetRequiredBait(customFood);
        
        CreateGordoSpawnLocation(gordo, SpawnLocations.LabyrinthDreamland, new Vector3(921.2218f, 154.5428f, -897.1734f), Vector3.up * 76.3804f, "blankGordo01");
        
        slime.showForZones = new Il2CppReferenceArray<ZoneDefinition>(Resources.FindObjectsOfTypeAll<ZoneDefinition>().ToArray());
        
        SetResourceGrower(customFood, 0.00075f, 1, "patchCarrot03", SpawnLocations.LabyrinthDreamland, "Onion");
    }
}