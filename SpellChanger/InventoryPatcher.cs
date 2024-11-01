using HutongGames.PlayMaker.Actions;
using SpellChanger.Utils;
using Vasi;

namespace SpellChanger;

internal class InventoryPatcher
{
    internal static Sprite cycloneIcon;
    internal static Sprite dashIcon;
    internal static Sprite greatIcon;
    internal static Dictionary<string, Sprite> regularNAIcons = new Dictionary<string, Sprite>()
    {
        { AbilityNames.DASHSLASH, cycloneIcon },
        { AbilityNames.CYCLONESLASH, dashIcon },
        { AbilityNames.GREATSLASH, greatIcon }
    };
    /// <summary>
    /// Constructs the helper, hooks needed methods.
    /// </summary>
    static InventoryPatcher()
    {
        On.PlayMakerFSM.Awake += OnPlayMakerFsmAwake;
    }

    private static void OnPlayMakerFsmAwake(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM self)
    {
        orig(self);


        if (self.gameObject.name != "Inv" || self.FsmName != "UI Inventory") { return; }


        AddSpellCycle("Scream", self);
        AddSpellCycle("Quake", self);
        AddSpellCycle("Fireball", self);
        AddSpellCycle("Dash Slash", self);
        AddSpellCycle("Cyclone", self);
        AddSpellCycle("Uppercut", self);

        PatchInventoryIcons("Scream", self);
        PatchInventoryIcons("Quake", self);
        PatchInventoryIcons("Fireball", self);

    }

    private static void AddSpellCycle(string type, PlayMakerFSM UiInventoryFSM)
    {
        FsmState slotState = UiInventoryFSM.GetState(type);
        if (slotState == null) { LogError("Inventory slot for spell " + type + " not found when creating cycle."); return; }

        //these contrarian bastards at team cherry made the great slash slot dash slash
        // and the dash slash slot uppercut
        // so i have to do this nonsense
        if (type == "Dash Slash")
        {
            type = AbilityNames.GREATSLASH;
        }
        if (type == "Uppercut")
        {
            type = AbilityNames.DASHSLASH;
        }
        if (type == "Cyclone")
        {
            type = AbilityNames.CYCLONESLASH;
        }

        regularNAIcons[AbilityNames.GREATSLASH] = GetIconNA(AbilityNames.GREATSLASH, UiInventoryFSM).GetComponent<SpriteRenderer>().sprite;
        regularNAIcons[AbilityNames.CYCLONESLASH] = GetIconNA(AbilityNames.CYCLONESLASH, UiInventoryFSM).GetComponent<SpriteRenderer>().sprite;
        regularNAIcons[AbilityNames.DASHSLASH] = GetIconNA(AbilityNames.DASHSLASH, UiInventoryFSM).GetComponent<SpriteRenderer>().sprite;

        FsmState slotConfirm = UiInventoryFSM.CreateState(type + " Confirm");
        slotConfirm.AddMethod(() => {
            CycleSpell(type); 
            if (AbilityNames.NAILARTSNAMES.Contains(type))
            {
                UpdateInventoryTextNA(type, slotState);
                UpdateInventoryIconsNA(type, UiInventoryFSM);
            }
            if (AbilityNames.SPELLCONTROLNAMES.Contains(type))
            {
                UpdateInventoryText(type, slotState);

                //to update icon
                PlayMakerFSM checkActive = GetIconFSM(type, UiInventoryFSM);
                if (checkActive == null) { return; }

                checkActive.SetState("Appear?");
            }
        });

        FsmEvent UIConfirm = UiInventoryFSM.GetFsmEvent("UI CONFIRM");
        FsmEvent Finished = FsmEvent.Finished;

        slotState.AddTransition(UIConfirm, slotConfirm);
        slotConfirm.AddTransition(Finished, slotState);
    }

