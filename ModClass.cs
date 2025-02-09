using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CottonLibrary;

public class CottonModInstance<M> : CottonMod where M : CottonMod
{
    public static CottonModInstance<M> Instance { get; private set; }
    public CottonModInstance() => Instance = this;
} 

public abstract class CottonMod : MelonMod
{
    public override void OnEarlyInitializeMelon()
    {
        Library.mods.Add(this);
    }
    public Semver.SemVersion version
    {
        get
        {
            return Info.Version;
        }
    }
    public static GameObject player { get { return Library.player; } set { Library.player = value; } }
    public static SystemContext systemContext { get { return Library.systemContext; } }
    public static GameContext gameContext { get { return Library.gameContext; } }
    public static SceneContext sceneContext { get { return Library.sceneContext; } }
    public static SlimeDefinitions slimeDefinitions { get { return Library.slimeDefinitions; } /*set { LibraryUtils.slimeDefinitions = value; }*/ }
    public virtual void OnPlayerSceneLoaded() { }

    public virtual void OnSystemSceneLoaded() { }
    public virtual void OnGameCoreLoaded() { }
    public virtual void OnZoneCoreLoaded() { }
    public virtual void OnSavedGameLoaded() { }
        
    public virtual void PreGameSaving() { }
    public virtual void SaveDirectorLoaded() { }
    public virtual void SaveDirectorLoading(AutoSaveDirector saveDirector) { }


}