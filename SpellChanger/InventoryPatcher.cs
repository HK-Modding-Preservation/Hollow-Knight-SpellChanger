using HutongGames.PlayMaker.Actions;
using SpellChanger.Utils;
using Vasi;

namespace SpellChanger;

internal class InventoryPatcher
{
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

        PatchInventoryIcons("Scream", self);
        PatchInventoryIcons("Quake", self);
        PatchInventoryIcons("Fireball", self);

    }

    private static void AddSpellCycle(string type, PlayMakerFSM UiInventoryFSM)
    {
        FsmState slotState = UiInventoryFSM.GetState(type);
        if (slotState == null) { LogError("Inventory slot for spell not found when creating cycle."); return; }

        FsmState slotConfirm = UiInventoryFSM.CreateState(type + " Confirm");
        slotConfirm.AddMethod(() => { 
            CycleSpell(type); 
            UpdateInventoryText(type, slotState);
            //to update icon
            PlayMakerFSM checkActive = GetIconFSM(type, UiInventoryFSM);
            if (checkActive == null) { return; }

            checkActive.SetState("Appear?");
        });

        FsmEvent UIConfirm = UiInventoryFSM.GetFsmEvent("UI CONFIRM");
        FsmEvent Finished = FsmEvent.Finished;

        slotState.AddTransition(UIConfirm, slotConfirm);
        slotConfirm.AddTransition(Finished, slotState);
    }

    private static void CycleSpell(string type)
    {
        if (type != "Scream" && type != "Fireball" && type != "Quake") {
            LogError("Attempted to cycle non-existent spell type.");
            return;
        }

        List<CustomSpell> spells = SpellHelper.GetSpellsOfType(type);
        if (spells.Count == 0) { return; } //No available spells to cycle

        CustomSpell equippedSpell;
        SpellHelper.equippedSpells.TryGetValue(type, out equippedSpell);
        int index = spells.FindIndex((CustomSpell check) => check == equippedSpell);
        if (equippedSpell == null) //Player has base spell equipped, cycle to first custom
        {
            CustomSpell firstCustomSpell = spells[0];
            SpellHelper.EquipCustomSpell(firstCustomSpell);
            return;
        } 

        if (index == -1) //equipped spell not found
        {
            LogError($"Equipped {type} spell not found in Custom Spell list. Returning to base spell.");
            SpellHelper.EquipBaseSpell(type);
            return;
        }

        //Cycle
        if (index == spells.Count - 1) //Return to base spell when at final spell in list
        {
            SpellHelper.EquipBaseSpell(type);
            return;
        }

        //cycle to next spell in list
        CustomSpell spellToEquip = spells[index + 1];
        SpellHelper.EquipCustomSpell(spellToEquip);
    }

    private static PlayMakerFSM GetIconFSM(string type, PlayMakerFSM UiInventoryFSM)
    {
        if (UiInventoryFSM == null) { return null; }

        GameObject Inv = UiInventoryFSM.Fsm.GameObject;
        GameObject InvItems = Inv.Child("Inv_Items");

        if (InvItems == null) { LogError("Inv_Items Gameobject not found when updating icons of type " + type); return null; }

        GameObject spellIcon = InvItems.Child("Spell " + type);
        if (spellIcon == false) { LogError("Spell icon " + type + " not found."); return null; }

        PlayMakerFSM checkActive = spellIcon.LocateMyFSM("Check Active");
        if (checkActive == false) { LogError("Check Active FSM not found when updating icons of type " + type); return null; }

        return checkActive;
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

            renderer.sprite = equippedSpell.GetSpriteLevel1();
        });

        level2State.AddMethod(() => {
            CustomSpell equippedSpell = SpellHelper.equippedSpells[type];
            if (equippedSpell == null) { return; }

            SpriteRenderer renderer = spellIcon.GetComponent<SpriteRenderer>();
            if (renderer == null) { LogError("SpriteRenderer of icon " + type + " not found."); return; }

            renderer.sprite = equippedSpell.GetSpriteLevel2();
        });
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
            buildstringName.stringParts[0] = equippedSpell.GetNameKey();
            buildstringDesc.stringParts[0] = equippedSpell.GetDescKey();
        }

        //update text
        PlayMakerFSM UpdateTextFSM = inventorySlotState.Fsm.GameObject.LocateMyFSM("Update Text");
        if (UpdateTextFSM == null) { LogError("Update Text FSM not found"); return; }
        UpdateTextFSM.SendEvent("UPDATE TEXT");
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

