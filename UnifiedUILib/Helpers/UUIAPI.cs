namespace UnifiedUI.API {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using UnifiedUI.GUI;

    public static class UUIAPI {
        /// <summary>
        /// Register custom button
        /// </summary>
        public static UIComponent Register
            (string name, string groupName, string tooltip, string spritefile,
            Action<bool> onToggle, Action<ToolBase> onToolChanged,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys) {
            var ret = MainPanel.Instance.Register(name: name, groupName: groupName, tooltip: tooltip, spritefile: spritefile);
            ret.ActivationKey = activationKey;
            ret.ActiveKeys = activeKeys;
            ret.OnToggleCallBack = onToggle;
            ret.OnToolChangedCallBack = onToolChanged;
            return ret;
        }


        // register tool button.
        public static UIComponent Register
            (string name, string groupName, string tooltip, string spritefile, ToolBase tool,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys) {
            var ret = MainPanel.Instance.Register(name: name, groupName: groupName, tooltip: tooltip, spritefile: spritefile);
            ret.ActivationKey = activationKey;
            ret.ActiveKeys = activeKeys;
            ret.Tool = tool;
            return ret;

        }

        // register hotkeys
        public static void Register(
            Action onToggle,
            SavedInputKey activationKey,
            Dictionary<SavedInputKey, Func<bool>> activeKeys) {
            try {
                if(activationKey != null && onToggle != null)
                    MainPanel.Instance.CustomHotkeys[activationKey] = onToggle;

                if(activeKeys != null) {
                    foreach(var pair in activeKeys) {
                        Assertion.AssertNotNull(pair.Key, "hotkey cannot be null in 'activeKeys'");
                        Assertion.AssertNotNull(pair.Value, "IsActive cannot be null in 'activeKeys'");
                        MainPanel.Instance.CustomActiveHotkeys[pair.Key] = pair.Value;
                    }
                }
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}
