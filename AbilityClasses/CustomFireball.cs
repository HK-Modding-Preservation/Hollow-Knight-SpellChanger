namespace SpellChanger.AbilityClasses;

public class CustomFireball : CustomSpell
{
    public override string spellType => AbilityNames.FIREBALL;
    public override string storedFSMName => AbilityFSMs.SPELLCONTROL;
    public CustomFireball(string name, string nameKey, string descKey, Sprite[] sprites) : base(name, nameKey, descKey, sprites)
    {
    }

}
