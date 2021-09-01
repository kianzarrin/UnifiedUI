namespace UnifiedUI.Tool {
    using System;
    using System.Linq;
    using ColossalFramework.UI;
    using UnityEngine;
    using ColossalFramework;
    using KianCommons;
    using UnifiedUI.GUI;

    internal abstract class UUIToolBase<T> : UUIToolBase where T : ToolBase {
        public static T Instance { get; private set; }
        public static T Create() => Instance = ToolsModifierControl.toolController.gameObject.AddComponent<T>();

        public static void Release() {
            try { DestroyImmediate(Instance); } catch (Exception ex) { ex.Log(); }
            Instance = null;
        }
    }
    internal abstract class UUIToolBase : ToolBase{

        protected override void OnEnable() {
            FloatingText = "";
            base.OnEnable();
        }
        protected override void OnDisable() {
            base.OnDisable();
            if (ToolsModifierControl.toolController.CurrentTool == this)
                ToolsModifierControl.SetTool<DefaultTool>();

            ShowToolInfo(
                !FloatingText.IsNullOrWhiteSpace(),
                FloatingText,
                Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        protected GUIStyle RedRectStyle;
        protected GUIStyle GreenRectStyle;

        protected string FloatingText { get; set; }

        protected override void OnToolUpdate() {
            base.OnToolUpdate();
            if (UIView.library.Get("PauseMenu").isVisible) {
                UIView.library.Hide("PauseMenu");
                enabled = false;
            }
        }

        public UIButton HoveredButton => FindObjectsOfType<UIButton>().FirstOrDefault(item => item.containsMouse);

        public static Rect GetRect(UIComponent c) {
            var size = c.size;
            var absolutePosition = c.absolutePosition;
            var dx = Screen.width / UIScaler.BaseResolutionX;
            var dy = Screen.height / UIScaler.BaseResolutionY;
            return new Rect(absolutePosition.x * dx, absolutePosition.y * dy, size.x * dx, size.y * dy);
        }

        protected override void OnToolGUI(Event e) {
            base.OnToolGUI(e);
            if (RedRectStyle == null) {
                RedRectStyle = new GUIStyle(GUI.skin.box);
                var bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, new Color(1.0f, 0.0f, 0.0f, 0.3f));
                bgTexture.Apply();
                RedRectStyle.normal.background = bgTexture;
                RedRectStyle.hover.background = bgTexture;
                RedRectStyle.active.background = bgTexture;
                RedRectStyle.focused.background = bgTexture;

                GreenRectStyle = new GUIStyle(GUI.skin.box);
                bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, new Color(0.0f, 1.0f, 0.0f, 0.3f));
                bgTexture.Apply();
                GreenRectStyle.normal.background = bgTexture;
                GreenRectStyle.hover.background = bgTexture;
                GreenRectStyle.active.background = bgTexture;
                GreenRectStyle.focused.background = bgTexture;
            }
        }
    }

}
