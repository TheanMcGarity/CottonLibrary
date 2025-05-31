using System;
using System.Collections;
using static CottonLibrary.Library;
using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI.Localization;
using Il2CppSystem.Globalization;
using MelonLoader;
using UnityEngine;

namespace CottonLibrary.Patches;


internal class LocalizationRepair
{
    public static void StartLoop() => MelonCoroutines.Start(RepairLoop());
    
    private static float timer = -1;
    private const float INTERVAL = 0.75f;
    
    static IEnumerator RepairLoop()
    {
        while (true)
        {
            if (timer > INTERVAL)
            {
                yield return FixLanguage(systemContext.LocalizationDirector,
                    systemContext.LocalizationDirector.Locales._items[
                        systemContext.LocalizationDirector.GetCurrentLocaleIndex()]);
            }

            timer += Time.unscaledTime;
            yield return null;
        }
    }
    
    static IEnumerator FixLanguage(LocalizationDirector director, UnityEngine.Localization.Locale curLocale)
    {
        var code = curLocale.Formatter.Cast<CultureInfo>()._name;

        LoadLanguage(code);

        loadedLocalizedStrings.Clear();
        addedTranslations.Clear();

        int counter = 0;
        foreach (var str in moddedLocalizedStrings)
        {
            var localized = CreateStaticString(LoadLocalizedText(str.Key, str.Value.parameters), str.Value.srKey, str.Value.table);

            var original = str.Value.str;

            original.m_TableEntryReference = localized.TableEntryReference;
            original.m_TableReference = localized.TableReference;
            
            counter++;
            if (counter > 20)
            {
                counter = 0;
                yield return null;
            }
        }
    }
}