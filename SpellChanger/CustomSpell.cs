using SpellChanger.Utils;
using Vasi;
using static UnityEngine.UI.Selectable;

namespace SpellChanger;
/// <summary>
/// Class used to create a CustomSpell. Register using SpellHelper.AddSpell(CustomSpell)
/// </summary>
public class CustomSpell
{
    /// <summary>
    /// The name of the spell.
    /// </summary>
    private string name;
    /// <summary>
    /// The id of the spell. Unique identifier added to state names.
    /// </summary>
    private int id;
    /// <summary>
    /// The text language key of the spell's name.
    /// </summary>
    private string nameKey;
    /// <summary>
    /// The sprite used for the spell icon at level 1.
    /// </summary>
    private Sprite spriteLevel1;
    /// <summary>
    /// The sprite used for the spell icon at level 2.
    /// </summary>
    private Sprite spriteLevel2;
    /// <summary>
    /// The text language key for the spell's description.
    /// </summary>
    private string descKey;
    /// <summary>
    /// The name of the starting state. Defaults to first added state.
    /// </summary>
    private string startingState;
    /// <summary>
    /// The name of the final state. Defaults to last added state.
    /// </summary>
    private string finalState;
    /// <summary>
    /// The FSM that will store the states given. 'SpellControl', or when created incorrectly a blank FSM
    /// </summary>
    private PlayMakerFSM spellControlFSM;
    /// <summary>
    /// A list of the spell's state names.
    /// </summary>
    private List<FsmState> fsmStates;
    /// <summary>
    /// The type of spell it is. (Fireball, Scream, Quake)
    /// </summary>
    private string spellType;
    /// <summary>
    /// MP Cost of the spell. If -1, use base game values.
    /// </summary>
    private int mpCost;
    /// <summary>
    /// The state in which MP is drained. If null, use starting state of the spell.
    /// </summary>
    private FsmState mpCostState;
    /// <summary>
    /// Whether or not the spell has been unlocked.
    /// </summary>
    private bool unlocked;

    /// <summary>
    /// Constructor method for CustomSpell. Create during the 'OnSpellControlLoad' event hook, or it will not be able to be added to the moveset.
    /// </summary>
    public CustomSpell(string name, string nameKey, string descKey, Sprite spriteLevel1, Sprite spriteLevel2)
    {
        this.name = name;
        this.nameKey = nameKey;
        this.spriteLevel1 = spriteLevel1;
        this.spriteLevel2 = spriteLevel2;
        this.descKey = descKey;
        id = SpellHelper.GenerateId();
        mpCost = -1;
        unlocked = true;

        fsmStates = new List<FsmState>();

        spellControlFSM = HeroController.instance.spellControl;
        if (HeroController.instance.spellControl == null)
        {
            Modding.Logger.LogError($"CustomSpell {name} created outside of 'OnSpellControlLoad' hook. May not be registered correctly.");
            spellControlFSM = SpellHelper.SpellFSMStoreObject.AddComponent<PlayMakerFSM>();
        }
    }

    /// <summary>
    /// Create a state for the spell.
    /// </summary>
    public FsmState CreateState(string stateName)
    {
        FsmState state = spellControlFSM.CreateState(stateName+id);
        fsmStates.Add(state);
        return state;
    }

    /// <summary>
    /// Create a copy of another state for the spell. The copy should already exist inside 'Spell Control', and must be a vanilla state. Transitions are not copied.
    /// </summary>
    public FsmState CopyStateActions(string copyState, string newStateName)
    {
        FsmState state = CreateState(newStateName); //dont add id here, its added in CreateState
        FsmState stateToCopy = spellControlFSM.GetState(copyState);

        if (stateToCopy == null) { Modding.Logger.Log($"Attempted to copy a non existent state '{copyState}'"); return state; }
        //copy actions
        state.CopyActionData(stateToCopy);

        return state;
    }

