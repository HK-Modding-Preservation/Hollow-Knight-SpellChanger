namespace SpellChanger.AbilityClasses;

public class CustomQuake : CustomSpell
{
    public override string spellType => AbilityNames.QUAKE;
    public override string storedFSMName => AbilityFSMs.SPELLCONTROL;
    public CustomQuake(string name, string nameKey, string descKey, Sprite[] sprites) : base(name, nameKey, descKey, sprites)
    {
    }

}
