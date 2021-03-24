namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using ICities;
    using KianCommons;
    using System;
    using System.Collections.Generic;

    public static class Settings {
        public const string FileName = nameof(UnifiedUI);
        static Settings() {
            // Creating setting file - from SamsamTS
            if(GameSettings.FindSettingsFileByName(FileName) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FileName } });
            }
        }

        public static SavedBool HideOriginalButtons { get; } = new SavedBool("HideOriginalButtons", FileName, true, true);
        public static SavedBool HandleESC { get; } = new SavedBool("HandleESC", FileName, true, true);
        public static event Action RefreshButtons;
        public static void DoRefreshButtons() => RefreshButtons?.Invoke();


        public static void Collisions(UIHelper helper) {
            var mainPanel = MainPanel.Instance;
            if(mainPanel != null) {

                var keys = new List<SavedInputKey>();
                foreach(var b in mainPanel.ModButtons) {
                    if(b.ActivationKey != null)
                        keys.Add(b.ActivationKey);
                }
                keys.AddRange(mainPanel.CustomHotkeys.Keys);

                foreach(var key1 in keys) {
                    foreach(var key2 in keys) {
                        if(key1 != key2 && key1.value == key2.value) {
                            var file1 = (string)ReflectionHelpers.GetFieldValue(key1, "m_FileName");
                            var file2 = (string)ReflectionHelpers.GetFieldValue(key2, "m_FileName");
                            Log.Warning($"Collision Detected: " +
                                $"{file1}.{key1.name}:'{key1}' collides with " +
                                $"{file2}.{key2.name}:'{key2}'");
                        }
                    }
                }
            }
        }

        public static void OnSettingsUI(UIHelper helper) {
            try {
                var hideCheckBox = helper.AddCheckbox(
                    "Hide original activation buttons",
                    HideOriginalButtons,
                    val => {
                        HideOriginalButtons.value = val;
                        Log.Info("HideOriginalButtons set to " + val);
                        RefreshButtons?.Invoke();
                    }) as UICheckBox;
                //hideCheckBox.tooltip = "might need game restart";

                var showCheckBox2 = helper.AddCheckbox(
                    "Handle ESC key (esc key exits current tool if any).",
                    HandleESC,
                    val => {
                        HandleESC.value = val;
                        Log.Info("HandleESC set to " + val);
                    }) as UICheckBox;

                //var keymappings = panel.gameObject.AddComponent<KeymappingsPanel>();
                //keymappings.AddKeymapping("Activation Shortcut", NodeControllerTool.ActivationShortcut);

                (helper.self as UIComponent).eventVisibilityChanged += (_,__) => Collisions(helper);
                Collisions(helper);
            } catch(Exception e) {
                Log.Exception(e);
            }
        }
    }
}
