using MelonLoader;
using CottonLibrary;
using CottonModTest;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using static CottonLibrary.Library;
using Object = UnityEngine.Object;


[assembly: MelonInfo(typeof(ModEntry), "Cotton Library Test Mod", "1.0.0", "PinkTarr")]

namespace CottonModTest;

public class ModEntry : CottonModInstance<ModEntry>
{
    // Mod variables
    public readonly Color32 white = new Color32(255, 255, 255, 255);
    
    // Blue variables
    public SlimeDefinition blueSlimeDef;
    public GameObject blueSlimeObj;
    
    public IdentifiableType bluePlortDef;
    public GameObject bluePlortObj;

    public readonly Color32 blueSlimeTopColor = new Color32(74, 127, 212, 255);
    public readonly Color32 blueSlimeMiddleColor = new Color32(58, 109, 189, 255);
    public readonly Color32 blueSlimeBottomColor = new Color32(38, 85, 158, 255);
    
    public readonly Color32 blueSlimeSplatColor = new Color32(52, 157, 201, 255);

    public readonly Color32 blueSlimeTopTwinColor = new Color32(74, 207, 212, 255);
    public readonly Color32 blueSlimeMiddleTwinColor = new Color32(47, 173, 159, 255);
    public readonly Color32 blueSlimeBottomTwinColor = new Color32(38, 158, 132, 255);
    

    public Sprite blueSlimeIcon;
    public Sprite bluePlortIcon;
    
    
    // Green variables
    public SlimeDefinition greenSlimeDef;
    public GameObject greenSlimeObj;
    
    public IdentifiableType greenPlortDef;
    public GameObject greenPlortObj;

    public readonly Color32 greenSlimeTopColor = new Color32(15, 219, 77, 255);
    public readonly Color32 greenSlimeMiddleColor = new Color32(65, 171, 0, 255);
    public readonly Color32 greenSlimeBottomColor = new Color32(100, 212, 0, 255);
    
    public readonly Color32 greenSlimeSplatColor = new Color32(78, 202, 25, 255);
    
    public override void SaveDirectorLoaded()
    {
        CreateBlueSlime();
        CreateGreenSlime();
    }

    public LargoSettings blueLargoSettings = LargoSettings.KeepSecondBody | LargoSettings.KeepFirstColor | LargoSettings.KeepSecondFace | LargoSettings.KeepFirstTwinColor;

    Dictionary<SlimeDefinition, SlimeDefinition> blueLargos = new Dictionary<SlimeDefinition, SlimeDefinition>();
    
    /// <summary>
    /// How you were supposed to make largos with every slime. Now it is built into <c>CreateSlimeDef</c>.
    /// </summary>
    void CreateBlueLargos()
    {
        blueSlimeDef.CanLargofy = true;
        foreach (var slime in baseSlimes._memberGroups._items[0]._memberTypes)
        {
            if (slime.Cast<SlimeDefinition>().CanLargofy && !blueLargos.ContainsKey(slime.Cast<SlimeDefinition>()))
            {
                var largo = CreateCompleteLargo(blueSlimeDef, slime.Cast<SlimeDefinition>(), blueLargoSettings);
                blueLargos.Add(slime.Cast<SlimeDefinition>(), largo);
            }
        }
    }

