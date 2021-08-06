namespace UnifiedUI.API {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using UnifiedUI.GUI;
    using UnityEngine;

    public static class UUIAPI {
        #region tool

        // register tool button.
        public static UIComponent Register
            (string name, string groupName, string tooltip, string spritefile, ToolBase tool,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys) {
            Log.Called(name, groupName, tooltip, spritefile, tool, activationKey, activeKeys);

            var ret = MainPanel.Instance.Register(name: name, groupName: groupName, tooltip: tooltip, spritefile: spritefile);
            ret.ActivationKey = activationKey;
            ret.ActiveKeys = activeKeys;
            ret.Tool = tool;
            return ret;

        }

        // register tool button.
        public static UIComponent Register
            (string name, string groupName, string tooltip, ToolBase tool,
            UITextureAtlas atlas, string[] spriteNames,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys) {
            Log.Called(name, groupName, tooltip, atlas, spriteNames, tool, activationKey, activeKeys);
            var ret = MainPanel.Instance.Register(name: name, groupName: groupName, tooltip: tooltip);

            Assertion.NotNull(atlas, "atlas");
            Assertion.NotNull(spriteNames, "spriteNames");
            Assertion.Equal(spriteNames.Length, 4, "spriteNames");
            ret.atlas = atlas;
            ret.BGNormal = spriteNames[0] ?? "";
            ret.BGHovered = spriteNames[1] ?? "";
            ret.BGPressed = spriteNames[2] ?? "";
            ret.BGDisabled = spriteNames[3] ?? "";

            ret.ActivationKey = activationKey;
            ret.ActiveKeys = activeKeys;
            ret.Tool = tool;
            return ret; 
        }

        public static UIComponent Register
            (string name, string groupName, string tooltip, ToolBase tool,
            Texture2D texture, SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys) {
            Log.Called(name, groupName, tooltip, texture, tool, activationKey, activeKeys);
            var ret = MainPanel.Instance.Register(name: name, groupName: groupName, tooltip: tooltip);

            Assertion.Assert(texture, "texture");
            texture.name = $"g:{groupName}, name:{name} tool:{tool} V:{tool.VersionOf()}";
            ret.SetIcon(texture);

            ret.ActivationKey = activationKey;
            ret.ActiveKeys = activeKeys;
            ret.Tool = tool;
            return ret;
        }

        #endregion

        #region register custom button
        // Register custom button
        public static UIComponent Register
            (string name, string groupName, string tooltip,
            UITextureAtlas atlas, string[] spriteNames,
            Action<bool> onToggle, Action<ToolBase> onToolChanged,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys) {
            Log.Called(name, groupName, tooltip, atlas, spriteNames, onToggle, onToolChanged, activationKey, activeKeys);
            var ret = MainPanel.Instance.Register(name: name, groupName: groupName, tooltip: tooltip);

            Assertion.NotNull(atlas, "atlas");
            Assertion.NotNull(spriteNames, "spriteNames");
            Assertion.Equal(spriteNames.Length, 4, "spriteNames");
            ret.atlas = atlas;
            ret.BGNormal = spriteNames[0] ?? "";
            ret.BGHovered = spriteNames[1] ?? "";
            ret.BGPressed = spriteNames[2] ?? "";
            ret.BGDisabled = spriteNames[3] ?? "";

            ret.ActivationKey = activationKey;
            ret.ActiveKeys = activeKeys;

            ret.OnToggleCallBack = onToggle;
            ret.OnToolChangedCallBack = onToolChanged;
            return ret;
        }


        /// <summary>
        /// Register custom button
        /// </summary>
        public static UIComponent Register
            (string name, string groupName, string tooltip, string spritefile,
            Action<bool> onToggle, Action<ToolBase> onToolChanged,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys) {
            Log.Called(name, groupName, tooltip, spritefile, onToggle, onToolChanged, activationKey, activeKeys);
            var ret = MainPanel.Instance.Register(name: name, groupName: groupName, tooltip: tooltip, spritefile: spritefile);
            ret.ActivationKey = activationKey;
            ret.ActiveKeys = activeKeys;
            ret.OnToggleCallBack = onToggle;
            ret.OnToolChangedCallBack = onToolChanged;
            return ret;
        }

        /// <summary>
        /// Register custom button
        /// </summary>
        public static UIComponent Register
            (string name, string groupName, string tooltip, Texture2D texture,
            Action<bool> onToggle, Action<ToolBase> onToolChanged,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys) {
            Log.Called(name, groupName, tooltip, texture, onToggle, onToolChanged, activationKey, activeKeys);
            var ret = MainPanel.Instance.Register(name: name, groupName: groupName, tooltip: tooltip);

            Assertion.Assert(texture, "texture");
            texture.name = $"g:{groupName}, name:{name} textureName:{texture.name}";
            ret.SetIcon(texture);

            ret.ActivationKey = activationKey;
            ret.ActiveKeys = activeKeys;
            ret.OnToggleCallBack = onToggle;
            ret.OnToolChangedCallBack = onToolChanged;
            return ret;
        }
        #endregion


        // register hotkeys
        public static void Register(
            Action onToggle,
            SavedInputKey activationKey,
            Dictionary<SavedInputKey, Func<bool>> activeKeys) {
            Log.Called(onToggle, activationKey, activeKeys);
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

        public static void AttachAlien(UIComponent alien, string groupName) {
            Log.Called(alien, groupName);
            MainPanel.Instance.AttachAlien(alien, groupName);
        }
    }
}
