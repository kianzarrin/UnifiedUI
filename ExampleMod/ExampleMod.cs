namespace ExampleMod {
    using System;
    using System.IO;
    using ICities;
    using UnityEngine.SceneManagement;
    using UnifiedUI.Helpers;
    using ColossalFramework.UI;
    using ColossalFramework;
    using UnityEngine;

    public class UserModExtension : LoadingExtensionBase, IUserMod {
        public string Name => "Example Mod";
        public string Description => "Show case for using UnifiedUI helpers";

        public static UserModExtension Instance;

        internal static bool InGameOrEditor =>
            SceneManager.GetActiveScene().name != "IntroScreen" &&
            SceneManager.GetActiveScene().name != "Startup";

        public void OnEnabled() {
            Instance = this;
            if (InGameOrEditor)
                LifeCycle.Init();
        }

        public void OnDisabled() {
            Instance = null;
            LifeCycle.Release();
        }

        public override void OnLevelLoaded(LoadMode mode) => LifeCycle.Init();

        public override void OnLevelUnloading() => LifeCycle.Release();
    }

    internal static class LifeCycle {
        internal static void Init() {
            Debug.Log("[UUIExampleMod] LifeCycle.Init()");
            ToolsModifierControl.toolController.gameObject.AddComponent<ExampleTool>();
        }
        internal static void Release() =>
            ToolsModifierControl.toolController.GetComponent<ExampleTool>()?.Destroy();
    }

    public class ExampleTool : ToolBase {
        UILabel label;
        UIComponent button;
        protected override void Awake() {
            base.Awake();
            string sprites = UserModExtension.Instance.GetFullPath("Resources", "B.png");
            Debug.Log("[UUIExampleMod] ExampleTool.Awake() sprites=" + sprites);
            button = UUIHelpers.RegisterToolButton(
                name: "ExampleModButton",
                groupName: null, // default group
                tooltip: "Example Mod",
                spritefile: sprites,
                tool: this);
        }

        protected override void OnDestroy() {
            button.Destroy();
            base.OnDestroy();
        }


        protected override void OnEnable() {
            Debug.Log("[UUIExampleMod] ExampleTool.OnEnable()");
            base.OnEnable();
            label = UIView.GetAView().AddUIComponent(typeof(UILabel)) as UILabel;
            label.text = "Hello world!";
        }

        protected override void OnDisable() {
            Destroy(label.gameObject);
            base.OnDisable();
        }
    }
}
