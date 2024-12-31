using MelonLoader;
using CottonLibrary;
using CottonModTest;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using static CottonLibrary.LibraryUtils;
using Object = UnityEngine.Object;


[assembly: MelonInfo(typeof(ModEntry), "Cotton Library Test Mod", "1.0.0", "PinkTarr")]

namespace CottonModTest;


public class ModEntry : CottonMod
{
    public ModEntry() => Instance = this;
    
    public static ModEntry Instance { get; private set; }
    
    public SlimeDefinition blueSlimeDef;
    public GameObject blueSlimeObj;

    public readonly Color32 blueSlimeTopColor = new Color32(74, 127, 212, 255);
    public readonly Color32 blueSlimeMiddleColor = new Color32(58, 109, 189, 255);
    public readonly Color32 blueSlimeBottomColor = new Color32(38, 85, 158, 255);

    public readonly Color32 blueSlimeTopTwinColor = new Color32(74, 207, 212, 255);
    public readonly Color32 blueSlimeMiddleTwinColor = new Color32(47, 173, 159, 255);
    public readonly Color32 blueSlimeBottomTwinColor = new Color32(38, 158, 132, 255);
    
    public readonly Color32 white = new Color32(255, 255, 255, 255);

    public Sprite blueSlimeIcon;
    
    public override void SaveDirectorLoaded()
    {
        if (blueSlimeDef) return;

        blueSlimeIcon = LoadPNG("iconSlimeBlueGreen").ConvertToSprite();
        
        var pink = GetSlime("Pink");
        
        blueSlimeDef = CreateSlimeDef(
            "Blue",
            blueSlimeMiddleColor, 
            blueSlimeIcon,
            Object.Instantiate(pink.AppearancesDefault[0]),
            "BlueDefault",
            "SlimeDefinition.Blue"
            );

        
        blueSlimeDef.localizedName = AddTranslation("Blue Slime", "l.cottontestslime");
        
        blueSlimeObj = CreatePrefab("slimeBlue", pink.prefab);
        
        blueSlimeDef.SetObjectPrefab(blueSlimeObj);
        blueSlimeObj.SetObjectIdent(blueSlimeDef);
        
        blueSlimeDef.SetSlimeColor(
            blueSlimeTopColor,
            blueSlimeMiddleColor,
            blueSlimeBottomColor,
            white,
            0,
            0,
            false,
            0
        );
        blueSlimeDef.SetTwinColor(
            blueSlimeTopTwinColor,
            blueSlimeMiddleTwinColor,
            blueSlimeBottomTwinColor,
            0,
            0,
            false,
            0
        );
        blueSlimeDef.EnableTwinEffect(0,0,false,0);

        IdentifiableTypeGroup[] blueFoods = new[]
        {
            fruits
        };
        
        blueSlimeDef.Diet.MajorFoodIdentifiableTypeGroups = new Il2CppReferenceArray<IdentifiableTypeGroup>(blueFoods);
        blueSlimeDef.RefreshEatmap();
        
        blueSlimeDef.MakeVaccable();
    }
}