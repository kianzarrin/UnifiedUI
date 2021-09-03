namespace UnifiedUI.Helpers {
    using System;
    using System.Collections.Generic;
    using ColossalFramework;
    public class UUIHotKeys {
        public SavedInputKey ActivationKey;
        public Dictionary<SavedInputKey, Func<bool>> InToolKeys;

        /// <summary>
        /// add in-mod hotkey. this will take priority over (suppress) the activation key of other mods.
        /// when your mod is active.
        /// </summary>
        public void AddInToolKey(SavedInputKey hotkey) => AddInToolKey(hotkey, null);

        /// <summary>
        /// add in-mod hotkey. this will take priority over (suppress) the activation key of other mods.
        /// when your mod is active and the hotkey is availble.
        /// </summary>
        /// <param name="isAvailble">a function that returns true when the in-mod hotkey is availble
        /// (for example when the mod has sub-tools)
        /// in other wors, if isAvailbe returns true, then activation key of other mods are suppressed.
        /// </param>
        public void AddInToolKey(SavedInputKey hotkey, Func<bool> isAvailble) {
            InToolKeys ??= new Dictionary<SavedInputKey, Func<bool>>();
            InToolKeys[hotkey] = isAvailble;
        }
    }
}
