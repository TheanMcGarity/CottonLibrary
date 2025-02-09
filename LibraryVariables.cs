using System;
using Il2CppSystem.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes;
using System.Linq;
using System.Reflection;
using Il2CppMonomiPark.SlimeRancher.Damage;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Persist;
using Il2CppMonomiPark.SlimeRancher.Script.Util;
using Il2CppMonomiPark.SlimeRancher.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Il2CppMonomiPark.SlimeRancher.Weather;
using Il2CppMonomiPark.SlimeRancher.World;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Playables;
using CottonLibrary;
using CottonLibrary.Storage;
using Il2Cpp;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Xml.Linq;
using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Slime;
using MelonLoader;

namespace CottonLibrary;

public static partial class Library
{
    internal static Dictionary<string, IdentifiableType> savedIdents = new Dictionary<string, IdentifiableType>();

    public static WeatherStateDefinition[] weatherStateDefinitions =>
        Resources.FindObjectsOfTypeAll<WeatherStateDefinition>();

    internal static List<CottonMod> mods = new List<CottonMod>();


    internal static Dictionary<IdentifiableType, ModdedMarketData> marketData =
        new Dictionary<IdentifiableType, ModdedMarketData>(0);

    internal static Dictionary<MarketUI.PlortEntry, bool> marketPlortEntries =
        new Dictionary<MarketUI.PlortEntry, bool>();

    internal static List<IdentifiableType> removeMarketPlortEntries = new List<IdentifiableType>();
    internal static GameObject rootOBJ;

    public static IdentifiableTypeGroup? slimes;
    public static IdentifiableTypeGroup? plorts;
    public static IdentifiableTypeGroup? largos;
    public static IdentifiableTypeGroup? baseSlimes;
    public static IdentifiableTypeGroup? food;
    public static IdentifiableTypeGroup? meat;
    public static IdentifiableTypeGroup? veggies;
    public static IdentifiableTypeGroup? fruits;
    public static IdentifiableTypeGroup? crafts;
    public static GameObject? player;

    // public enum VanillaPediaEntryCategories { TUTORIAL, SLIMES, RESOURCES, WORLD, RANCH, SCIENCE, WEATHER }
    public static SystemContext systemContext => SystemContext.Instance;
    public static GameContext gameContext => GameContext.Instance;
    public static SceneContext sceneContext => SceneContext.Instance;

    public static SlimeDefinitions? slimeDefinitions
    {
        get { return gameContext.SlimeDefinitions; } /*set { gameContext.SlimeDefinitions = value; }*/
    }

    private static SlimeAppearanceDirector _mainAppearanceDirector;

    public static SlimeAppearanceDirector mainAppearanceDirector
    {
        get
        {
            if (_mainAppearanceDirector == null)
                _mainAppearanceDirector = Get<SlimeAppearanceDirector>("MainSlimeAppearanceDirector");
            return _mainAppearanceDirector;
        }
        set { _mainAppearanceDirector = value; }
    }


    /// <summary>
    /// The key for this is $"{table}__{key}"
    /// </summary>
    internal static Dictionary<string, LocalizedString> existingTranslations =
        new Dictionary<string, LocalizedString>();

    public static Dictionary<string, Dictionary<string, string>> addedTranslations =
        new Dictionary<string, Dictionary<string, string>>();

    public static GameObject? Get(string name) => Get<GameObject>(name);

    public static GameV06? Save => gameContext.AutoSaveDirector.SavedGame.gameState;

    internal static Sprite LoadSprite(string fileName) => LoadPNG(fileName).ConvertToSprite();
    public static bool inGame => systemContext.SceneLoader.IsCurrentSceneGroupGameplay();
}
