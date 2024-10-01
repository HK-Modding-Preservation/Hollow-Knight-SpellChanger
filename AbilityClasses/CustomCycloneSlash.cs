namespace SpellChanger.AbilityClasses;

public class CustomCycloneSlash : CustomSpell
{

    public override string spellType => AbilityNames.CYCLONESLASH;
    public override string storedFSMName => AbilityFSMs.NAILARTS;
    public CustomCycloneSlash(string name, string nameKey, string descKey, Sprite[] sprites) : base(name, nameKey, descKey, sprites)
    {
        mpCost = 0;
    }

}
