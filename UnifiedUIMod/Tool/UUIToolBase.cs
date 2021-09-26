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
        }

        protected GUIStyle RedRectStyle;
        protected GUIStyle OrangeRectStyle;
        protected GUIStyle GreenRectStyle;

        protected string FloatingText { get; set; }

        protected override void OnToolUpdate() {
            base.OnToolUpdate();
            if (UIView.library.Get("PauseMenu").isVisible) {
                UIView.library.Hide("PauseMenu");
                enabled = false;
            }
            ShowToolInfo(
                !FloatingText.IsNullOrWhiteSpace(),
                FloatingText,
                (Vector2)Input.mousePosition);
        }

        private static readonly string kCursorInfoErrorColor = "<color #ff7e00>";
        private static readonly string kCursorInfoNormalColor = "<color #87d3ff>";
        private static readonly string kCursorInfoCloseColorTag = "</color>";

        protected void ShowToolInfo(bool show, string text, Vector2 pos) {
            if (ToolBase.cursorInfoLabel == null) {
                return;
            }
            ToolErrors errors = this.GetErrors();
            if ((errors & ToolErrors.VisibleErrors) != ToolErrors.None) {
                bool hasText = !string.IsNullOrEmpty(text);
                if (hasText) {
                    text += "\n";
                } else {
                    text = string.Empty;
                }
                text += kCursorInfoErrorColor;
                for(var toolErrors = ToolErrors.ObjectCollision;
                    toolErrors <= ToolErrors.AdministrationBuildingExists;
                    toolErrors = (ToolErrors)((int)toolErrors << 1)) {
                    if((errors & toolErrors) != ToolErrors.None) {
                        if(hasText) {
                            text += "\n";
                        }
                        hasText = true;
                        text += this.GetErrorString(toolErrors);
                    }
                }
                text += kCursorInfoCloseColorTag;
            }
            if (!string.IsNullOrEmpty(text) && show) {
                text = kCursorInfoNormalColor + text + kCursorInfoCloseColorTag;
                ToolBase.cursorInfoLabel.isVisible = true;
                UIView uiview = ToolBase.cursorInfoLabel.GetUIView();
                Vector2 res = (!(ToolBase.fullscreenContainer != null)) ? uiview.GetScreenResolution() : ToolBase.fullscreenContainer.size;
                Vector2 startCorner = ToolBase.cursorInfoLabel.pivot.UpperLeftToTransform(ToolBase.cursorInfoLabel.size, ToolBase.cursorInfoLabel.arbitraryPivotOffset);
                Vector3 relativePosition = uiview.ScreenPointToGUI(pos/ uiview.inputScale) + startCorner;
                ToolBase.cursorInfoLabel.text = text;
                if (relativePosition.x < 0f) {
                    relativePosition.x = 0f;
                }
                if (relativePosition.y < 0f) {
                    relativePosition.y = 0f;
                }
                if (relativePosition.x + ToolBase.cursorInfoLabel.width > res.x) {
                    relativePosition.x = res.x - ToolBase.cursorInfoLabel.width;
                }
                if (relativePosition.y + ToolBase.cursorInfoLabel.height > res.y) {
                    relativePosition.y = res.y - ToolBase.cursorInfoLabel.height;
                }
                ToolBase.cursorInfoLabel.relativePosition = relativePosition;
            } else {
                ToolBase.cursorInfoLabel.isVisible = false;
            }
        }

        public UIButton HoveredButton =>
            FindObjectsOfType<UIButton>()
            .FirstOrDefault(item => item.containsMouse && item is not FloatingButton);

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
                var bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, new Color(1.0f, 0.0f, 0.0f, 0.3f));
                bgTexture.Apply();
                RedRectStyle = new GUIStyle(GUI.skin.box); 
                RedRectStyle.normal.background = bgTexture;
                RedRectStyle.hover.background = bgTexture;
                RedRectStyle.active.background = bgTexture;
                RedRectStyle.focused.background = bgTexture;

                bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, new Color(0.0f, 1.0f, 0.0f, 0.3f));
                bgTexture.Apply();
                GreenRectStyle = new GUIStyle(GUI.skin.box);
                GreenRectStyle.normal.background = bgTexture;
                GreenRectStyle.hover.background = bgTexture;
                GreenRectStyle.active.background = bgTexture;
                GreenRectStyle.focused.background = bgTexture;

                bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, new Color32(255, 128, 0, 75));
                bgTexture.Apply();
                OrangeRectStyle = new GUIStyle(GUI.skin.box);
                OrangeRectStyle.normal.background = bgTexture;
                OrangeRectStyle.hover.background = bgTexture;
                OrangeRectStyle.active.background = bgTexture;
                OrangeRectStyle.focused.background = bgTexture;
            }
        }
    }

}
