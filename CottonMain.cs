using CottonLibrary;
using MelonLoader;
using UnityEngine;
using static CottonLibrary.LibraryUtils;

[assembly: MelonInfo(typeof(CottonMain), "Cotton Library", "1.2.0", "PinkTarr")]
[assembly: MelonGame("MonomiPark", "SlimeRancher2")]

namespace CottonLibrary
{

    public class CottonMain : MelonMod
    {
        public override void OnLateInitializeMelon()
        {

            foreach (MelonBase melonBase in MelonBase.RegisteredMelons)
            {
                if (melonBase is CottonMod)
                {
                    CottonMod mod = melonBase as CottonMod;
                    mods.Add(mod);
                    MelonLogger.Msg("Cotton registered mod: " + mod.MelonAssembly.Assembly.FullName);
                }
            }
            if (Get("SR2ELibraryROOT")) { rootOBJ = Get("CottonLibraryROOT"); }
            else
            {
                rootOBJ = new GameObject();
                rootOBJ.SetActive(false);
                rootOBJ.name = "CottonLibraryROOT";
                UnityEngine.Object.DontDestroyOnLoad(rootOBJ);
            }
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            switch (sceneName)
            {
                case "GameCore":
                    foreach (CottonMod mod in mods)
                        mod.OnGameCoreLoaded();
                    break;
                case "ZoneCore":
                    foreach (CottonMod mod in mods)
                        mod.OnZoneCoreLoaded();
                    break;
                case "SystemCore":
                    foreach (CottonMod mod in mods)
                        mod.OnSystemSceneLoaded();
                    break;
                case "PlayerCore":
                    foreach (CottonMod mod in mods)
                        mod.OnPlayerSceneLoaded();
                    break;
            }
        }
    }
}
