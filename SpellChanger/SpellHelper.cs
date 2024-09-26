using Vasi;

namespace SpellChanger;

public static class SpellHelper
{
    private static List<CustomSpell> _customSpells = new List<CustomSpell>();
    internal static GameObject SpellFSMStoreObject = new GameObject("SpellFsmStoreObject");
    internal static Dictionary<string, CustomSpell> equippedSpells = new Dictionary<string, CustomSpell>()
    {
        { "Scream", null },
        { "Fireball", null },
        { "Quake", null }
    };
    internal static Dictionary<string, string> baseSpellStates = new Dictionary<string, string>()
    {
        { "Scream", "Scream Get?" },
        { "Fireball", "Wallside?" },
        { "Quake", "On Ground?" }
    };

    internal static int idCounter = 0;
    /// <summary>
    /// Generates a unique number to add to the end of spell state names to prevent clashing of similarly named states.
    /// </summary>
    internal static int GenerateId()
    {
        idCounter++;
        return idCounter;
    }

    /// <summary>
    /// Constructs the helper, hooks needed methods.
    /// </summary>
    static SpellHelper()
    {
        On.PlayMakerFSM.Awake += OnPlayMakerFsmAwake;
    }

    internal static void ClearSpellList()
    {
        _customSpells.Clear();
        EquipBaseSpell("Scream");
        EquipBaseSpell("Quake");
        EquipBaseSpell("Fireball");
    }

    /// <summary>
    /// Initialises CustomSpells into the game.
    /// </summary>
    private static void SetupSpell(PlayMakerFSM spellControl, CustomSpell spell)
    {
        PlayMakerFSM fsm = spell.GetStoreFSM();
        string name = spell.GetName();
        if (fsm == null)
        {
            LogError($"Spell {name} not added, created when the 'Spell Control' FSM did not exist - Use the 'OnSpellControlLoad' event hook.");
            return;
        }

        string finalName = spell.GetFinalState();
        FsmState finalState = spellControl.GetState(finalName);
        if (finalState == null)
        {
            LogError($"Spell {name} not added, final state not found.");
            return;
        }

        string startingName = spell.GetStartingState();
        FsmState startingState = spellControl.GetState(startingName);
        if (startingState == null)
        {
            LogError($"Spell {name} not added, starting state not found.");
            return;
        }

        FsmState MPCostState = spell.GetMPCostState();
        int MPCost = spell.GetMPCost();

        if (MPCostState == null)
        {
            MPCostState = startingState;
        }

        if (MPCost == -1)
        {
            AddMPCostMethod(MPCostState);
        }

        finalState.AddTransition(FsmEvent.Finished, "Spell End");
    }

    private static void AddMPCostMethod(FsmState MPCostState)
    {
        PlayMakerFSM spellControl = HeroController.instance.spellControl;
        if (spellControl == null) { return; }

        MPCostState.InsertMethod(0, () => {
            FsmInt MPCostInt = spellControl.FsmVariables.GetFsmInt("MP Cost");
            int MPVal = MPCostInt.Value;
            HeroController.instance.TakeMP(MPVal);
        });
    }

    internal static List<CustomSpell> GetCustomSpells()
    {
        return _customSpells;
    }

    /// <summary>
    /// Gets all registered spells of a type. Default types 'Scream', 'Fireball', and 'Quake'.
    /// </summary>
    internal static List<CustomSpell> GetSpellsOfType(string type)
    {
        List<CustomSpell> spellsofType = new List<CustomSpell>();
        foreach (CustomSpell spell in _customSpells)
        {
            if (spell.GetSpellType() != type) { continue; }
            if (spell.GetUnlocked() == false) { continue; }
            spellsofType.Add(spell);
        }

        return spellsofType;
    }

    internal static void EquipCustomSpell(CustomSpell spell)
    {
        CustomSpell checkSpell = _customSpells.Find((CustomSpell check) => check == spell);
        if (checkSpell == null) { return; }

        string spellType = spell.GetSpellType();
        string state = "Has " + spellType + "?";

        PlayMakerFSM spellcontrol = HeroController.instance.spellControl;
        FsmState activeState = spellcontrol.GetState(state);
        if (activeState == null) { return; }

        activeState.ChangeTransition("CAST", spell.GetStartingState());
        equippedSpells[spellType] = spell;
    }

    internal static void EquipBaseSpell(string spellType)
    {
        string state = "Has " + spellType + "?";

        PlayMakerFSM spellcontrol = HeroController.instance.spellControl;
        FsmState activeState = spellcontrol.GetState(state);
        if (activeState == null) { return; }

        activeState.ChangeTransition("CAST", baseSpellStates[spellType]);
        equippedSpells[spellType] = null;
    }

    /// <summary>
    /// Adds all stored spells into 'Spell Control' FSM
    /// </summary>
    private static void LoadSpells(PlayMakerFSM spellControl)
    {
        foreach (CustomSpell spell in _customSpells)
        {
            SetupSpell(spellControl, spell);
        }
    }

    public static void SetSpellUnlocked(CustomSpell spell, bool unlocked)
    {
        CustomSpell checkSpell = _customSpells.Find((CustomSpell check) => check == spell);
        if (checkSpell == null) { LogError("Spell " + spell.GetName() + " not found"); return; }

        checkSpell.SetUnlocked(unlocked);
    }

    /// <summary>
    /// Used for static initialization.
    /// </summary>
    internal static void staticInit()
    {
        //I don't like doing this, but it's the best I've got.
        Modding.Logger.LogDebug("Initialising [SpellChanger]:[SpellHelper]");
    }

    private static void OnPlayMakerFsmAwake(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM self)
    {
        orig(self);

        if (self.gameObject.name != "Knight" || self.FsmName != "Spell Control") { return; }

        try
        {
            Events.SpellControlLoaded();
        }
        catch (Exception e)
        {
            LogError($"Error in OnSpellControlLoad event:\n{e}");
            throw;
        }

        try
        {
            LoadSpells(self);
        }
        catch (Exception e)
        {
            LogError($"Error in LoadSpells function:\n{e}");
            throw;
        }
    }

    /// <summary>
    /// Adds a spell to the private store. MUST BE USED WITHIN 'OnSpellControlLoad' EVENT HOOK.
    /// </summary>
    public static void AddSpell(CustomSpell customSpell, bool unlocked)
    {
        LogDebug($"Adding spell '{customSpell.GetName()}'");
        customSpell.SetUnlocked(unlocked);
        _customSpells.Add(customSpell);
    }

    private static void LogDebug(string message) => Modding.Logger.LogDebug(message + "[SpellChanger]:[SpellHelper]");
    private static void LogError(string message) => Modding.Logger.LogError(message + "[SpellChanger]:[SpellHelper]");
}