    private static void CycleSpell(string type)
    {
        if (!AbilityNames.ALLNAMES.Contains(type)) {
            LogError("Attempted to cycle non-existent spell type.");
            return;
        }

        List<CustomSpell> spells = SpellHelper.GetSpellsOfType(type);
        if (spells.Count == 0) { LogError("No spells"); return; } //No available spells to cycle

        CustomSpell equippedSpell;
        SpellHelper.equippedSpells.TryGetValue(type, out equippedSpell);
        int index = spells.FindIndex((CustomSpell check) => check == equippedSpell);
        if (equippedSpell == null) //Player has base spell equipped, cycle to first custom
        {
            CustomSpell firstCustomSpell = spells[0];
            SpellHelper.ForceEquipCustomSpell(firstCustomSpell);
            return;
        } 

        if (index == -1) //equipped spell not found
        {
            LogError($"Equipped {type} spell not found in Custom Spell list. Returning to base spell.");
            SpellHelper.ForceEquipBaseSpell(type);
            return;
        }

        //Cycle
        if (index == spells.Count - 1) //Return to base spell when at final spell in list
        {
            SpellHelper.ForceEquipBaseSpell(type);
            return;
        }

        //cycle to next spell in list
        CustomSpell spellToEquip = spells[index + 1];
        SpellHelper.ForceEquipCustomSpell(spellToEquip);
    }

    private static GameObject GetIcon(PlayMakerFSM UiInventoryFSM, string type)
    {
        if (UiInventoryFSM == null) { return null; }

        GameObject Inv = UiInventoryFSM.Fsm.GameObject;
        GameObject InvItems = Inv.Child("Inv_Items");

        if (InvItems == null) { LogError("Inv_Items Gameobject not found when updating icons of type " + type); return null; }

        GameObject Icon = InvItems.Child(type);
        if (Icon == null) { LogError("Spell icon " + type + " not found."); return null; }

        return Icon;
    }

    private static PlayMakerFSM GetIconFSM(string type, PlayMakerFSM UiInventoryFSM)
    {
        GameObject spellIcon = GetIcon(UiInventoryFSM, "Spell " + type);

        if (spellIcon == null) { return null; } //error already sent by GetIcon

        PlayMakerFSM checkActive = spellIcon.LocateMyFSM("Check Active");
        if (checkActive == null) { LogError("Check Active FSM not found when updating icons of type " + type); return null; }

        return checkActive;
    }

    private static GameObject GetIconNA(string type, PlayMakerFSM UiInventoryFSM)
    {
        string typeNA = AbilityFSMs.NAIconAlias[type];

        if (typeNA == null) { LogError("Icon type " + type + "not found."); }

        GameObject Icon = GetIcon(UiInventoryFSM, "Art " + typeNA);

        return Icon;
    }

    private static void UpdateInventoryIconsNA(string type, PlayMakerFSM UiInventoryFSM)
    {
        GameObject NAIcon = GetIconNA(type, UiInventoryFSM);

        if (NAIcon == null) { return; } //Errors handled in GetIconNA

        SpriteRenderer renderer = NAIcon.GetComponent<SpriteRenderer>();
        if (renderer == null) { LogError("SpriteRenderer not found in icon " + type); return; }

        InvItemDisplay display = NAIcon.GetComponent<InvItemDisplay>();

        CustomSpell equippedSpell = SpellHelper.equippedSpells[type];
        if (equippedSpell == null) { 
            renderer.sprite = regularNAIcons[type];
            display.activeSprite = regularNAIcons[type];
            return;
        }

        renderer.sprite = equippedSpell.sprites[0];
        display.activeSprite = equippedSpell.sprites[0];
    }

