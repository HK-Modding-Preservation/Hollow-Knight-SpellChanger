namespace SpellChanger
{
    /// <summary>
    /// SpellChanger events.
    /// </summary>
    public static class Events
    {
        /// <summary>
        /// Called after the 'Spell Control' FSM loads.
        /// </summary>
        public static event Action? OnSpellControlLoad;

        /// <summary>
        /// Called after the 'Nail Art' FSM loads.
        /// </summary>
        public static event Action? OnNailArtControlLoad;

        public static void SpellControlLoaded()
        {
            SpellHelper.ClearSpellList();
            OnSpellControlLoad?.Invoke();
        }

        public static void NailArtsLoaded()
        {
            SpellHelper.ClearSpellListNA();
            OnNailArtControlLoad?.Invoke();
        }
    }
}
