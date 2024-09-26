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

        public static void SpellControlLoaded()
        {
            SpellHelper.ClearSpellList();
            OnSpellControlLoad?.Invoke();
        }
    }
}
