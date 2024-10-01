using Vasi;

namespace SpellChanger;

public static class SpellHelper
{
    private static List<CustomSpell> _customSpells = new List<CustomSpell>();
    internal static GameObject SpellFSMStoreObject = new GameObject("SpellFsmStoreObject");
    internal static Dictionary<string, CustomSpell> equippedSpells = new Dictionary<string, CustomSpell>()
    {
        { AbilityNames.SCREAM, null },
        { AbilityNames.FIREBALL, null },
        { AbilityNames.QUAKE, null },
        { AbilityNames.DASHSLASH, null },
        { AbilityNames.CYCLONESLASH, null },
        { AbilityNames.GREATSLASH, null }
    };
    internal static Dictionary<string, string> baseSpellStates = new Dictionary<string, string>()
    {
        { AbilityNames.SCREAM, "Scream Get?" },
        { AbilityNames.FIREBALL, "Wallside?" },
        { AbilityNames.QUAKE, "On Ground?" },
        { AbilityNames.DASHSLASH, "DSlash Start" },
        { AbilityNames.CYCLONESLASH, "Flash" },
        { AbilityNames.GREATSLASH, "Flash 2" }
    };
    internal static Dictionary<string, string> baseSpellEvents = new Dictionary<string, string>()
    {
        { AbilityNames.SCREAM, "CAST" },
        { AbilityNames.FIREBALL, "CAST" },
        { AbilityNames.QUAKE, "CAST" },
        { AbilityNames.DASHSLASH, "DASH END" },
        { AbilityNames.CYCLONESLASH, "FINISHED" },
        { AbilityNames.GREATSLASH, "FINISHED" }
    };
    internal static Dictionary<string, string> baseHasStateNames = new Dictionary<string, string>()
    {
        { AbilityNames.SCREAM, "Scream" },
        { AbilityNames.FIREBALL, "Fireball" },
        { AbilityNames.QUAKE, "Quake" },
        { AbilityNames.DASHSLASH, "Dash" },
        { AbilityNames.CYCLONESLASH, "Cyclone" },
        { AbilityNames.GREATSLASH, "G Slash" }
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
        On.PlayMakerFSM.Awake += OnPlayMakerFsmAwakeNA;
    }

    internal static void ClearSpellList()
    {
        _customSpells.RemoveAll(x => AbilityNames.SPELLCONTROLNAMES.Contains(x.spellType));

        ForceEquipBaseSpell(AbilityNames.FIREBALL);
        ForceEquipBaseSpell(AbilityNames.QUAKE);
        ForceEquipBaseSpell(AbilityNames.SCREAM);
    }

    internal static void ClearSpellListNA()
    {
        _customSpells.RemoveAll(x => AbilityNames.NAILARTSNAMES.Contains(x.spellType));

        ForceEquipBaseSpell(AbilityNames.DASHSLASH);
        ForceEquipBaseSpell(AbilityNames.CYCLONESLASH);
        ForceEquipBaseSpell(AbilityNames.GREATSLASH);
    }

    /// <summary>
    /// Initialises CustomSpells into the game.
    /// </summary>
    private static void SetupSpell(CustomSpell spell)
    {
        PlayMakerFSM fsm = spell.storedFSM;
        string name = spell.name;
        if (fsm == null)
        {
            LogError($"Spell {name} not added, created when the 'Spell Control' FSM did not exist - Use the 'OnSpellControlLoad' or 'OnNailArtLoaded' event hook.");
            return;
        }

        string finalName = spell.GetFinalState();
        FsmState finalState = fsm.GetState(finalName);
        if (finalState == null)
        {
            LogError($"Spell {name} not added, final state not found.");
            return;
        }

        string startingName = spell.GetStartingState();
        FsmState startingState = fsm.GetState(startingName);
        if (startingState == null)
        {
            LogError($"Spell {name} not added, starting state not found.");
            return;
        }

        FsmState MPCostState = spell.mpCostState;
        int MPCost = spell.mpCost;

        if (MPCostState == null)
        {
            MPCostState = startingState;
        }

        if (MPCost == -1)
        {
            AddMPCostMethod(spell, MPCostState);
        }

        string finalstate = "Spell End";
        if (AbilityNames.NAILARTSNAMES.Contains(spell.spellType)) {
            finalstate = "Regain Control";
        }

        finalState.AddTransition(FsmEvent.Finished, finalstate);
    }

