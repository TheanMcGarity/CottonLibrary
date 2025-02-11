using Il2Cpp;

namespace CottonLibrary;

public static partial class Library
{
    internal static void INTERNAL_SetupSaveForIdent(string RefID, IdentifiableType ident)
    {
        GameContext.Instance.AutoSaveDirector.SavedGame.identifiableTypeLookup.TryAdd(RefID, ident);

        if (GameContext.Instance.AutoSaveDirector.SavedGame.identifiableTypeToPersistenceId._primaryIndex.Count > 0)
            if (!GameContext.Instance.AutoSaveDirector.SavedGame.identifiableTypeToPersistenceId._primaryIndex.Contains(RefID))
                GameContext.Instance.AutoSaveDirector.SavedGame.identifiableTypeToPersistenceId._primaryIndex =
                    GameContext.Instance.AutoSaveDirector.SavedGame.identifiableTypeToPersistenceId._primaryIndex
                        .AddString(RefID);

        GameContext.Instance.AutoSaveDirector.SavedGame.identifiableTypeToPersistenceId._reverseIndex.TryAdd(RefID,
            GameContext.Instance.AutoSaveDirector.SavedGame.identifiableTypeToPersistenceId._reverseIndex.Count);


        if (ident is SlimeDefinition)
        {
            if (!gameContext.SlimeDefinitions.Slimes.Contains(ident.Cast<SlimeDefinition>()))
                gameContext.SlimeDefinitions.Slimes.Add(ident.Cast<SlimeDefinition>());

            gameContext.SlimeDefinitions._slimeDefinitionsByIdentifiable.TryAdd(ident,
                ident.Cast<SlimeDefinition>());
        }

        ident.referenceId = RefID;
    }

    internal static void INTERNAL_SetupLoadForIdent(string RefID, IdentifiableType ident)
    {
        ident.SetupForSaving(RefID);

        if (!GameContext.Instance.AutoSaveDirector.identifiableTypes._memberTypes.Contains(ident))
            GameContext.Instance.AutoSaveDirector.identifiableTypes._memberTypes.Add(ident);

        gameContext.LookupDirector._identifiableTypeByRefId.TryAdd(RefID, ident);

        INTERNAL_SetupSaveForIdent(RefID, ident);
    }}