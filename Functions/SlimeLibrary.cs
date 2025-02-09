using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Damage;
using Il2CppMonomiPark.SlimeRancher.Slime;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CottonLibrary;

public static partial class Library
{

    public static SlimeDefinition CreateSlimeDef(
        string name,
        Color32 vacColor,
        Sprite icon,
        SlimeAppearance baseAppearance,
        string appearanceName,
        string refID) =>
        CreateSlimeDef(
            name,
            vacColor,
            icon,
            baseAppearance,
            appearanceName,
            refID,
            false,
            (LargoSettings)0
        );
    
    public static SlimeDefinition CreateSlimeDef(
        string name,
        Color32 vacColor,
        Sprite icon,
        SlimeAppearance baseAppearance,
        string appearanceName,
        string refID,
        bool largoable,
        LargoSettings largoSettings)
    {
        SlimeDefinition slimedef = Object.Instantiate(GetSlime("Pink"));
        Object.DontDestroyOnLoad(slimedef);
        slimedef.hideFlags = HideFlags.HideAndDontSave;
        slimedef.name = name;
        slimedef.AppearancesDefault = new Il2CppReferenceArray<SlimeAppearance>(1);

        SlimeAppearance appearance = Object.Instantiate(baseAppearance);
        Object.DontDestroyOnLoad(appearance);
        appearance.name = appearanceName;
        appearance._icon = icon;
        slimedef.AppearancesDefault = slimedef.AppearancesDefault.Add(appearance);
        if (slimedef.AppearancesDefault[0] == null)
        {
            slimedef.AppearancesDefault[0] = appearance;
        }

        for (int i = 0; i < slimedef.AppearancesDefault[0].Structures.Count - 1; i++)
        {
            SlimeAppearanceStructure a = slimedef.AppearancesDefault[0].Structures[i];
            var a2 = new SlimeAppearanceStructure(a);
            slimedef.AppearancesDefault[0].Structures[i] = a2;
            if (a.DefaultMaterials.Count != 0)
            {
                a2.DefaultMaterials[0] = Object.Instantiate(a.DefaultMaterials[0]);
            }
        }

        SlimeDiet diet = INTERNAL_CreateNewDiet();
        slimedef.Diet = diet;
        slimedef.color = vacColor;
        slimedef.icon = icon;
        slimeDefinitions.Slimes.Add(slimedef);
        if (!slimedef.IsLargo)
        {
            gameContext.SlimeDefinitions.Slimes = gameContext.SlimeDefinitions.Slimes.AddItem(slimedef).ToArray();
            gameContext.SlimeDefinitions._slimeDefinitionsByIdentifiable.TryAdd(slimedef, slimedef);
        }

        INTERNAL_SetupLoadForIdent(refID, slimedef);

        if (largoable)
        {
            slimedef.CanLargofy = true;
            
            foreach (var slime in baseSlimes._memberGroups._items[0]._memberTypes)
                if (slime.Cast<SlimeDefinition>().CanLargofy)
                    CreateCompleteLargo(slimedef, slime.Cast<SlimeDefinition>(), largoSettings);
        }
        
        return slimedef;
    }

    public static void SetLargoPallete(this SlimeAppearance app, Material slimeMaterial, SlimeDefinition definition)
    {
        app._colorPalette = new SlimeAppearance.Palette()
        {
            Ammo = definition.color,
            Bottom = slimeMaterial.GetColor("_BottomColor"),
            Middle = slimeMaterial.GetColor("_MiddleColor"),
            Top = slimeMaterial.GetColor("_TopColor"),
        };
    }

    internal static SlimeAppearance.Palette INTERNAL_GetTwinPalette(this SlimeAppearance app)
    {
        Material mat = null;
        foreach (var structure in app._structures)
        {
            if (structure.Element.Type == SlimeAppearanceElement.ElementType.BODY)
            {
                mat = structure.DefaultMaterials[0];
                break;
            }
        }

        return new SlimeAppearance.Palette()
        {
            Ammo = new Color32(255, 255, 255, 255),
            Top = mat.GetColor("_TwinTopColor"),
            Middle = mat.GetColor("_TwinMiddleColor"),
            Bottom = mat.GetColor("_TwinBottomColor"),
        };
    }

    internal static SlimeAppearance.Palette INTERNAL_GetSloomberPalette(this SlimeAppearance app)
    {
        Material mat = null;
        foreach (var structure in app._structures)
        {
            if (structure.Element.Type == SlimeAppearanceElement.ElementType.BODY)
            {
                mat = structure.DefaultMaterials[0];
                break;
            }
        }

        return new SlimeAppearance.Palette()
        {
            Ammo = new Color32(255, 255, 255, 255),
            Top = mat.GetColor("_SloomberTopColor"),
            Middle = mat.GetColor("_SloomberMiddleColor"),
            Bottom = mat.GetColor("_SloomberBottomColor"),
        };
    }

    internal static bool INTERNAL_GetLargoHasTwinEffect(SlimeAppearance slime1, SlimeAppearance slime2)
    {
        bool result = false;

        foreach (var structure in slime1._structures)
        {
            if (structure.DefaultMaterials[0].IsKeywordEnabled("_ENABLETWINEFFECT_ON"))
            {
                result = true;
                break;
            }
        }

        foreach (var structure in slime2._structures)
        {
            if (result) break;

            if (structure.DefaultMaterials[0].IsKeywordEnabled("_ENABLETWINEFFECT_ON"))
            {
                result = true;
                break;
            }
        }

        return result;
    }

    internal static bool INTERNAL_GetLargoHasSloomberEffect(SlimeAppearance slime1, SlimeAppearance slime2)
    {
        bool result = false;

        foreach (var structure in slime1._structures)
        {
            if (structure.DefaultMaterials.Count != 0 &&
                structure.DefaultMaterials[0].IsKeywordEnabled("_BODYCOLORING_SLOOMBER"))
            {
                result = true;
                break;
            }
        }

        foreach (var structure in slime2._structures)
        {
            if (result) break;

            if (structure.DefaultMaterials.Count != 0 &&
                structure.DefaultMaterials[0].IsKeywordEnabled("_BODYCOLORING_SLOOMBER"))
            {
                result = true;
                break;
            }
        }

        return result;
    }

