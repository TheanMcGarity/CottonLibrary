using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace CottonLibrary;

public static partial class Library
{
    [Flags]
    public enum SpawnLocations : ushort
    {
        RainbowFields = 1 << 0,
        StarlightStand = 1 << 1,
        EmberValley = 1 << 2,
        PowderfallBluffs = 1 << 3,
        LabyrinthWaterworks = 1 << 4,
        LabyrinthLavadepths = 1 << 5,
        LabyrinthDreamland = 1 << 6,
        LabyrinthHub = 1 << 7,
        CustomArray = 1 << 8,
        All = 255
    }

    [Flags]
    public enum SpawnerTypes : byte
    {
        Slime = 1 << 0,
        Animal = 1 << 1,
        //Crate = 1 << 2,
    }

    public enum SpawningMode
    {
        Default,
        ReplacementBasedSpawning,
    }

    public static void MakeSpawnableInZones(IdentifiableType ident, DirectedActorSpawner.TimeWindow timeWindow,
        SpawnLocations zones, float weight, SpawnerTypes spawnerTargets) =>
        MakeSpawnableInZones(ident, timeWindow, zones, weight, spawnerTargets, SpawningMode.Default, Array.Empty<string>());

    public static void MakeSpawnableInZones(IdentifiableType ident, DirectedActorSpawner.TimeWindow timeWindow,
        SpawnLocations zones, float weight, SpawnerTypes spawnerTargets, SpawningMode mode) =>
        MakeSpawnableInZones(ident, timeWindow, zones, weight, spawnerTargets, mode, Array.Empty<string>());

    public static void MakeSpawnableInZones(IdentifiableType ident, DirectedActorSpawner.TimeWindow timeWindow,
        SpawnLocations zones, float weight, SpawnerTypes spawnerTargets, SpawningMode mode,
        params string[] customZoneSceneNames)
    {
        
        var list = GetSceneNamesFromSpawnerZones(zones);

        foreach (var custom in customZoneSceneNames)
            list.Add(custom);
        
        switch (mode)
        {
            case SpawningMode.Default:

                executeOnSpawnerAwake.Add(new(spawner =>
                {
                    
                    if (ContainsZoneName(spawner.gameObject.scene.name, list))
                    {
                        if (spawnerTargets.HasFlag(SpawnerTypes.Slime))
                            if (spawner.TryCast<DirectedSlimeSpawner>() != null)
                                AddSpawningSettings(spawner, ident, weight, timeWindow);

                        if (spawnerTargets.HasFlag(SpawnerTypes.Animal))
                            if (spawner.TryCast<DirectedAnimalSpawner>() != null)
                                AddSpawningSettings(spawner, ident, weight, timeWindow);
                    }
                }));
                break;
            case SpawningMode.ReplacementBasedSpawning:
                spawnerReplacements.Add(new ReplacementSpawnerData()
                {
                    chance = weight,
                    ident = ident,
                    zones = list.ToArray(),
                });
                break;
            default:
                throw new InvalidOperationException("Unknown spawning mode used!");
        }

    }

    public static List<string> GetSceneNamesFromSpawnerZones(SpawnLocations zones)
    {
        List<string> names = new();

        if (zones.HasFlag(SpawnLocations.RainbowFields)) names.Add("zoneFields");
        if (zones.HasFlag(SpawnLocations.EmberValley)) names.Add("zoneGorge");
        if (zones.HasFlag(SpawnLocations.StarlightStand)) names.Add("zoneStrand");
        if (zones.HasFlag(SpawnLocations.LabyrinthWaterworks)) names.Add("LabStrand");
        if (zones.HasFlag(SpawnLocations.LabyrinthLavadepths)) names.Add("LabValley");
        if (zones.HasFlag(SpawnLocations.LabyrinthDreamland)) names.Add("Dreamland");
        if (zones.HasFlag(SpawnLocations.LabyrinthHub)) names.Add("Hub");

        return names;
    }

    public static List<string> GetDefNamesFromSpawnerZones(SpawnLocations zones)
    {
        List<string> names = new();

        if (zones.HasFlag(SpawnLocations.RainbowFields)) names.Add("fields");
        if (zones.HasFlag(SpawnLocations.EmberValley)) names.Add("gorge");
        if (zones.HasFlag(SpawnLocations.StarlightStand)) names.Add("strand");
        if (zones.HasFlag(SpawnLocations.LabyrinthWaterworks)) names.Add("waterworks");
        if (zones.HasFlag(SpawnLocations.LabyrinthLavadepths)) names.Add("lavadepths");
        if (zones.HasFlag(SpawnLocations.LabyrinthDreamland)) names.Add("dream");
        if (zones.HasFlag(SpawnLocations.LabyrinthHub)) names.Add("hub");

        return names;
    }

