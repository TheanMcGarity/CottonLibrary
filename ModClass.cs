using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CottonLibrary;

/// <summary>
/// <see cref="CottonMod"/> but with a pre-made instance variable.
/// </summary>
/// <typeparam name="M">This is your entry point class. For example, you can do <code>public class ModEntry : CottonModInstance&#60;ModEntry&#62; {}</code></typeparam>
public class CottonModInstance<M> : CottonMod where M : CottonMod
{
    public static CottonModInstance<M> Instance { get; private set; }
    
    /// <summary>
    /// Make sure this code runs or else the class is useless!
    /// </summary>
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
    
    /// <summary>
    /// This is the same thing as <c>SaveDirectorLoaded</c> except its called after <c>SaveDirectorLoaded</c> has already called on each mod.
    /// </summary>
    public virtual void LateSaveDirectorLoaded() { }
    public virtual void SaveDirectorLoading(AutoSaveDirector saveDirector) { }


}