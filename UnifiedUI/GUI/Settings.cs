namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using ICities;
    using KianCommons;
    using System;

    public static class Settings {
        public const string FileName = nameof(UnifiedUI);
        static Settings() {
            // Creating setting file - from SamsamTS
            if (GameSettings.FindSettingsFileByName(FileName) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FileName } });
            }
        }

        public static SavedBool HideOriginalButtons { get; } = new SavedBool(nameof(HideOriginalButtons), FileName, true, true);
        public static SavedBool HandleESC { get; } = new SavedBool(nameof(HandleESC), FileName, true, true);

        public static void OnSettingsUI(UIHelperBase helper) {
            try {
                var showCheckBox = helper.AddCheckbox(
                    "Hide original activation buttons",
                    HideOriginalButtons,
                    val => {
                        HideOriginalButtons.value = val;
                        // TODO refresh buttons.
                    }
                    ) as UICheckBox;

                var showCheckBox2 = helper.AddCheckbox(
                    "Handle ESC key (esc key exists current tool if any).",
                    HandleESC,
                    val => {
                        HandleESC.value = val;
                    }
                    ) as UICheckBox;
                //var keymappings = panel.gameObject.AddComponent<KeymappingsPanel>();
                //keymappings.AddKeymapping("Activation Shortcut", NodeControllerTool.ActivationShortcut);
            }
            catch (Exception e) {
                Log.Exception(e);
            }
        }
    }
}
