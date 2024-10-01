namespace SpellChanger;

internal static class AbilityNames
{
    public static string DASHSLASH { get; } = "Dash Slash";
    public static string CYCLONESLASH { get; } = "Cyclone Slash";
    public static string GREATSLASH { get; } = "Great Slash";
    public static string FIREBALL { get; } = "Fireball";
    public static string QUAKE { get; } = "Quake";
    public static string SCREAM { get; } = "Scream";

    public static string[] NAILARTSNAMES { get; } = { DASHSLASH, CYCLONESLASH, GREATSLASH };
    public static string[] SPELLCONTROLNAMES { get; } = { FIREBALL, QUAKE, SCREAM };
    public static string[] ALLNAMES { get; } = { DASHSLASH, CYCLONESLASH, GREATSLASH, FIREBALL, QUAKE, SCREAM };
}

internal static class AbilityFSMs
{
    public static string NAILARTS { get; } = "Nail Arts";
    public static string SPELLCONTROL { get; } = "Spell Control";

    public static string[] names = new string[] { NAILARTS, SPELLCONTROL };

    //constants for the wackjob mystery bag of strings you need
    public static Dictionary<string, string> baseSpellStates = new Dictionary<string, string>()
    {
        { AbilityNames.SCREAM, "Scream Get?" },
        { AbilityNames.FIREBALL, "Wallside?" },
        { AbilityNames.QUAKE, "On Ground?" },
        { AbilityNames.DASHSLASH, "DSlash Start" },
        { AbilityNames.CYCLONESLASH, "Flash" },
        { AbilityNames.GREATSLASH, "Flash 2" }
    };
    public static Dictionary<string, string> baseSpellEvents = new Dictionary<string, string>()
    {
        { AbilityNames.SCREAM, "CAST" },
        { AbilityNames.FIREBALL, "CAST" },
        { AbilityNames.QUAKE, "CAST" },
        { AbilityNames.DASHSLASH, "DASH END" },
        { AbilityNames.CYCLONESLASH, "FINISHED" },
        { AbilityNames.GREATSLASH, "FINISHED" }
    };
    public static Dictionary<string, string> baseHasStateNames = new Dictionary<string, string>()
    {
        { AbilityNames.SCREAM, "Scream" },
        { AbilityNames.FIREBALL, "Fireball" },
        { AbilityNames.QUAKE, "Quake" },
        { AbilityNames.DASHSLASH, "Dash" },
        { AbilityNames.CYCLONESLASH, "Cyclone" },
        { AbilityNames.GREATSLASH, "G Slash" }
    };

    public static Dictionary<string, string> baseNAAlias = new Dictionary<string, string>()
    {
        { AbilityNames.DASHSLASH, "UPPER" },
        { AbilityNames.CYCLONESLASH, "CYCLONE" },
        { AbilityNames.GREATSLASH, "DASH" }
    };
    public static Dictionary<string, string> NAIconAlias = new Dictionary<string, string>()
    {
        { AbilityNames.DASHSLASH, "Uppercut" },
        { AbilityNames.CYCLONESLASH, "Cyclone" },
        { AbilityNames.GREATSLASH, "Dash" }
    };
}
