namespace SpellChanger.AbilityClasses;

public class CustomDashSlash : CustomSpell
{
    public override string spellType => AbilityNames.DASHSLASH;
    public override string storedFSMName => AbilityFSMs.NAILARTS;
    public CustomDashSlash(string name, string nameKey, string descKey, Sprite[] sprites) : base(name, nameKey, descKey, sprites)
    {
        mpCost = 0;
    }

}
