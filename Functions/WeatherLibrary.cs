using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Weather;

namespace CottonLibrary;

public static partial class Library
{
    public static WeatherStateDefinition? GetWeatherStateByName(string name)
    {
        return weatherStateDefinitions.FirstOrDefault(x =>
            name.ToUpper().Replace(" ", "") == x.name.Replace(" ", "").ToUpper());
    }

    public static Il2CppArrayBase<WeatherStateDefinition> weatherStates =>
        GameContext.Instance.AutoSaveDirector.weatherStates.items.ToArray();

}