    private static void PatchInventoryIcons(string type, PlayMakerFSM UiInventoryFSM)
    {
        PlayMakerFSM checkActive = GetIconFSM(type, UiInventoryFSM);
        if (checkActive == null) { return; }

        GameObject spellIcon = checkActive.gameObject;

        FsmState level1State = checkActive.GetState("Lv 1");
        FsmState level2State = checkActive.GetState("Lv 2");

        if (level1State == null || level2State == null) { LogError("One or more icon level states not found. "); return; }

        level1State.AddMethod(() => {
            CustomSpell equippedSpell = SpellHelper.equippedSpells[type];
            if (equippedSpell == null) { return; }

            SpriteRenderer renderer = spellIcon.GetComponent<SpriteRenderer>();
            if (renderer == null) { LogError("SpriteRenderer of icon " + type + " not found."); return; }

            renderer.sprite = equippedSpell.sprites[0];
        });

        level2State.AddMethod(() => {
            CustomSpell equippedSpell = SpellHelper.equippedSpells[type];
            if (equippedSpell == null) { return; }

            SpriteRenderer renderer = spellIcon.GetComponent<SpriteRenderer>();
            if (renderer == null) { LogError("SpriteRenderer of icon " + type + " not found."); return; }

            renderer.sprite = equippedSpell.sprites[1];
        });
    }

    private static void UpdateInventoryTextNA(string type, FsmState inventorySlotState)
    {
        if (inventorySlotState == null) { return; }

        SetFsmString setstringName = inventorySlotState.GetAction<SetFsmString>(2);
        SetFsmString setstringDesc = inventorySlotState.GetAction<SetFsmString>(3);

        if (setstringName == null || setstringDesc == null)
        {
            LogError("SetFsmString action not found when updating Inventory text.");
            return;
        }

        CustomSpell equippedSpell;
        SpellHelper.equippedSpells.TryGetValue(type, out equippedSpell);

        if (equippedSpell == null)
        {
            setstringName.setValue = "INV_NAME_ART_" + AbilityFSMs.baseNAAlias[type];
            setstringDesc.setValue = "INV_DESC_ART_" + AbilityFSMs.baseNAAlias[type];
        }
        else
        {
            setstringName.setValue = equippedSpell.nameKey;
            setstringDesc.setValue = equippedSpell.descKey;
        }

        UpdateTextEvent(inventorySlotState);
    }

    private static void UpdateTextEvent(FsmState inventorySlotState)
    {
        //update text
        PlayMakerFSM UpdateTextFSM = inventorySlotState.Fsm.GameObject.LocateMyFSM("Update Text");
        if (UpdateTextFSM == null) { LogError("Update Text FSM not found"); return; }
        UpdateTextFSM.SendEvent("UPDATE TEXT");
    }

    private static void UpdateInventoryText(string type, FsmState inventorySlotState)
    {
        if (inventorySlotState == null) { return; }

        BuildString buildstringName = inventorySlotState.GetAction<BuildString>(4);
        BuildString buildstringDesc = inventorySlotState.GetAction<BuildString>(5);

        if (buildstringName == null || buildstringDesc == null)
        {
            LogError("BuildString action not found when updating Inventory text.");
            return;
        }

        CustomSpell equippedSpell;
        SpellHelper.equippedSpells.TryGetValue(type, out equippedSpell);

        if (equippedSpell == null)
        {
            buildstringName.stringParts[0] = "INV_NAME_SPELL_" + type.ToUpper();
            buildstringDesc.stringParts[0] = "INV_DESC_SPELL_" + type.ToUpper();
        } else
        {
            buildstringName.stringParts[0] = equippedSpell.nameKey;
            buildstringDesc.stringParts[0] = equippedSpell.descKey;
        }

        UpdateTextEvent(inventorySlotState);
    }

    /// <summary>
    /// Used for static initialization.
    /// </summary>
    internal static void staticInit()
    {
        //I don't like doing this, but it's the best I've got.
        LogDebug("Initialising");
    }


    private static void LogDebug(string message) => Modding.Logger.LogDebug(message + "[SpellChanger]:[InventoryPatcher]");
    private static void LogError(string message) => Modding.Logger.LogError(message + "[SpellChanger]:[InventoryPatcher]");
}