    private static void AddMPCostMethod(CustomSpell spell, FsmState MPCostState)
    {
        PlayMakerFSM spellControl = spell.storedFSM;
        if (spellControl == null) { return; }

        MPCostState.InsertMethod(0, () => {
            FsmInt MPCostInt = spellControl.FsmVariables.GetFsmInt("MP Cost");
            int MPVal = MPCostInt.Value;
            if (spell.mpCost != -1)
            {
                MPVal = spell.mpCost;
            }
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
            if (spell.spellType != type) { continue; }
            if (spell.unlocked == false) { continue; }
            spellsofType.Add(spell);
        }

        return spellsofType;
    }

    private static string getFSMName(string spellType)
    {
        string fsmName = "";

        if (AbilityNames.NAILARTSNAMES.Contains(spellType))
        {
            fsmName = AbilityFSMs.NAILARTS;
        }
        if (AbilityNames.SPELLCONTROLNAMES.Contains(spellType))
        {
            fsmName = AbilityFSMs.SPELLCONTROL;
        }

        if (fsmName == "") { LogError("Fsm Type not found when equipping spell of type " + spellType);}

        return fsmName;
    }

    public static void ForceEquipCustomSpell(CustomSpell spell)
    {
        CustomSpell checkSpell = _customSpells.Find((CustomSpell check) => check == spell);
        if (checkSpell == null) { return; }

        string spellType = spell.spellType;
        string eventAddon = baseHasStateNames[spellType];
        if (eventAddon == null) { LogError("'Has?' state equivalent not found for spell type " + spellType); return; }
        string state = "Has " + eventAddon + "?";

        if (spellType == AbilityNames.DASHSLASH) //cause team cherry hates consistency
        {
            state = "Dash Slash Ready";
        }

        string fsmName = getFSMName(spellType);
        if (fsmName == "") {LogError("Spell "+ spell.name + " not equipped."); return; }

        PlayMakerFSM fsm = HeroController.instance.gameObject.LocateMyFSM(fsmName);
        FsmState activeState = fsm.GetState(state);
        if (activeState == null) { return; }

        activeState.ChangeTransition(baseSpellEvents[spellType], spell.GetStartingState());
        equippedSpells[spellType] = spell;
    }

    public static void ForceEquipBaseSpell(string spellType)
    {
        string eventAddon = baseHasStateNames[spellType];
        if (eventAddon == null) { LogError("'Has?' state equivalent not found for spell type " + spellType); return; }
        string state = "Has " + eventAddon + "?";

        if (spellType == AbilityNames.DASHSLASH) //cause team cherry hates consistency
        {
            state = "Dash Slash Ready";
        }

        string fsmName = getFSMName(spellType);
        if (fsmName == "") { LogError("Base Spell " + spellType + " not equipped."); return; }

        PlayMakerFSM fsm = HeroController.instance.gameObject.LocateMyFSM(fsmName);
        FsmState activeState = fsm.GetState(state);
        if (activeState == null) { return; }

        activeState.ChangeTransition(baseSpellEvents[spellType], baseSpellStates[spellType]);
        equippedSpells[spellType] = null;
    }

    /// <summary>
    /// Adds all stored spells into 'Spell Control' FSM
    /// </summary>
    private static void LoadSpells()
    {
        foreach (CustomSpell spell in _customSpells)
        {
            if (!AbilityNames.SPELLCONTROLNAMES.Contains(spell.spellType)) { continue; }
            SetupSpell(spell);
        }
    }

    private static void LoadNA()
    {
        foreach (CustomSpell spell in _customSpells)
        {
            if (!AbilityNames.NAILARTSNAMES.Contains(spell.spellType)) { continue; }
            SetupSpell(spell);
        }
    }

    public static void SetSpellUnlocked(CustomSpell spell, bool unlocked)
    {
        CustomSpell checkSpell = _customSpells.Find((CustomSpell check) => check == spell);
        if (checkSpell == null) { LogError("Spell " + spell.name + " not found"); return; }

        checkSpell.unlocked = unlocked;
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
            LoadSpells();
        }
        catch (Exception e)
        {
            LogError($"Error in LoadSpells function:\n{e}");
            throw;
        }
    }

    private static void OnPlayMakerFsmAwakeNA(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM self)
    {
        orig(self);

        if (self.gameObject.name != "Knight" || self.FsmName != "Nail Arts") { return; }

        try
        {
            Events.NailArtsLoaded();
        }
        catch (Exception e)
        {
            LogError($"Error in OnNailArtsLoaded event:\n{e}");
            throw;
        }

        try
        {
            LoadNA();
        }
        catch (Exception e)
        {
            LogError($"Error in LoadSpells function:\n{e}");
            throw;
        }
    }

    /// <summary>
    /// Adds a spell to the private store. MUST BE USED WITHIN 'OnSpellControlLoad' OR 'OnNailArtLoaded' EVENT HOOK.
    /// </summary>
    public static void AddSpell(CustomSpell customSpell, bool unlocked)
    {
        LogDebug($"Adding spell '{customSpell.name}'");
        customSpell.unlocked = unlocked;
        _customSpells.Add(customSpell);
    }

    private static void LogDebug(string message) => Modding.Logger.LogDebug(message + "[SpellChanger]:[SpellHelper]");
    private static void LogError(string message) => Modding.Logger.LogError(message + "[SpellChanger]:[SpellHelper]");
}
