namespace UnifiedUI.Helpers {
    using ColossalFramework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public abstract class UnsavedInputKey : SavedInputKey {
        public UnsavedInputKey(string keyName, string modName, InputKey key) : base(keyName, modName, key, false) {
            this.m_Synced = true; // no need to sync with file.
        }

        /// <summary>
        /// Override this to serialize hotkey when user changes hotkey inside UUI.
        /// </summary>
        public abstract void OnConflictResolved();
    }
}
