namespace ExampleMod {
    using ColossalFramework;
    using ColossalFramework.UI;
    using ICities;
    using System;
    using UnifiedUI.Helpers;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class UserModExtension : LoadingExtensionBase, IUserMod {
        public static UserModExtension Instance;

        internal static bool InGameOrEditor =>
            SceneManager.GetActiveScene().name != "IntroScreen" &&
            SceneManager.GetActiveScene().name != "Startup";

        public string Name => "Example Mod";
        public string Description => "Show case for using UnifiedUI helpers";

        public void OnEnabled() {
            Instance = this;
            if (InGameOrEditor) // hot reload
                Init();
        }

        public void OnDisabled() {
            Instance = null;
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
        const string FILE_NAME = "UUIExampleMod";

        static ModSettings() {
            if (GameSettings.FindSettingsFileByName(FILE_NAME) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FILE_NAME } });
            }
        }

        public static SavedInputKey Hotkey = new SavedInputKey(
            "UUIExampleMod_HotKey", FILE_NAME,
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
                string sprites = UserModExtension.Instance.GetFullPath("Resources", "B.png");
                Debug.Log("[UUIExampleMod] ExampleTool.Awake() sprites=" + sprites);
                button_ = UUIHelpers.RegisterToolButton(
                    name: "ExampleModButton",
                    groupName: null, // default group
                    tooltip: "UUI Example Mod",
                    spritefile: sprites,
                    tool: this,
                    activationKey: ModSettings.Hotkey);

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
