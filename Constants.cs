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

}