    /// <summary>
    /// Add a transition between two states that have been added.
    /// </summary>
    public void AddTransition(string eventName, string to, string from)
    {
        FsmState toState = fsmStates.Find((FsmState t) => t.Name == to+id);
        FsmState fromState = fsmStates.Find((FsmState t) => t.Name == from+id);

        if (toState == null || fromState == null)
        {
            Modding.Logger.LogError("Error finding one or more states while adding transition.");
            return;
        }

        FsmEvent @event = spellControlFSM.GetFsmEvent(eventName);
        if (@event == null)
        {
            @event = spellControlFSM.CreateFsmEvent(eventName);
        }

        fromState.AddTransition(@event, toState);
    }
    /// <summary>
    /// Changes the state that a transition will lead into.
    /// </summary>
    public void ChangeTransition(string eventName, string to, string from)
    {
        FsmState toState = fsmStates.Find((FsmState t) => t.Name == to + id);
        FsmState fromState = fsmStates.Find((FsmState t) => t.Name == from + id);

        if (toState == null || fromState == null)
        {
            Modding.Logger.LogError("Error finding one or more states while changing transition.");
            return;
        }

        FsmEvent @event = spellControlFSM.GetFsmEvent(eventName);
        if (@event == null)
        {
            @event = spellControlFSM.CreateFsmEvent(eventName);
        }

        fromState.ChangeTransition(eventName, to + id);
    }

    /// <summary>
    /// Sets the starting state of the spell.
    /// </summary>
    public void SetStartingState(string stateName)
    {
        startingState = stateName+id;
    }

    /// <summary>
    /// Sets the final state of the spell.
    /// </summary>
    public void SetFinalState(string stateName)
    {
        finalState = stateName+id;
    }
    /// <summary>
    /// Sets the MP cost of the spell. Defaults to base game values. If you change this, add your own logic for taking MP from the player.
    /// </summary>
    public void SetMPCost(int MPCost)
    {
        mpCost = MPCost;
    }
    /// <summary>
    /// Sets whether or not the spell has been unlocked for use. Defaults to unlocked. If locked, add your own logic for unlocking it.
    /// </summary>
    internal void SetUnlocked(bool Unlocked)
    {
        unlocked = Unlocked;
    }
    /// <summary>
    /// Sets the state in which MP is drained. Defaults to the starting state of the spell. To disable, call CustomSpell.SetMPCost(0), then add your own logic.
    /// </summary>
    public void SetMPCostState(FsmState MPCostState)
    {
        mpCostState = MPCostState;
    }
    /// <summary>
    /// Binds spell input type to Fireball.
    /// </summary>
    public void BindToFireball()
    {
        spellType = "Fireball";
    }
    /// <summary>
    /// Binds spell input type to Scream.
    /// </summary>
    public void BindToScream()
    {
        spellType = "Scream";
    }
    /// <summary>
    /// Binds spell input type to Quake.
    /// </summary>
    public void BindToQuake()
    {
        spellType = "Quake";
    }
    /// <summary>
    /// Returns spell type.
    /// </summary>
    public string GetSpellType()
    {
        return spellType;
    }
    /// <summary>
    /// Returns starting state. Defaults to first added state if not set.
    /// </summary>
    public string GetStartingState()
    {
        if (startingState == null)
        {
            return fsmStates[0].Name;
        }
        return startingState;
    }
    /// <summary>
    /// Returns final state. Defaults to last added state if not set.
    /// </summary>
    public string GetFinalState()
    {
        if (finalState == null)
        {
            return fsmStates[fsmStates.Count - 1].Name;
        }
        return finalState;
    }
    /// <summary>
    /// Returns spell name.
    /// </summary>
    public string GetName()
    {
        return name;
    }
    /// <summary>
    /// Returns text language key of the spell name.
    /// </summary>
    public string GetNameKey()
    {
        return nameKey;
    }
    /// <summary>
    /// Returns text language key of the spell description.
    /// </summary>
    public string GetDescKey()
    {
        return descKey;
    }
    /// <summary>
    /// Returns level 1 sprite icon.
    /// </summary>
    public Sprite GetSpriteLevel1()
    {
        return spriteLevel1;
    }
    /// <summary>
    /// Returns level 2 sprite icon
    /// </summary>
    public Sprite GetSpriteLevel2()
    {
        return spriteLevel2;
    }
    /// <summary>
    /// Returns MP Cost, or -1 if not set. If -1, use base game values.
    /// </summary>
    public int GetMPCost()
    {
        return mpCost;
    }
    /// <summary>
    /// Returns the state in which MPCost is drained, or null if not set. If null, defaults to starting state.
    /// </summary>
    public FsmState GetMPCostState()
    {
        return mpCostState;
    }
    /// <summary>
    /// Returns the FSM used to store the spell's states. Usually 'Spell Control', or a blank FSM when the class is incorrectly created.
    /// </summary>
    public PlayMakerFSM GetStoreFSM()
    {
        return spellControlFSM;
    }
    /// <summary>
    /// Returns whether or not the spell is unlocked.
    /// </summary>
    public bool GetUnlocked()
    {
        return unlocked;
    }

}
