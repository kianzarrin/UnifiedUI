namespace UnitedUI.GUI {
    using ColossalFramework.UI;
    using ICities;
    using ColossalFramework;
    using System;
    using KianCommons;

    public static class Settings {
        public const string FileName = nameof(UnitedUI);
        static Settings() {
            // Creating setting file - from SamsamTS
            if (GameSettings.FindSettingsFileByName(FileName) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FileName } });
            }
        }

        public static SavedBool HideOriginalButtons { get; } = new SavedBool(nameof(HideOriginalButtons), FileName, true, true);

        public static void OnSettingsUI(UIHelperBase helper) {
            try {
                var showCheckBox = helper.AddCheckbox(
                    "Hide original activation buttons",
                    HideOriginalButtons,
                    val => HideOriginalButtons.value = val
                    ) as UICheckBox;


                //var keymappings = panel.gameObject.AddComponent<KeymappingsPanel>();
                //keymappings.AddKeymapping("Activation Shortcut", NodeControllerTool.ActivationShortcut);
            }
            catch (Exception e){
                Log.Exception(e);
            }
        }
    }
}
