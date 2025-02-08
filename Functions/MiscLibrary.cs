using System.Reflection;
using Il2CppMonomiPark.SlimeRancher.Script.Util;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace CottonLibrary;

public static partial class Library
{
    public static T? Get<T>(string name) where T : Object =>
        Resources.FindObjectsOfTypeAll<T>().FirstOrDefault((T x) => x.name == name);
    public static GameObject CreatePrefab(string Name, GameObject baseObject)
    {
        var obj = baseObject.CopyObject();
        UnityEngine.Object.DontDestroyOnLoad(obj);
        obj.name = Name;
        obj.transform.parent = rootOBJ.transform;
        var components = obj.GetComponents<Behaviour>();
        foreach (var component in components)
        {
            component.enabled = true;
        }

        return obj;
    } public static LocalizedString AddTranslation(string localized, string key = "l.Empty",
        string table = "Actor")
    {
        StringTable table2 = LocalizationUtil.GetTable(table);

        Dictionary<string, string> dictionary;
        if (!addedTranslations.TryGetValue(table, out dictionary))
        {
            dictionary = new Dictionary<string, string>();

            addedTranslations.Add(table, dictionary);
        }

        if (dictionary.ContainsKey(key))
        {
            return existingTranslations[$"{table}__{key}"];
        }

        dictionary.Add(key, localized);

        StringTableEntry stringTableEntry = table2.AddEntry(key, localized);
        LocalizedString result =
            new LocalizedString(table2.SharedData.TableCollectionName, stringTableEntry.SharedEntry.Id);

        existingTranslations.Add($"{table}__{key}", result);

        return result;
    }public static Texture2D LoadPNG(string embeddedFile)
    {
        Assembly executingAssembly = Assembly.GetCallingAssembly();
        System.IO.Stream manifestResourceStream =
            executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + "." + embeddedFile +
                                                        ".png");
        byte[] array = new byte[manifestResourceStream.Length];
        manifestResourceStream.Read(array, 0, array.Length);
        Texture2D texture2D = new Texture2D(1, 1);
        ImageConversion.LoadImage(texture2D, array);
        texture2D.filterMode = FilterMode.Bilinear;
        return texture2D;
    }
}