    internal static bool ContainsZoneName(string sceneName, List<string> zoneNames)
    {
        foreach (var zoneName in zoneNames)
            if (sceneName.Contains(zoneName))
                return true;

        return false;
    }

    internal static bool IsInZone(string[] zoneNames)
    {
        if (sceneContext == null) return false;
        if (sceneContext.Player == null) return false;
        var tracker = sceneContext.Player.GetComponent<PlayerZoneTracker>();
        if (tracker == null) return false;
        var zone = tracker.GetCurrentZone();
        if (zone == null) return false;
        foreach (var name in zoneNames)
        {
            if (zone.name.ToLower().Contains(name.ToLower())) return true;
        }

        return false;
    }

    public static void AddSpawningSettings(DirectedActorSpawner spawner, IdentifiableType ident, float weight,
        DirectedActorSpawner.TimeWindow timeWindow)
    {
        DirectedActorSpawner.SpawnConstraint constraint = null;
        float consWeight = 0; // only used if a new constraint needs to be made.

        foreach (var cons in spawner.Constraints)
        {
            consWeight = ((consWeight / 2) + (cons.Weight / 2)) * 2;
            if (cons.Window.TimeMode == timeWindow.TimeMode)
            {
                constraint = cons;
                break;
            }
        }

        if (constraint == null)
        {
            constraint = new DirectedActorSpawner.SpawnConstraint();
            constraint.Window = timeWindow;
            constraint.Feral = false;
            constraint.Weight = consWeight;
            constraint.Slimeset = new SlimeSet();
            constraint.Slimeset.Members = new Il2CppReferenceArray<SlimeSet.Member>(0);
            spawner.Constraints = spawner.Constraints.Add(constraint);
        }

        var member = new SlimeSet.Member()
        {
            _prefab = ident.prefab,
            IdentType = ident,
            Weight = weight
        };
        constraint.Slimeset.Members = constraint.Slimeset.Members.Add(member);
    }

    internal struct ReplacementSpawnerData
    {
        public IdentifiableType ident;
        public float chance;
        public string[] zones;
    }

    public static void SetResourceGrower(
        IdentifiableType ident,
        float chance,
        int minAmount,
        string spawnerObjectName,
        SpawnLocations zones,
        params string[] blacklistInSpawnerName)
    {
        onResourceGrowerAwake.Add(spawner =>
        {
            var id = spawnerObjectName;
            var i = 0;
            foreach (var blacklist in blacklistInSpawnerName)
            {
                id += $"-blacklist{i}_{blacklist}";
                i++;
            }

            id += $"-zones_{(int)zones}";
            var scenes = GetSceneNamesFromSpawnerZones(zones);
            
            bool inZone = scenes.Any(scene => spawner.gameObject.scene.name.Contains(scene));
            
            if (!inZone) return;
            
            if (!spawner.gameObject.name.Contains(spawnerObjectName)) return;
            
            bool containsBlacklistedWord = blacklistInSpawnerName.Any(name => spawner.gameObject.name.Contains(name));
            
            if (containsBlacklistedWord) return;
            
            resourceGrowerDefinitions.TryGetValue(id, out var spawnerDefinition);
            
            if (spawnerDefinition == null)
            {
                spawnerDefinition = Object.Instantiate(spawner.ResourceGrowerDefinition);
                spawnerDefinition.name = id;
                Object.DontDestroyOnLoad(spawnerDefinition);
            }

            try
            {
                spawnerDefinition._resources.First(x => x.ResourceIdentifiableType.name == ident.name);
            }
            catch
            {
                spawnerDefinition._resources = spawnerDefinition._resources.Add(
                    new ResourceSpawnerDefinition.WeightedResourceEntry()
                    {
                        MinimumAmount = minAmount,
                        ResourceIdentifiableType = ident,
                        Weight = chance
                    }); 
            }

            spawner._resourceGrowerDefinition = spawnerDefinition;
            
            resourceGrowerDefinitions.TryAdd(id, spawnerDefinition);
        });
    }

    public static Dictionary<string, ResourceGrowerDefinition> resourceGrowerDefinitions = new();
    
    internal static List<Action<SpawnResource>> onResourceGrowerAwake = new ();
}