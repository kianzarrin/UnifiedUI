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
            if (InGameOrEditor)
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

        public void OnSettingsUI(UIHelper helper) => new ModSettings(helper);
    }

    public class ModSettings {
        const string FILE_NAME = "UUIExampleMod";

        static ModSettings() {
            if (GameSettings.FindSettingsFileByName(FILE_NAME) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FILE_NAME } });
            }
        }

        public static ModSettings instance;

        public SavedInputKey Hotkey = new SavedInputKey(
            "UUIExampleMod_HotKey", FILE_NAME,
            key: KeyCode.A, control: true, shift: true, alt: false, true);


        public ModSettings(UIHelper helper) {
            try {
                instance = this;
                Debug.Log(Environment.StackTrace);
                var keymappingsPanel = helper.AddKeymappingsPanel();
                keymappingsPanel.AddKeymapping("Hotkey", Hotkey);
            } catch (Exception ex) {
                Debug.LogException(ex);
                UIView.ForwardException(ex);
            }
        }
    }

    public class ThreadingExtension : ThreadingExtensionBase {
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            if (ModSettings.instance?.Hotkey?.IsKeyUp() ?? false) {
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }
    }

    internal static class LifeCycle {

    }


    public class ExampleTool : ToolBase {
        UILabel label;
        UIComponent button;
        protected override void Awake() {
            try {
                base.Awake();
                string sprites = UserModExtension.Instance.GetFullPath("Resources", "B.png");
                Debug.Log("[UUIExampleMod] ExampleTool.Awake() sprites=" + sprites);
                button = UUIHelpers.RegisterToolButton(
                    name: "ExampleModButton",
                    groupName: null, // default group
                    tooltip: "UUI Example Mod",
                    spritefile: sprites,
                    tool: this,
                    activationKey: ModSettings.instance.Hotkey);

                if (button != null) {
                    ModSettings.instance.Hotkey = null;
                } else {
                    button = UIView.GetAView().AddUIComponent(typeof(UIPanel));
                    new UIHelper(button).AddButton("UUI Example Mod", () => {
                        if (enabled)
                            ToolsModifierControl.SetTool<DefaultTool>();
                        else
                            enabled = true;
                    });
                }
            } catch (Exception ex) {
                Debug.LogException(ex);
                UIView.ForwardException(ex);
            }
        }

        protected override void OnDestroy() {
            try {
                button.Destroy();
                base.OnDestroy();
            } catch (Exception ex) {
                Debug.LogException(ex);
                UIView.ForwardException(ex);
            }
        }


        protected override void OnEnable() {
            try {
                Debug.Log("[UUIExampleMod] ExampleTool.OnEnable()" + Environment.StackTrace);
                base.OnEnable();
                label = UIView.GetAView().AddUIComponent(typeof(UILabel)) as UILabel;
                label.text = "Hello world!";
            } catch (Exception ex) {
                Debug.LogException(ex);
                UIView.ForwardException(ex);
            }
        }

        protected override void OnDisable() {
            try {
                label.Destroy();
                base.OnDisable();
            } catch (Exception ex) {
                Debug.LogException(ex);
                UIView.ForwardException(ex);
            }
        }
    }
}