    void CreateBlueSlime()
    {
        if (blueSlimeDef) return;

        bluePlortIcon = LoadPNG("iconPlortBlueGreen").ConvertToSprite();

        
        bluePlortDef = CreatePlortType("Blue", blueSlimeMiddleColor, bluePlortIcon, "IdentifiableType.BluePlort", 21, 15);
        bluePlortObj = CreatePrefab("plortBlue", GetPlort("TwinPlort").prefab);
        
        bluePlortDef.localizedName = AddTranslation("Blue Plort", "l.cottontestplort");
        
        bluePlortDef.SetObjectPrefab(bluePlortObj);
        bluePlortObj.SetObjectIdent(bluePlortDef);
        
        SetPlortColor(blueSlimeTopColor, blueSlimeMiddleColor, blueSlimeBottomColor, bluePlortObj);
        SetPlortTwinColor(blueSlimeTopTwinColor, blueSlimeMiddleTwinColor, blueSlimeBottomTwinColor, bluePlortObj);
        
        bluePlortDef.MakeVaccable();
        
        blueSlimeIcon = LoadPNG("iconSlimeBlueGreen").ConvertToSprite();
        
        var pink = GetSlime("Pink");
        
        blueSlimeDef = CreateSlimeDef(
            "Blue",
            blueSlimeMiddleColor, 
            blueSlimeIcon,
            Object.Instantiate(pink.AppearancesDefault[0]),
            "BlueDefault",
            "SlimeDefinition.Blue",
            true,
            blueLargoSettings
        );

        
        blueSlimeDef.localizedName = AddTranslation("Blue Slime", "l.cottontestslime");
        
        blueSlimeObj = CreatePrefab("slimeBlue", pink.prefab);
        
        blueSlimeDef.SetObjectPrefab(blueSlimeObj);
        blueSlimeObj.SetObjectIdent(blueSlimeDef);
        
        blueSlimeDef.SetSlimeColor(
            blueSlimeTopColor,
            blueSlimeMiddleColor,
            blueSlimeBottomColor
        );
        blueSlimeDef.SetTwinColor(
            blueSlimeTopTwinColor,
            blueSlimeMiddleTwinColor,
            blueSlimeBottomTwinColor
        );
        blueSlimeDef.EnableTwinEffect();

        blueSlimeDef.AppearancesDefault[0].SetPalette(blueSlimeDef.AppearancesDefault[0].Structures[0].DefaultMaterials[0], blueSlimeDef);

        blueSlimeDef.AppearancesDefault[0]._splatColor = blueSlimeSplatColor;
        blueSlimeDef.AppearancesDefault[0].SetSplatColor();
        
        IdentifiableTypeGroup[] blueFoods = new[]
        {
            fruits
        };

        blueSlimeDef.AddProduceIdent(bluePlortDef);
        
        blueSlimeDef.Diet.MajorFoodIdentifiableTypeGroups = new Il2CppReferenceArray<IdentifiableTypeGroup>(blueFoods);
        blueSlimeDef.RefreshEatmap();
        
        blueSlimeDef.MakeVaccable();
    }
    
    public LargoSettings greenLargoSettings = LargoSettings.KeepSecondBody | LargoSettings.KeepFirstColor | LargoSettings.KeepSecondFace | LargoSettings.MergeTwinColors;

    void CreateGreenSlime()
    {
        if (greenSlimeDef) return;

        
        greenPlortDef = CreatePlortType("Green", greenSlimeMiddleColor, null, "IdentifiableType.GreenPlort", 21, 15);
        greenPlortObj = CreatePrefab("plortGreen", GetPlort("SloomberPlort").prefab);
        
        greenPlortDef.localizedName = AddTranslation("Green Plort", "l.cottontestplort2");
        
        greenPlortDef.SetObjectPrefab(greenPlortObj);
        greenPlortObj.SetObjectIdent(greenPlortDef);
        
        SetPlortColor(greenSlimeTopColor, greenSlimeMiddleColor, greenSlimeBottomColor, greenPlortObj);
        
        greenPlortDef.MakeVaccable();
        
        
        var pink = GetSlime("Pink");
        
        greenSlimeDef = CreateSlimeDef(
            "Green",
            greenSlimeMiddleColor, 
            null,
            Object.Instantiate(pink.AppearancesDefault[0]),
            "GreenDefault",
            "SlimeDefinition.Green",
            true,
            greenLargoSettings
        );

        greenSlimeDef.AppearancesDefault[0]._structures[0].DefaultMaterials[0] = Object.Instantiate(GetSlime("Sloomber").AppearancesDefault[0].Structures[0].DefaultMaterials[0]);
        
        greenSlimeDef.localizedName = AddTranslation("Green Slime", "l.cottontestslime2");
        
        greenSlimeObj = CreatePrefab("slimeGreen", pink.prefab);
        
        greenSlimeDef.SetObjectPrefab(greenSlimeObj);
        greenSlimeObj.SetObjectIdent(greenSlimeDef);
        
        greenSlimeDef.SetSlimeColor(
            greenSlimeTopColor,
            greenSlimeMiddleColor,
            greenSlimeBottomColor
        );
        greenSlimeDef.SetSloomberColor(
            greenSlimeTopColor,
            greenSlimeMiddleColor,
            greenSlimeBottomColor
        );

        greenSlimeDef.AppearancesDefault[0].SetPalette(greenSlimeDef.AppearancesDefault[0].Structures[0].DefaultMaterials[0], greenSlimeDef);

        greenSlimeDef.AppearancesDefault[0]._splatColor = greenSlimeSplatColor;
        greenSlimeDef.AppearancesDefault[0].SetSplatColor();
        
        IdentifiableTypeGroup[] greenFoods = new[]
        {
            fruits
        };

        greenSlimeDef.AddProduceIdent(greenPlortDef);
        
        greenSlimeDef.Diet.MajorFoodIdentifiableTypeGroups = new Il2CppReferenceArray<IdentifiableTypeGroup>(greenFoods);
        greenSlimeDef.RefreshEatmap();
        
        greenSlimeDef.MakeVaccable();
    }
}