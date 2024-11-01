namespace SpellChanger;

public class SpellChangerMod : Mod
{
    internal static SpellChangerMod Instance;

    public SpellChangerMod() : base("SpellChanger")
    {
        Instance = this;
    }

    public override string GetVersion() => "1.0.0.2";

    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        SpellHelper.staticInit();
        InventoryPatcher.staticInit();
    }

}