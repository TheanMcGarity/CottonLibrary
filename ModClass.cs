using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CottonLibrary
{
    public abstract class CottonMod : MelonMod
    {
        public override void OnEarlyInitializeMelon()
        {
            LibraryUtils.mods.Add(this);
        }
        public Semver.SemVersion version
        {
            get
            {
                return Info.Version;
            }
        }
        public static GameObject player { get { return LibraryUtils.player; } set { LibraryUtils.player = value; } }
        public static SystemContext systemContext { get { return LibraryUtils.systemContext; } }
        public static GameContext gameContext { get { return LibraryUtils.gameContext; } }
        public static SceneContext sceneContext { get { return LibraryUtils.sceneContext; } }
        public static SlimeDefinitions slimeDefinitions { get { return LibraryUtils.slimeDefinitions; } /*set { LibraryUtils.slimeDefinitions = value; }*/ }
        public virtual void OnPlayerSceneLoaded() { }

        public virtual void OnSystemSceneLoaded() { }
        public virtual void OnGameCoreLoaded() { }
        public virtual void OnZoneCoreLoaded() { }
        public virtual void OnSavedGameLoaded() { }
        
        public virtual void PreGameSaving() { }
        public virtual void SaveDirectorLoaded() { }
        public virtual void SaveDirectorLoading(AutoSaveDirector saveDirector) { }


    }
}
