namespace SpellChanger.AbilityClasses;

public class CustomScream : CustomSpell
{
    public override string spellType => AbilityNames.SCREAM;
    public override string storedFSMName => AbilityFSMs.SPELLCONTROL;
    public CustomScream(string name, string nameKey, string descKey, Sprite[] sprites) : base(name, nameKey, descKey, sprites)
    {
    }

}
