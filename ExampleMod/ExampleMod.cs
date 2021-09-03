namespace ExampleMod {
    using ColossalFramework;
    using ColossalFramework.UI;
    using ICities;
    using System;
    using UnifiedUI.Helpers;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class UserModExtension : LoadingExtensionBase, IUserMod {
        internal static bool InGameOrEditor =>
            SceneManager.GetActiveScene().name != "IntroScreen" &&
            SceneManager.GetActiveScene().name != "Startup";

        public string Name => "Example Mod";
        public string Description => "Show case for using UnifiedUI helpers";

        public void OnEnabled() {
            if (InGameOrEditor) // hot reload
                Init();
        }

        public void OnDisabled() {
            Release();
        }

        public override void OnLevelLoaded(LoadMode mode) => Init();

        public override void OnLevelUnloading() => Release();

        internal static void Init() {
            Debug.Log("[UUIExampleMod] LifeCycle.Init()");
            ToolsModifierControl.toolController.gameObject.AddComponent<ExampleTool>();
        }
        internal static void Release() =>
            ToolsModifierControl.toolController.GetComponent<ExampleTool>()?.Destroy();

        public void OnSettingsUI(UIHelper helper) => ModSettings.OnSettingsUI(helper);
    }

    public static class ModSettings {
        const string SETTINGS_FILE_NAME = "UUIExampleMod";

        static ModSettings() {
            if (GameSettings.FindSettingsFileByName(SETTINGS_FILE_NAME) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = SETTINGS_FILE_NAME } });
            }
        }

        public static SavedInputKey Hotkey = new SavedInputKey(
            "UUIExampleMod_HotKey", SETTINGS_FILE_NAME,
            key: KeyCode.A, control: true, shift: true, alt: false, true);


        public static void OnSettingsUI(UIHelper helper) {
            try {
                Debug.Log(Environment.StackTrace);
                var keymappingsPanel = helper.AddKeymappingsPanel();
                keymappingsPanel.AddKeymapping("Hotkey", Hotkey);
            } catch (Exception ex) {
                Debug.LogException(ex);
                UIView.ForwardException(ex);
            }
        }
    }

    public class ExampleTool : ToolBase {
        UILabel label_;
        UIComponent button_;
        protected override void Awake() {
            try {
                base.Awake();
                string spritePath = UUIHelpers.GetFullPath<UserModExtension>("Resources", "A1.png");
                Debug.Log("[UUIExampleMod] ExampleTool.Awake() sprites=" + spritePath);
                Texture2D icon = UUIHelpers.LoadTexture(spritePath);
                var hotkeys = new UUIHotKeys { ActivationKey = ModSettings.Hotkey };

                button_ = UUIHelpers.RegisterToolButton(
                    name: "ExampleModButton",
                    groupName: null, // default group
                    tooltip: "UUI Example Mod",
                    tool: this,
                    icon: icon,
                    hotkeys: hotkeys);

            } catch (Exception ex) {
                Debug.LogException(ex);
                UIView.ForwardException(ex);
            }
        }

        protected override void OnDestroy() {
            try {
                button_.Destroy();
                base.OnDestroy();
                button_ = null;
                label_ = null;
            } catch (Exception ex) {
                Debug.LogException(ex);
                UIView.ForwardException(ex);
            }
        }

        protected override void OnEnable() {
            try {
                Debug.Log("[UUIExampleMod] ExampleTool.OnEnable()" + Environment.StackTrace);
                base.OnEnable();
                label_ = UIView.GetAView().AddUIComponent(typeof(UILabel)) as UILabel;
                label_.text = "Hello world!";
            } catch (Exception ex) {
                Debug.LogException(ex);
                UIView.ForwardException(ex);
            }
        }

        protected override void OnDisable() {
            try {
                label_.Destroy();
                base.OnDisable();
            } catch (Exception ex) {
                Debug.LogException(ex);
                UIView.ForwardException(ex);
            }
        }
    }
}
