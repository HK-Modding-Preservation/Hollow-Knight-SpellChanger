namespace SpellChanger.AbilityClasses;

public class CustomGreatSlash : CustomSpell
{
    public override string spellType => AbilityNames.GREATSLASH;
    public override string storedFSMName => AbilityFSMs.NAILARTS;
    public CustomGreatSlash(string name, string nameKey, string descKey, Sprite[] sprites) : base(name, nameKey, descKey, sprites)
    {
        mpCost = 0;
    }
}