    public static Il2CppReferenceArray<SlimeAppearanceStructure> MergeStructures(SlimeAppearance slime1,
        SlimeAppearance slime2, LargoSettings settings)
    {
        var newStructures = new List<SlimeAppearanceStructure>(0);
        SlimeAppearance.Palette firstColor = slime1._colorPalette;
        SlimeAppearance.Palette firstColorTwin = slime1.INTERNAL_GetTwinPalette();
        SlimeAppearance.Palette firstColorSloomber = slime1.INTERNAL_GetSloomberPalette();

        SlimeAppearance.Palette secondColor = slime2._colorPalette;
        SlimeAppearance.Palette secondColorTwin = slime2.INTERNAL_GetTwinPalette();
        SlimeAppearance.Palette secondColorSloomber = slime2.INTERNAL_GetSloomberPalette();


        bool useTwinShader = INTERNAL_GetLargoHasTwinEffect(slime1, slime2);

        bool useSloomberShader = INTERNAL_GetLargoHasSloomberEffect(slime1, slime2);
        Material sloomberMat = GetSlime("Sloomber").AppearancesDefault[0]._structures[0].DefaultMaterials[0];

        foreach (var structure in slime1.Structures)
        {
            if (structure.Element.Type == SlimeAppearanceElement.ElementType.FACE ||
                structure.Element.Type == SlimeAppearanceElement.ElementType.FACE_ATTACH)
            {
                if (settings.HasFlag(LargoSettings.KeepFirstFace))
                {
                    if (structure != null && !newStructures.Contains(structure) &&
                        structure.DefaultMaterials.Length != 0)
                    {
                        var newStructure = new SlimeAppearanceStructure(structure);
                        newStructures.Add(newStructure);
                        var mat = Object.Instantiate(structure.DefaultMaterials[0]);
                        newStructure.DefaultMaterials[0] = mat;


                        try
                        {
                            if (settings.HasFlag(LargoSettings.KeepFirstColor))
                            {
                                mat.SetColor("_TopColor", firstColor.Top);
                                mat.SetColor("_MiddleColor", firstColor.Middle);
                                mat.SetColor("_BottomColor", firstColor.Bottom);
                                mat.SetColor("_SpecColor", firstColor.Middle);
                            }
                            else if (settings.HasFlag(LargoSettings.KeepSecondColor))
                            {
                                mat.SetColor("_TopColor", secondColor.Top);
                                mat.SetColor("_MiddleColor", secondColor.Middle);
                                mat.SetColor("_BottomColor", secondColor.Bottom);
                                mat.SetColor("_SpecColor", secondColor.Middle);
                            }
                            else if (settings.HasFlag(LargoSettings.MergeColors))
                            {
                                var top = Color.Lerp(firstColor.Top, secondColor.Top, 0.5f);
                                var middle = Color.Lerp(firstColor.Middle, secondColor.Middle, 0.5f);
                                var bottom = Color.Lerp(firstColor.Bottom, secondColor.Bottom, 0.5f);
                                mat.SetColor("_TopColor", top);
                                mat.SetColor("_MiddleColor", middle);
                                mat.SetColor("_BottomColor", bottom);
                                mat.SetColor("_SpecColor", middle);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            else if (structure.Element.Type == SlimeAppearanceElement.ElementType.BODY)
            {
                if (settings.HasFlag(LargoSettings.KeepFirstBody))
                {
                    if (structure != null && !newStructures.Contains(structure) &&
                        structure.DefaultMaterials.Length != 0)
                    {
                        var newStructure = new SlimeAppearanceStructure(structure);
                        newStructures.Add(newStructure);
                        var mat = Object.Instantiate(structure.DefaultMaterials[0]);
                        newStructure.DefaultMaterials[0] = mat;

                        try
                        {
                            if (settings.HasFlag(LargoSettings.KeepFirstColor))
                            {
                                mat.SetColor("_TopColor", firstColor.Top);
                                mat.SetColor("_MiddleColor", firstColor.Middle);
                                mat.SetColor("_BottomColor", firstColor.Bottom);
                                mat.SetColor("_SpecColor", firstColor.Middle);
                            }
                            else if (settings.HasFlag(LargoSettings.KeepSecondColor))
                            {
                                mat.SetColor("_TopColor", secondColor.Top);
                                mat.SetColor("_MiddleColor", secondColor.Middle);
                                mat.SetColor("_BottomColor", secondColor.Bottom);
                                mat.SetColor("_SpecColor", secondColor.Middle);
                            }
                            else if (settings.HasFlag(LargoSettings.MergeColors))
                            {
                                var top = Color.Lerp(firstColor.Top, secondColor.Top, 0.5f);
                                var middle = Color.Lerp(firstColor.Middle, secondColor.Middle, 0.5f);
                                var bottom = Color.Lerp(firstColor.Bottom, secondColor.Bottom, 0.5f);
                                mat.SetColor("_TopColor", top);
                                mat.SetColor("_MiddleColor", middle);
                                mat.SetColor("_BottomColor", bottom);
                                mat.SetColor("_SpecColor", middle);
                            }



                            // 0.6 - Twin material
                            if (useTwinShader)
                            {
                                mat.EnableKeyword("_ENABLETWINEFFECT_ON");

                                if (settings.HasFlag(LargoSettings.KeepFirstTwinColor))
                                {
                                    mat.SetColor("_TwinTopColor", firstColorTwin.Top);
                                    mat.SetColor("_TwinMiddleColor", firstColorTwin.Middle);
                                    mat.SetColor("_TwinBottomColor", firstColor.Bottom);
                                }
                                else if (settings.HasFlag(LargoSettings.KeepSecondTwinColor))
                                {
                                    mat.SetColor("_TwinTopColor", secondColorTwin.Top);
                                    mat.SetColor("_TwinMiddleColor", secondColorTwin.Middle);
                                    mat.SetColor("_TwinBottomColor", secondColorTwin.Bottom);
                                }
                                else if (settings.HasFlag(LargoSettings.MergeTwinColors))
                                {
                                    var top = Color.Lerp(firstColorTwin.Top, secondColorTwin.Top, 0.5f);
                                    var middle = Color.Lerp(firstColorTwin.Middle, secondColorTwin.Middle, 0.5f);
                                    var bottom = Color.Lerp(firstColorTwin.Bottom, secondColorTwin.Bottom, 0.5f);
                                    mat.SetColor("_TwinTopColor", top);
                                    mat.SetColor("_TwinMiddleColor", middle);
                                    mat.SetColor("_TwinBottomColor", bottom);
                                }

                                if (useSloomberShader)
                                {
                                    mat.SetTexture("_SloomberColorOverlay",
                                        sloomberMat.GetTexture("_SloomberColorOverlay"));
                                    mat.SetTexture("_SloomberStarMask", sloomberMat.GetTexture("_SloomberStarMask"));

                                    mat.EnableKeyword("_BODYCOLORING_SLOOMBER");
                                    mat.DisableKeyword("_BODYCOLORING_DEFAULT");

                                    if (settings.HasFlag(LargoSettings.KeepFirstColor))
                                    {
                                        mat.SetColor("_SloomberTopColor", firstColorSloomber.Top);
                                        mat.SetColor("_SloomberMiddleColor", firstColorSloomber.Middle);
                                        mat.SetColor("_SloomberBottomColor", firstColorSloomber.Bottom);
                                    }
                                    else if (settings.HasFlag(LargoSettings.KeepSecondColor))
                                    {
                                        mat.SetColor("_SloomberTopColor", secondColorSloomber.Top);
                                        mat.SetColor("_SloomberMiddleColor", secondColorSloomber.Middle);
                                        mat.SetColor("_SloomberBottomColor", secondColorSloomber.Bottom);
                                    }
                                    else if (settings.HasFlag(LargoSettings.MergeColors))
                                    {
                                        var top = Color.Lerp(firstColorSloomber.Top, secondColorSloomber.Top, 0.5f);
                                        var middle = Color.Lerp(firstColorSloomber.Middle, secondColorSloomber.Middle,
                                            0.5f);
                                        var bottom = Color.Lerp(firstColorSloomber.Bottom, secondColorSloomber.Bottom,
                                            0.5f);
                                        mat.SetColor("_SloomberTopColor", top);
                                        mat.SetColor("_SloomberMiddleColor", middle);
                                        mat.SetColor("_SloomberBottomColor", bottom);
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            else if (structure != null && !newStructures.Contains(structure) && structure.DefaultMaterials.Length != 0)
            {
                var newStructure = new SlimeAppearanceStructure(structure);
                newStructures.Add(newStructure);
                var mat = Object.Instantiate(structure.DefaultMaterials[0]);
                structure.DefaultMaterials[0] = mat;

                try
                {
                    if (settings.HasFlag(LargoSettings.KeepFirstColor))
                    {
                        mat.SetColor("_TopColor", firstColor.Top);
                        mat.SetColor("_MiddleColor", firstColor.Middle);
                        mat.SetColor("_BottomColor", firstColor.Bottom);
                        mat.SetColor("_SpecColor", firstColor.Middle);
                    }
                    else if (settings.HasFlag(LargoSettings.KeepSecondColor))
                    {
                        mat.SetColor("_TopColor", secondColor.Top);
                        mat.SetColor("_MiddleColor", secondColor.Middle);
                        mat.SetColor("_BottomColor", secondColor.Bottom);
                        mat.SetColor("_SpecColor", secondColor.Middle);
                    }
                    else if (settings.HasFlag(LargoSettings.MergeColors))
                    {
                        var top = Color.Lerp(firstColor.Top, secondColor.Top, 0.5f);
                        var middle = Color.Lerp(firstColor.Middle, secondColor.Middle, 0.5f);
                        var bottom = Color.Lerp(firstColor.Bottom, secondColor.Bottom, 0.5f);
                        mat.SetColor("_TopColor", top);
                        mat.SetColor("_MiddleColor", middle);
                        mat.SetColor("_BottomColor", bottom);
                        mat.SetColor("_SpecColor", middle);
                    }

                    if (useSloomberShader)
                    {
                        mat.SetTexture("_SloomberColorOverlay", sloomberMat.GetTexture("_SloomberColorOverlay"));
                        mat.SetTexture("_SloomberStarMask", sloomberMat.GetTexture("_SloomberStarMask"));

                        mat.EnableKeyword("_BODYCOLORING_SLOOMBER");
                        mat.DisableKeyword("_BODYCOLORING_DEFAULT");

                        if (settings.HasFlag(LargoSettings.KeepFirstColor))
                        {
                            mat.SetColor("_SloomberTopColor", firstColorSloomber.Top);
                            mat.SetColor("_SloomberMiddleColor", firstColorSloomber.Middle);
                            mat.SetColor("_SloomberBottomColor", firstColorSloomber.Bottom);
                        }
                        else if (settings.HasFlag(LargoSettings.KeepSecondColor))
                        {
                            mat.SetColor("_SloomberTopColor", secondColorSloomber.Top);
                            mat.SetColor("_SloomberMiddleColor", secondColorSloomber.Middle);
                            mat.SetColor("_SloomberBottomColor", secondColorSloomber.Bottom);
                        }
                        else if (settings.HasFlag(LargoSettings.MergeColors))
                        {
                            var top = Color.Lerp(firstColorSloomber.Top, secondColorSloomber.Top, 0.5f);
                            var middle = Color.Lerp(firstColorSloomber.Middle, secondColorSloomber.Middle, 0.5f);
                            var bottom = Color.Lerp(firstColorSloomber.Bottom, secondColorSloomber.Bottom, 0.5f);
                            mat.SetColor("_SloomberTopColor", top);
                            mat.SetColor("_SloomberMiddleColor", middle);
                            mat.SetColor("_SloomberBottomColor", bottom);
                        }
                    }

                    if (useTwinShader)
                    {
                        mat.EnableKeyword("_ENABLETWINEFFECT_ON");

                        if (settings.HasFlag(LargoSettings.KeepFirstTwinColor))
                        {
                            mat.SetColor("_TwinTopColor", firstColorTwin.Top);
                            mat.SetColor("_TwinMiddleColor", firstColorTwin.Middle);
                            mat.SetColor("_TwinBottomColor", firstColorTwin.Bottom);
                        }
                        else if (settings.HasFlag(LargoSettings.KeepSecondTwinColor))
                        {
                            mat.SetColor("_TwinTopColor", secondColorTwin.Top);
                            mat.SetColor("_TwinMiddleColor", secondColorTwin.Middle);
                            mat.SetColor("_TwinBottomColor", secondColorTwin.Bottom);
                        }
                        else if (settings.HasFlag(LargoSettings.MergeTwinColors))
                        {
                            var top = Color.Lerp(firstColorTwin.Top, secondColorTwin.Top, 0.5f);
                            var middle = Color.Lerp(firstColorTwin.Middle, secondColorTwin.Middle, 0.5f);
                            var bottom = Color.Lerp(firstColorTwin.Bottom, secondColorTwin.Bottom, 0.5f);
                            mat.SetColor("_TwinTopColor", top);
                            mat.SetColor("_TwinMiddleColor", middle);
                            mat.SetColor("_TwinBottomColor", bottom);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        foreach (var structure in slime2.Structures)
        {
            if (structure.Element.Type == SlimeAppearanceElement.ElementType.FACE ||
                structure.Element.Type == SlimeAppearanceElement.ElementType.FACE_ATTACH)
            {
                if (settings.HasFlag(LargoSettings.KeepSecondFace))
                {
                    if (structure != null && !newStructures.Contains(structure) &&
                        structure.DefaultMaterials.Length != 0)
                    {
                        var newStructure = new SlimeAppearanceStructure(structure);
                        newStructures.Add(newStructure);
                        var mat = Object.Instantiate(structure.DefaultMaterials[0]);
                        newStructure.DefaultMaterials[0] = mat;


                        try
                        {
                            if (settings.HasFlag(LargoSettings.KeepFirstColor))
                            {
                                mat.SetColor("_TopColor", firstColor.Top);
                                mat.SetColor("_MiddleColor", firstColor.Middle);
                                mat.SetColor("_BottomColor", firstColor.Bottom);
                                mat.SetColor("_SpecColor", firstColor.Middle);
                            }
                            else if (settings.HasFlag(LargoSettings.KeepSecondColor))
                            {
                                mat.SetColor("_TopColor", secondColor.Top);
                                mat.SetColor("_MiddleColor", secondColor.Middle);
                                mat.SetColor("_BottomColor", secondColor.Bottom);
                                mat.SetColor("_SpecColor", secondColor.Middle);
                            }
                            else if (settings.HasFlag(LargoSettings.MergeColors))
                            {
                                var top = Color.Lerp(firstColor.Top, secondColor.Top, 0.5f);
                                var middle = Color.Lerp(firstColor.Middle, secondColor.Middle, 0.5f);
                                var bottom = Color.Lerp(firstColor.Bottom, secondColor.Bottom, 0.5f);
                                mat.SetColor("_TopColor", top);
                                mat.SetColor("_MiddleColor", middle);
                                mat.SetColor("_BottomColor", bottom);
                                mat.SetColor("_SpecColor", middle);
                            }

                            if (useSloomberShader)
                            {
                                mat.SetTexture("_SloomberColorOverlay",
                                    sloomberMat.GetTexture("_SloomberColorOverlay"));
                                mat.SetTexture("_SloomberStarMask", sloomberMat.GetTexture("_SloomberStarMask"));

                                mat.EnableKeyword("_BODYCOLORING_SLOOMBER");
                                mat.DisableKeyword("_BODYCOLORING_DEFAULT");

                                if (settings.HasFlag(LargoSettings.KeepFirstColor))
                                {
                                    mat.SetColor("_SloomberTopColor", firstColorSloomber.Top);
                                    mat.SetColor("_SloomberMiddleColor", firstColorSloomber.Middle);
                                    mat.SetColor("_SloomberBottomColor", firstColorSloomber.Bottom);
                                }
                                else if (settings.HasFlag(LargoSettings.KeepSecondColor))
                                {
                                    mat.SetColor("_SloomberTopColor", secondColorSloomber.Top);
                                    mat.SetColor("_SloomberMiddleColor", secondColorSloomber.Middle);
                                    mat.SetColor("_SloomberBottomColor", secondColorSloomber.Bottom);
                                }
                                else if (settings.HasFlag(LargoSettings.MergeColors))
                                {
                                    var top = Color.Lerp(firstColorSloomber.Top, secondColorSloomber.Top, 0.5f);
                                    var middle = Color.Lerp(firstColorSloomber.Middle, secondColorSloomber.Middle,
                                        0.5f);
                                    var bottom = Color.Lerp(firstColorSloomber.Bottom, secondColorSloomber.Bottom,
                                        0.5f);
                                    mat.SetColor("_SloomberTopColor", top);
                                    mat.SetColor("_SloomberMiddleColor", middle);
                                    mat.SetColor("_SloomberBottomColor", bottom);
                                }
                            }

                            if (useTwinShader)
                            {
                                mat.EnableKeyword("_ENABLETWINEFFECT_ON");

                                if (settings.HasFlag(LargoSettings.KeepFirstTwinColor))
                                {
                                    mat.SetColor("_TwinTopColor", firstColorTwin.Top);
                                    mat.SetColor("_TwinMiddleColor", firstColorTwin.Middle);
                                    mat.SetColor("_TwinBottomColor", firstColorTwin.Bottom);
                                }
                                else if (settings.HasFlag(LargoSettings.KeepSecondTwinColor))
                                {
                                    mat.SetColor("_TwinTopColor", secondColorTwin.Top);
                                    mat.SetColor("_TwinMiddleColor", secondColorTwin.Middle);
                                    mat.SetColor("_TwinBottomColor", secondColorTwin.Bottom);
                                }
                                else if (settings.HasFlag(LargoSettings.MergeTwinColors))
                                {
                                    var top = Color.Lerp(firstColorTwin.Top, secondColorTwin.Top, 0.5f);
                                    var middle = Color.Lerp(firstColorTwin.Middle, secondColorTwin.Middle, 0.5f);
                                    var bottom = Color.Lerp(firstColorTwin.Bottom, secondColorTwin.Bottom, 0.5f);
                                    mat.SetColor("_TwinTopColor", top);
                                    mat.SetColor("_TwinMiddleColor", middle);
                                    mat.SetColor("_TwinBottomColor", bottom);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            else if (structure.Element.Type == SlimeAppearanceElement.ElementType.BODY)
            {
                if (settings.HasFlag(LargoSettings.KeepSecondBody))
                {
                    if (!newStructures.Contains(structure))
                    {
                        var newStructure = new SlimeAppearanceStructure(structure);
                        newStructures.Add(newStructure);
                        var mat = Object.Instantiate(structure.DefaultMaterials[0]);
                        newStructure.DefaultMaterials[0] = mat;


                        try
                        {
                            if (settings.HasFlag(LargoSettings.KeepFirstColor))
                            {
                                mat.SetColor("_TopColor", firstColor.Top);
                                mat.SetColor("_MiddleColor", firstColor.Middle);
                                mat.SetColor("_BottomColor", firstColor.Bottom);
                                mat.SetColor("_SpecColor", firstColor.Middle);
                            }
                            else if (settings.HasFlag(LargoSettings.KeepSecondColor))
                            {
                                mat.SetColor("_TopColor", secondColor.Top);
                                mat.SetColor("_MiddleColor", secondColor.Middle);
                                mat.SetColor("_BottomColor", secondColor.Bottom);
                                mat.SetColor("_SpecColor", secondColor.Middle);
                            }
                            else if (settings.HasFlag(LargoSettings.MergeColors))
                            {
                                var top = Color.Lerp(firstColor.Top, secondColor.Top, 0.5f);
                                var middle = Color.Lerp(firstColor.Middle, secondColor.Middle, 0.5f);
                                var bottom = Color.Lerp(firstColor.Bottom, secondColor.Bottom, 0.5f);
                                mat.SetColor("_TopColor", top);
                                mat.SetColor("_MiddleColor", middle);
                                mat.SetColor("_BottomColor", bottom);
                                mat.SetColor("_SpecColor", middle);
                            }

                            if (useSloomberShader)
                            {
                                mat.SetTexture("_SloomberColorOverlay",
                                    sloomberMat.GetTexture("_SloomberColorOverlay"));
                                mat.SetTexture("_SloomberStarMask", sloomberMat.GetTexture("_SloomberStarMask"));

                                mat.EnableKeyword("_BODYCOLORING_SLOOMBER");
                                mat.DisableKeyword("_BODYCOLORING_DEFAULT");

                                if (settings.HasFlag(LargoSettings.KeepFirstColor))
                                {
                                    mat.SetColor("_SloomberTopColor", firstColorSloomber.Top);
                                    mat.SetColor("_SloomberMiddleColor", firstColorSloomber.Middle);
                                    mat.SetColor("_SloomberBottomColor", firstColorSloomber.Bottom);
                                }
                                else if (settings.HasFlag(LargoSettings.KeepSecondColor))
                                {
                                    mat.SetColor("_SloomberTopColor", secondColorSloomber.Top);
                                    mat.SetColor("_SloomberMiddleColor", secondColorSloomber.Middle);
                                    mat.SetColor("_SloomberBottomColor", secondColorSloomber.Bottom);
                                }
                                else if (settings.HasFlag(LargoSettings.MergeColors))
                                {
                                    var top = Color.Lerp(firstColorSloomber.Top, secondColorSloomber.Top, 0.5f);
                                    var middle = Color.Lerp(firstColorSloomber.Middle, secondColorSloomber.Middle,
                                        0.5f);
                                    var bottom = Color.Lerp(firstColorSloomber.Bottom, secondColorSloomber.Bottom,
                                        0.5f);
                                    mat.SetColor("_SloomberTopColor", top);
                                    mat.SetColor("_SloomberMiddleColor", middle);
                                    mat.SetColor("_SloomberBottomColor", bottom);
                                }
                            }

                            if (useTwinShader)
                            {
                                mat.EnableKeyword("_ENABLETWINEFFECT_ON");

                                if (settings.HasFlag(LargoSettings.KeepFirstTwinColor))
                                {
                                    mat.SetColor("_TwinTopColor", firstColorTwin.Top);
                                    mat.SetColor("_TwinMiddleColor", firstColorTwin.Middle);
                                    mat.SetColor("_TwinBottomColor", firstColorTwin.Bottom);
                                }
                                else if (settings.HasFlag(LargoSettings.KeepSecondTwinColor))
                                {
                                    mat.SetColor("_TwinTopColor", secondColorTwin.Top);
                                    mat.SetColor("_TwinMiddleColor", secondColorTwin.Middle);
                                    mat.SetColor("_TwinBottomColor", secondColorTwin.Bottom);
                                }
                                else if (settings.HasFlag(LargoSettings.MergeTwinColors))
                                {
                                    var top = Color.Lerp(firstColorTwin.Top, secondColorTwin.Top, 0.5f);
                                    var middle = Color.Lerp(firstColorTwin.Middle, secondColorTwin.Middle, 0.5f);
                                    var bottom = Color.Lerp(firstColorTwin.Bottom, secondColorTwin.Bottom, 0.5f);
                                    mat.SetColor("_TwinTopColor", top);
                                    mat.SetColor("_TwinMiddleColor", middle);
                                    mat.SetColor("_TwinBottomColor", bottom);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }

                }
            }
            else if (structure != null && !newStructures.Contains(structure) && structure.DefaultMaterials.Length != 0)
            {

                var newStructure = new SlimeAppearanceStructure(structure);
                newStructures.Add(newStructure);
                var mat = Object.Instantiate(structure.DefaultMaterials[0]);
                newStructure.DefaultMaterials[0] = mat;

                try
                {
                    if (settings.HasFlag(LargoSettings.KeepFirstColor))
                    {
                        mat.SetColor("_TopColor", firstColor.Top);
                        mat.SetColor("_MiddleColor", firstColor.Middle);
                        mat.SetColor("_BottomColor", firstColor.Bottom);
                        mat.SetColor("_SpecColor", firstColor.Middle);
                    }
                    else if (settings.HasFlag(LargoSettings.KeepSecondColor))
                    {
                        mat.SetColor("_TopColor", secondColor.Top);
                        mat.SetColor("_MiddleColor", secondColor.Middle);
                        mat.SetColor("_BottomColor", secondColor.Bottom);
                        mat.SetColor("_SpecColor", secondColor.Middle);
                    }
                    else if (settings.HasFlag(LargoSettings.MergeColors))
                    {
                        var top = Color.Lerp(firstColor.Top, secondColor.Top, 0.5f);
                        var middle = Color.Lerp(firstColor.Middle, secondColor.Middle, 0.5f);
                        var bottom = Color.Lerp(firstColor.Bottom, secondColor.Bottom, 0.5f);
                        mat.SetColor("_TopColor", top);
                        mat.SetColor("_MiddleColor", middle);
                        mat.SetColor("_BottomColor", bottom);
                        mat.SetColor("_SpecColor", middle);
                    }

                    if (useSloomberShader)
                    {
                        mat.SetTexture("_SloomberColorOverlay", sloomberMat.GetTexture("_SloomberColorOverlay"));
                        mat.SetTexture("_SloomberStarMask", sloomberMat.GetTexture("_SloomberStarMask"));

                        mat.EnableKeyword("_BODYCOLORING_SLOOMBER");
                        mat.DisableKeyword("_BODYCOLORING_DEFAULT");

                        if (settings.HasFlag(LargoSettings.KeepFirstColor))
                        {
                            mat.SetColor("_SloomberTopColor", firstColorSloomber.Top);
                            mat.SetColor("_SloomberMiddleColor", firstColorSloomber.Middle);
                            mat.SetColor("_SloomberBottomColor", firstColorSloomber.Bottom);
                        }
                        else if (settings.HasFlag(LargoSettings.KeepSecondColor))
                        {
                            mat.SetColor("_SloomberTopColor", secondColorSloomber.Top);
                            mat.SetColor("_SloomberMiddleColor", secondColorSloomber.Middle);
                            mat.SetColor("_SloomberBottomColor", secondColorSloomber.Bottom);
                        }
                        else if (settings.HasFlag(LargoSettings.MergeColors))
                        {
                            var top = Color.Lerp(firstColorSloomber.Top, secondColorSloomber.Top, 0.5f);
                            var middle = Color.Lerp(firstColorSloomber.Middle, secondColorSloomber.Middle, 0.5f);
                            var bottom = Color.Lerp(firstColorSloomber.Bottom, secondColorSloomber.Bottom, 0.5f);
                            mat.SetColor("_SloomberTopColor", top);
                            mat.SetColor("_SloomberMiddleColor", middle);
                            mat.SetColor("_SloomberBottomColor", bottom);
                        }
                    }

                    if (useTwinShader)
                    {
                        mat.EnableKeyword("_ENABLETWINEFFECT_ON");

                        if (settings.HasFlag(LargoSettings.KeepFirstTwinColor))
                        {
                            mat.SetColor("_TwinTopColor", firstColorTwin.Top);
                            mat.SetColor("_TwinMiddleColor", firstColorTwin.Middle);
                            mat.SetColor("_TwinBottomColor", firstColorTwin.Bottom);
                        }
                        else if (settings.HasFlag(LargoSettings.KeepSecondTwinColor))
                        {
                            mat.SetColor("_TwinTopColor", secondColorTwin.Top);
                            mat.SetColor("_TwinMiddleColor", secondColorTwin.Middle);
                            mat.SetColor("_TwinBottomColor", secondColorTwin.Bottom);
                        }
                        else if (settings.HasFlag(LargoSettings.MergeTwinColors))
                        {
                            var top = Color.Lerp(firstColorTwin.Top, secondColorTwin.Top, 0.5f);
                            var middle = Color.Lerp(firstColorTwin.Middle, secondColorTwin.Middle, 0.5f);
                            var bottom = Color.Lerp(firstColorTwin.Bottom, secondColorTwin.Bottom, 0.5f);
                            mat.SetColor("_TwinTopColor", top);
                            mat.SetColor("_TwinMiddleColor", middle);
                            mat.SetColor("_TwinBottomColor", bottom);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        return new Il2CppReferenceArray<SlimeAppearanceStructure>(newStructures.ToArray());
    }

    public static SlimeDefinition CreateCompleteLargo(SlimeDefinition slimeOne, SlimeDefinition slimeTwo,
        LargoSettings settings)
    {
        SlimeDefinition pinkRock = Get<SlimeDefinition>("PinkRock");
        if (slimeOne.IsLargo || slimeTwo.IsLargo)
            return null;
        slimeOne.CanLargofy = true;
        slimeTwo.CanLargofy = true;

        SlimeDefinition slimedef = Object.Instantiate(pinkRock);
        slimedef.BaseSlimes = new[]
        {
            slimeOne, slimeTwo
        };
        slimedef.SlimeModules = new[]
        {
            Get<GameObject>("moduleSlime" + slimeOne.name), Get<GameObject>("moduleSlime" + slimeTwo.name)
        };


        slimedef._pediaPersistenceSuffix = slimeOne.name.ToLower() + "_" + slimeTwo.name.ToLower() + "_largo";
        slimedef.referenceId = "SlimeDefinition." + slimeOne.name + slimeTwo.name;
        slimedef.localizedName = AddTranslation(slimeOne.name + " " + slimeTwo.name + " Largo",
            "l." + slimedef._pediaPersistenceSuffix);

        slimedef.FavoriteToyIdents = new Il2CppReferenceArray<ToyDefinition>(0);

        Object.DontDestroyOnLoad(slimedef);
        slimedef.hideFlags = HideFlags.HideAndDontSave;
        slimedef.name = slimeOne.name + slimeTwo.name;
        ;
        slimedef.Name = slimeOne.name + " " + slimeTwo.name;
        ;

        slimedef.prefab = Object.Instantiate(pinkRock.prefab, rootOBJ.transform);
        slimedef.prefab.name = $"slime{slimeOne.name + slimeTwo.name}";
        slimedef.prefab.GetComponent<Identifiable>().identType = slimedef;
        slimedef.prefab.GetComponent<SlimeEat>().SlimeDefinition = slimedef;
        slimedef.prefab.GetComponent<SlimeAppearanceApplicator>().SlimeDefinition = slimedef;
        slimedef.prefab.GetComponent<PlayWithToys>().SlimeDefinition = slimedef;
        slimedef.prefab.GetComponent<ReactToToyNearby>().SlimeDefinition = slimedef;
        slimedef.prefab.RemoveComponent<RockSlimeRoll>();
        slimedef.prefab.RemoveComponent<DamagePlayerOnTouch>();

        SlimeAppearance appearance = Object.Instantiate(pinkRock.AppearancesDefault[0]);
        slimedef.AppearancesDefault[0] = appearance;
        Object.DontDestroyOnLoad(appearance);
        appearance.name = slimeOne.AppearancesDefault[0].name + slimeTwo.AppearancesDefault[0].name;

        appearance._dependentAppearances = new[]
        {
            slimeOne.AppearancesDefault[0], slimeTwo.AppearancesDefault[0]
        };
        appearance._structures = MergeStructures(appearance._dependentAppearances[0],
            appearance._dependentAppearances[1], settings);
        slimedef.Diet = MergeDiet(slimeOne.Diet, slimeTwo.Diet);
        SlimeDefinition tarr = Get<SlimeDefinition>("Tarr"); /*
        slimeOne.Diet.EatMap.Add(CreateEatmap(SlimeEmotions.Emotion.AGITATION, 0.5f, null,
           slimeTwo.Diet.ProduceIdents[0],slimedef));
        slimeTwo.Diet.EatMap.Add(CreateEatmap(SlimeEmotions.Emotion.AGITATION, 0.5f, null,
            slimeOne.Diet.ProduceIdents[0],slimedef));
        foreach (SlimeDiet.EatMapEntry entry in slimedef.Diet.EatMap)
            if (entry.EatsIdent.IsPlort)
                if (entry.EatsIdent.ValidatableName == slimeOne.Diet.ProduceIdents[0].ValidatableName || entry.EatsIdent.ValidatableName == slimeTwo.Diet.ProduceIdents[0].ValidatableName)
                    slimedef.Diet.EatMap.Remove(entry);
        foreach (SlimeDiet.EatMapEntry entry in slimedef.Diet.EatMap)
            entry.BecomesIdent = tarr;

        slimedef.SetProduceIdent(slimeOne.Diet.ProduceIdents[0],0);
        slimedef.SetProduceIdent(slimeTwo.Diet.ProduceIdents[0],1);*/
        slimedef.RefreshEatmap();

        slimeDefinitions.Slimes.Add(slimedef);
        slimeDefinitions._slimeDefinitionsByIdentifiable.TryAdd(slimedef, slimedef);
        slimeDefinitions._largoDefinitionByBaseDefinitions.TryAdd(new SlimeDefinitions.SlimeDefinitionPair()
            {
                SlimeDefinition1 = slimeOne,
                SlimeDefinition2 = slimeTwo
            },
            slimedef);
        mainAppearanceDirector.RegisterDependentAppearances(slimedef, slimedef.AppearancesDefault[0]);
        mainAppearanceDirector.UpdateChosenSlimeAppearance(slimedef, slimedef.AppearancesDefault[0]);

        slimedef.AddToGroup("LargoGroup");
        slimedef.AddToGroup("SlimesGroup");
        INTERNAL_SetupLoadForIdent(slimedef.referenceId, slimedef);

        slimeOne.RefreshEatmap();
        slimeTwo.RefreshEatmap();

        return slimedef;
    }

    public static void AddStructure(this SlimeAppearance appearance, SlimeAppearanceStructure structure)
    {
        appearance.Structures.Add(structure);
    }

    public static SlimeDiet MergeDiet(this SlimeDiet firstDiet, SlimeDiet secondDiet)
    {
        var mergedDiet = INTERNAL_CreateNewDiet();

        mergedDiet.EatMap.AddListRangeNoMultiple(firstDiet.EatMap);
        mergedDiet.EatMap.AddListRangeNoMultiple(secondDiet.EatMap);

        mergedDiet.AdditionalFoodIdents =
            mergedDiet.AdditionalFoodIdents.AddRangeNoMultiple(firstDiet.AdditionalFoodIdents);
        mergedDiet.AdditionalFoodIdents =
            mergedDiet.AdditionalFoodIdents.AddRangeNoMultiple(secondDiet.AdditionalFoodIdents);

        mergedDiet.FavoriteIdents = mergedDiet.FavoriteIdents.AddRangeNoMultiple(firstDiet.FavoriteIdents);
        mergedDiet.FavoriteIdents = mergedDiet.FavoriteIdents.AddRangeNoMultiple(secondDiet.FavoriteIdents);

        mergedDiet.MajorFoodIdentifiableTypeGroups =
            mergedDiet.MajorFoodIdentifiableTypeGroups.AddRangeNoMultiple(firstDiet.MajorFoodIdentifiableTypeGroups);
        mergedDiet.MajorFoodIdentifiableTypeGroups =
            mergedDiet.MajorFoodIdentifiableTypeGroups.AddRangeNoMultiple(secondDiet.MajorFoodIdentifiableTypeGroups);

        mergedDiet.ProduceIdents = mergedDiet.ProduceIdents.AddRangeNoMultiple(firstDiet.ProduceIdents);
        mergedDiet.ProduceIdents = mergedDiet.ProduceIdents.AddRangeNoMultiple(secondDiet.ProduceIdents);

        return mergedDiet;
    }

    public static void SwitchSlimeAppearances(this SlimeDefinition slimeOneDef, SlimeDefinition slimeTwoDef)
    {
        var appearanceOne = slimeOneDef.AppearancesDefault[0]._structures;
        slimeOneDef.AppearancesDefault[0]._structures = slimeTwoDef.AppearancesDefault[0]._structures;
        slimeTwoDef.AppearancesDefault[0]._structures = appearanceOne;
        var appearanceSplatOne = slimeOneDef.AppearancesDefault[0]._splatColor;
        slimeOneDef.AppearancesDefault[0]._splatColor = slimeTwoDef.AppearancesDefault[0]._splatColor;
        slimeTwoDef.AppearancesDefault[0]._splatColor = appearanceSplatOne;

        var colorPalate = slimeOneDef.AppearancesDefault[0]._colorPalette;
        slimeOneDef.AppearancesDefault[0]._colorPalette = slimeTwoDef.AppearancesDefault[0]._colorPalette;
        slimeTwoDef.AppearancesDefault[0]._colorPalette = colorPalate;

        var structureIcon = slimeOneDef.AppearancesDefault[0]._icon;
        slimeOneDef.AppearancesDefault[0]._icon = slimeTwoDef.AppearancesDefault[0]._icon;
        slimeTwoDef.AppearancesDefault[0]._icon = structureIcon;
        var icon = slimeOneDef.icon;
        slimeOneDef.icon = slimeTwoDef.icon;
        slimeTwoDef.icon = icon;

        var debugIcon = slimeOneDef.debugIcon;
        slimeOneDef.debugIcon = slimeTwoDef.debugIcon;
        slimeTwoDef.debugIcon = debugIcon;

    }

    public static SlimeDiet.EatMapEntry CreateEatmap(SlimeEmotions.Emotion driver, float mindrive,
        IdentifiableType produce, IdentifiableType eat, IdentifiableType becomes)
    {
        var eatmap = new SlimeDiet.EatMapEntry
        {
            EatsIdent = eat,
            ProducesIdent = produce,
            BecomesIdent = becomes,
            Driver = driver,
            MinDrive = mindrive
        };
        return eatmap;
    }

    public static SlimeDiet.EatMapEntry CreateEatmap(SlimeEmotions.Emotion driver, float mindrive,
        IdentifiableType produce, IdentifiableType eat)
    {
        var eatmap = new SlimeDiet.EatMapEntry
        {
            EatsIdent = eat,
            ProducesIdent = produce,
            Driver = driver,
            MinDrive = mindrive
        };
        return eatmap;
    }

    public static void ModifyEatmap(this SlimeDiet.EatMapEntry eatmap, SlimeEmotions.Emotion driver, float mindrive,
        IdentifiableType produce, IdentifiableType eat, IdentifiableType becomes)
    {
        eatmap.EatsIdent = eat;
        eatmap.BecomesIdent = becomes;
        eatmap.ProducesIdent = produce;
        eatmap.Driver = driver;
        eatmap.MinDrive = mindrive;
    }

    public static void ModifyEatmap(this SlimeDiet.EatMapEntry eatmap, SlimeEmotions.Emotion driver, float mindrive,
        IdentifiableType produce, IdentifiableType eat)
    {
        eatmap.EatsIdent = eat;
        eatmap.ProducesIdent = produce;
        eatmap.Driver = driver;
        eatmap.MinDrive = mindrive;
    }

    public static void AddProduceIdent(this SlimeDefinition slimedef, IdentifiableType ident)
    {
        slimedef.Diet.ProduceIdents = slimedef.Diet.ProduceIdents.Add(ident);
    }

    public static void SetProduceIdent(this SlimeDefinition slimedef, IdentifiableType ident, int index)
    {
        slimedef.Diet.ProduceIdents[index] = ident;
    }

    public static void AddExtraEatIdent(this SlimeDefinition slimedef, IdentifiableType ident)
    {
        slimedef.Diet.AdditionalFoodIdents = slimedef.Diet.AdditionalFoodIdents.Add(ident);

    }

    public static void SetFavoriteProduceCount(this SlimeDefinition slimedef, int count)
    {
        slimedef.Diet.FavoriteProductionCount = count;
    }

    public static void AddFavorite(this SlimeDefinition slimedef, IdentifiableType id)
    {
        slimedef.Diet.FavoriteIdents = slimedef.Diet.FavoriteIdents.Add(id);
    }

    public static void AddEatmapToSlime(this SlimeDefinition slimedef, SlimeDiet.EatMapEntry eatmap)
    {
        slimedef.Diet.EatMap.Add(eatmap);
    }

    public static void SetStructColor(this SlimeAppearanceStructure structure, int id, Color color)
    {
        structure.DefaultMaterials[0].SetColor(id, color);
    }

    public static void RefreshEatmap(this SlimeDefinition def)
    {
        def.Diet.RefreshEatMap(slimeDefinitions, def);
    }

    public static void ChangeSlimeFoodGroup(this SlimeDefinition def, IdentifiableTypeGroup FG, int index)
    {
        def.Diet.MajorFoodIdentifiableTypeGroups[index] = FG;
    }

    public static void AddSlimeFoodGroup(this SlimeDefinition def, IdentifiableTypeGroup FG)
    {
        def.Diet.MajorFoodIdentifiableTypeGroups = def.Diet.MajorFoodIdentifiableTypeGroups.Add(FG);
        def.Diet.MajorFoodIdentifiableTypeGroups = def.Diet.MajorFoodIdentifiableTypeGroups.Add(FG);
    }

    internal static SlimeDiet INTERNAL_CreateNewDiet()
    {
        var diet = new SlimeDiet();

        diet.ProduceIdents = new Il2CppReferenceArray<IdentifiableType>(0);
        diet.FavoriteProductionCount = 2;
        diet.EatMap = new Il2CppSystem.Collections.Generic.List<SlimeDiet.EatMapEntry>(0);
        diet.FavoriteIdents = new Il2CppReferenceArray<IdentifiableType>(0);
        diet.AdditionalFoodIdents = new Il2CppReferenceArray<IdentifiableType>(0);
        diet.MajorFoodIdentifiableTypeGroups = new Il2CppReferenceArray<IdentifiableTypeGroup>(0);
        diet.BecomesOnTarrifyIdentifiableType = Get<IdentifiableType>("Tarr");
        diet.EdiblePlortIdentifiableTypeGroup = Get<IdentifiableTypeGroup>("EdiblePlortFoodGroup");

        // 0.6 - Unstable identifiables
        diet.StableResourceIdentifiableTypeGroup = Get<IdentifiableTypeGroup>("StableResourcesGroup");
        diet.UnstableResourceIdentifiableTypeGroup = Get<IdentifiableTypeGroup>("UnstableResourcesGroup");
        diet.UnstablePlort = GetPlort("UnstablePlort");

        return diet;
    }

    public static SlimeDefinition GetSlime(string name)
    {
        foreach (IdentifiableType type in slimes.GetAllMembersArray())
            if (type.name.ToUpper() == name.ToUpper())
                return type.Cast<SlimeDefinition>();

        return null;
    }

    public static void SetSlimeColor(this SlimeDefinition slimedef, Color32 Top, Color32 Middle, Color32 Bottom,
        Color32 Spec, int index, int index2, bool isSS, int structure)
    {
        Material mat = null;
        if (isSS == true)
        {
            mat = slimedef.AppearancesDynamic.ToArray()[index].Structures[structure].DefaultMaterials[index2];
        }
        else
        {
            mat = slimedef.AppearancesDefault[index].Structures[structure].DefaultMaterials[index2];
        }

        mat.SetColor("_TopColor", Top);
        mat.SetColor("_MiddleColor", Middle);
        mat.SetColor("_BottomColor", Bottom);
        mat.SetColor("_SpecColor", Spec);
    }

    public static void SetTwinColor(this SlimeDefinition slimedef, Color32 Top, Color32 Middle, Color32 Bottom,
        int index, int index2, bool isSS, int structure)
    {
        Material mat = null;
        if (isSS == true)
        {
            mat = slimedef.AppearancesDynamic.ToArray()[index].Structures[structure].DefaultMaterials[index2];
        }
        else
        {
            mat = slimedef.AppearancesDefault[index].Structures[structure].DefaultMaterials[index2];
        }

        mat.SetColor("_TwinTopColor", Top);
        mat.SetColor("_TwinMiddleColor", Middle);
        mat.SetColor("_TwinBottomColor", Bottom);
    }

    public static void SetSloomberColor(this SlimeDefinition slimedef, Color32 Top, Color32 Middle, Color32 Bottom,
        int index, int index2, bool isSS, int structure)
    {
        Material mat = null;
        if (isSS == true)
        {
            mat = slimedef.AppearancesDynamic.ToArray()[index].Structures[structure].DefaultMaterials[index2];
        }
        else
        {
            mat = slimedef.AppearancesDefault[index].Structures[structure].DefaultMaterials[index2];
        }

        mat.SetColor("_SloomberTopColor", Top);
        mat.SetColor("_SloomberMiddleColor", Middle);
        mat.SetColor("_SloomberBottomColor", Bottom);
    }

    // Twin effect uses the shader keyword "_ENABLETWINEFFECT_ON"
    public static void EnableTwinEffect(this SlimeDefinition slimeDef, int index, int index2, bool isSS, int structure)
    {
        Material mat;
        if (isSS == true)
        {
            mat = slimeDef.AppearancesDynamic.ToArray()[index].Structures[structure].DefaultMaterials[index2];
        }
        else
        {
            mat = slimeDef.AppearancesDefault[index].Structures[structure].DefaultMaterials[index2];
        }

        mat.EnableKeyword("_ENABLETWINEFFECT_ON");
    }

    public static void DisableTwinEffect(this SlimeDefinition slimeDef, int index, int index2, bool isSS, int structure)
    {

        Material mat;
        if (isSS == true)
        {
            mat = slimeDef.AppearancesDynamic.ToArray()[index].Structures[structure].DefaultMaterials[index2];
        }
        else
        {
            mat = slimeDef.AppearancesDefault[index].Structures[structure].DefaultMaterials[index2];
        }

        mat.DisableKeyword("_ENABLETWINEFFECT_ON");
    }

    public static void SetSlimeMatTopColor(this Material mat, Color color) => mat.SetColor("_TopColor", color);
    public static void SetSlimeMatMiddleColor(this Material mat, Color color) => mat.SetColor("_MiddleColor", color);

    public static void SetSlimeMatBottomColor(this Material mat, Color color) => mat.SetColor("_BottomColor", color);

    public static void SetSlimeMatColors(this Material material, Color32 Top, Color32 Middle, Color32 Bottom,
        Color32 Specular)
    {
        material.SetColor("_TopColor", Top);
        material.SetColor("_MiddleColor", Middle);
        material.SetColor("_BottomColor", Bottom);
        material.SetColor("_SpecColor", Specular);
    }

    public static void SetSlimeMatColors(this Material material, Color32 Top, Color32 Middle, Color32 Bottom)
    {
        material.SetColor("_TopColor", Top);
        material.SetColor("_MiddleColor", Middle);
        material.SetColor("_BottomColor", Bottom);
    }

    public static SlimeDefinition GetBaseSlime(string name)
    {
        foreach (IdentifiableType type in baseSlimes.GetAllMembersArray())
            if (type.name.ToUpper() == name.ToUpper())
                return type.Cast<SlimeDefinition>();
        return null;
    }

    public static SlimeDefinition GetLargo(string name)
    {
        foreach (IdentifiableType type in largos.GetAllMembersArray())
            if (type.name.ToUpper() == name.ToUpper())
                return type.Cast<SlimeDefinition>();
        return null;
    }

    [Flags]
    public enum LargoSettings
    {
        KeepFirstBody = 1 << 0,
        KeepSecondBody = 1 << 1,
        KeepFirstFace = 1 << 2,
        KeepSecondFace = 1 << 3,
        KeepFirstColor = 1 << 4,
        KeepSecondColor = 1 << 5,
        KeepFirstTwinColor = 1 << 6,
        KeepSecondTwinColor = 1 << 7,
        MergeColors = 1 << 8,
        MergeTwinColors = 1 << 9,
    }
}