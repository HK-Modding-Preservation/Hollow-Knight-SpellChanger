using SpellChanger.Utils;
using Vasi;
using static UnityEngine.UI.Selectable;

namespace SpellChanger;
/// <summary>
/// Class used to create a CustomSpell. Register using SpellHelper.AddSpell(CustomSpell)
/// </summary>
public abstract class CustomSpell
{
    /// <summary>
    /// The name of the spell.
    /// </summary>
    public string name { get; }
    /// <summary>
    /// The id of the spell. Unique identifier added to state names.
    /// </summary>
    public int id { get; }
    /// <summary>
    /// The text language key of the spell's name.
    /// </summary>
    public string nameKey { get; }
    /// <summary>
    /// The sprites used in the spell icons.
    /// </summary>
    public Sprite[] sprites { get; }
    /// <summary>
    /// The text language key for the spell's description.
    /// </summary>
    public string descKey { get; }
    /// <summary>
    /// The name of the starting state. Defaults to first added state.
    /// </summary>
    private string startingState;
    /// <summary>
    /// The name of the final state. Defaults to last added state.
    /// </summary>
    private string finalState;
    /// <summary>
    /// The name of the FSM that will store given states.
    /// </summary>
    public abstract string storedFSMName { get; }
    /// <summary>
    /// The FSM that will store the states given.
    /// </summary>
    public PlayMakerFSM storedFSM { get; }
    /// <summary>
    /// A list of the spell's state names.
    /// </summary>
    public List<FsmState> fsmStates { get; }
    /// <summary>
    /// The type of spell it is. (Fireball, Scream, Quake)
    /// </summary>
    public abstract string spellType { get; }
    /// <summary>
    /// MP Cost of the spell. If -1, use base game values.
    /// </summary>
    public int mpCost { get; set; }
    /// <summary>
    /// The state in which MP is drained. If null, use starting state of the spell.
    /// </summary>
    public FsmState mpCostState { get; set; }
    /// <summary>
    /// Whether or not the spell has been unlocked.
    /// </summary>
    internal bool unlocked { get; set; }

    /// <summary>
    /// Constructor method for CustomSpell. Create during one of the event hooks added with the mod.
    /// </summary>
    public CustomSpell(string name, string nameKey, string descKey, Sprite[] sprites)
    {
        this.name = name;
        this.nameKey = nameKey;
        this.sprites = sprites;
        this.descKey = descKey;
        id = SpellHelper.GenerateId();
        mpCost = -1;
        unlocked = true;

        fsmStates = new List<FsmState>();

        storedFSM = HeroController.instance.gameObject.LocateMyFSM(storedFSMName);
        if (storedFSM == null)
        {
            Modding.Logger.LogError($"CustomSpell {name} created outside of 'OnSpellControlLoad' hook. May not be registered correctly.");
            storedFSM = SpellHelper.SpellFSMStoreObject.AddComponent<PlayMakerFSM>();
        }
    }

    /// <summary>
    /// Create a state for the spell.
    /// </summary>
    public FsmState CreateState(string stateName)
    {
        FsmState state = storedFSM.CreateState(stateName+id);
        fsmStates.Add(state);
        return state;
    }

    /// <summary>
    /// Create a copy of another state for the spell. The copy should already exist inside 'Spell Control', and must be a vanilla state. Transitions are not copied.
    /// </summary>
    public FsmState CopyStateActions(string copyState, string newStateName)
    {
        FsmState state = CreateState(newStateName); //dont add id here, its added in CreateState
        FsmState stateToCopy = storedFSM.GetState(copyState);

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

        FsmEvent @event = storedFSM.GetFsmEvent(eventName);
        if (@event == null)
        {
            @event = storedFSM.CreateFsmEvent(eventName);
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

        FsmEvent @event = storedFSM.GetFsmEvent(eventName);
        if (@event == null)
        {
            @event = storedFSM.CreateFsmEvent(eventName);
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

}
