using Il2CppMonomiPark.SlimeRancher.World;
using Il2Cpp;
namespace CottonLibrary
{
    public static class Callbacks
    {
        public delegate void OnPlortSold(int amount, IdentifiableType id);
        public delegate void OnZoneEnter(ZoneDefinition zone);
        public delegate void OnZoneExit(ZoneDefinition zone);
        public delegate void OnFieldsFinishLoad();
        public delegate void OnEmberFinishLoad();
        public delegate void OnStrandFinishLoad();
        public delegate void OnBluffsFinishLoad();


        public static event OnPlortSold onPlortSold;
        public static event OnZoneEnter onZoneEnter;
        public static event OnZoneExit onZoneExit;
        //public static event OnFieldsFinishLoad onFieldsLoad;
        //public static event OnEmberFinishLoad onEmberLoad;
        //public static event OnStrandFinishLoad onStrandLoad;
        //public static event OnBluffsFinishLoad onBluffsLoad;

        internal static void Invoke_onPlortSold(int amount, IdentifiableType id) => onPlortSold?.Invoke(amount, id);
        internal static void Invoke_onZoneEnter(ZoneDefinition zone) => onZoneEnter?.Invoke(zone);
        internal static void Invoke_onZoneExit(ZoneDefinition zone) => onZoneExit?.Invoke(zone);
        //internal static void Invoke_onFieldsLoad() => onFieldsLoad?.Invoke();
        //internal static void Invoke_onEmberLoad() => onEmberLoad?.Invoke();
        //internal static void Invoke_onStrandLoad() => onStrandLoad?.Invoke();
        //internal static void Invoke_onBluffsLoad() => onBluffsLoad?.Invoke();

    }
}
