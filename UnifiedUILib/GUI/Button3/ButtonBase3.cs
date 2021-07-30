namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;

    public class Button3Panel : UIPanel {
        public ButtonBase3 Button { get; private set;}

        public override void Awake() {
            base.Awake();
            autoFitChildrenHorizontally = autoFitChildrenVertically = true;
            atlas = TextureUtil.Ingame;
            backgroundSprite = "ButtonWhite";
            Button = AddUIComponent<ButtonBase3>();
        }
    }

    public abstract class ButtonBase3 : UIButton {
        internal const string ICON = "Icon";

        //internal const string BG_NORMAL = "BGHovered";
        //internal const string BG_HOVERED = "BGHovered";
        //internal const string BG_PRESSED = "BGPressed";
        //internal const string BG_DISABLED = "BGDisabled";
        internal static Color NORMAL_COLOR = Color.grey;
        internal static Color ACTIVE_COLOR = Color.blue;
        internal static Color PRESSED_COLOR = Color.green;
        internal static Color HOVERED_COLOR = Color.cyan;
        internal static Color DISABLED_COLOR = Color.grey;
        internal static Color DISABLED_ICON_COLOR = Color.black;


        internal protected string Icon => ICON;
        //internal protected string BGNormal = BG_NORMAL;
        //internal protected string BGHovered = BG_HOVERED;
        //internal protected string BGPressed = BG_PRESSED;
        //internal protected string BGDisabled = BG_DISABLED;

        public UIPanel Wrapper => parent as UIPanel;

        public string SuggestedAtlasName => $"{GetType().FullName}_{Name}_rev"  + typeof(ButtonBase3).VersionOf();
        public const int SIZE = 40;
        public virtual string Name => GetType().Name;
        public virtual string Tooltip => null;

        public bool activeTheme_ = false;

        public SavedInputKey ActivationKey;
        public Dictionary<SavedInputKey, Func<bool>> ActiveKeys;

        public bool IsActive {
            get => activeTheme_;
            set { if(value) ActiveTheme(); else InactiveTheme(); }
        }

        protected override void OnButtonStateChanged(ButtonState newState) {
            try {
                base.OnButtonStateChanged(newState);
                Wrapper.color = newState switch {
                    ButtonState.Normal => NORMAL_COLOR,
                    ButtonState.Hovered => HOVERED_COLOR,
                    ButtonState.Pressed => PRESSED_COLOR,
                    ButtonState.Disabled => DISABLED_COLOR,
                    _ => throw new Exception("Unreachable code"),
                };
            } catch(Exception ex) { ex.Log(); }
        }

        public override void Awake() {
            try {
                base.Awake();
                isVisible = true;
                size = new Vector2(SIZE, SIZE);
                canFocus = false;
                name = Name;
                if(Tooltip != null) tooltip = Tooltip;
                normalBgSprite = hoveredBgSprite = pressedBgSprite = disabledBgSprite = Icon;
                disabledColor = DISABLED_ICON_COLOR;
            } catch (Exception ex) { Log.Exception(ex); }
        }

        public override void Start() {
            try {
                Log.Debug(ThisMethod + " is called for " + Name, false);
                base.Start();
                InactiveTheme();
                // m_TooltipBox = GetUIView()?.defaultTooltipBox; // Set up the tooltip

                MainPanel.Instance.EventToolChanged += OnToolChanged;
            } catch(Exception ex) { Log.Exception(ex); }
        }

        public virtual void OnToolChanged(ToolBase newTool) { }

        public override void OnDestroy() {
            Log.Debug(ThisMethod + " called for " + Name, false);
            Hide();
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public static UITextureAtlas GetOrCreateAtlas(string atlasName, string spriteFile, string[] spriteNames = null) {
            try {
                Log.Called(atlasName, spriteFile, spriteNames);
                spriteNames ??= new string[] { ICON };
                return TextureUtil.GetAtlasOrNull(atlasName) ??
                    TextureUtil.CreateTextureAtlas(TextureUtil.GetTextureFromFile(spriteFile), atlasName, spriteNames);
            } catch (Exception ex) {
                Log.Exception(ex);
                return TextureUtil.Ingame;
            }
        }


        public void ActiveTheme() {
            Invalidate();
            activeTheme_ = true;
        }

        public void InactiveTheme() {
            Invalidate();
            activeTheme_ = false;
        }

        public bool AvoidCollision() {
            if(!IsActive || ActiveKeys.IsNullorEmpty())
                return false;
            foreach(var pair in ActiveKeys) {
                var active = pair.Value?.Invoke() ?? true;
                var key = pair.Key;
                if(active && key.IsKeyUp())
                    return true;
            }
            return false;
        }

        public bool HandleHotKey() {
            if(ActivationKey != null && ActivationKey.IsKeyUp()) {
                Toggle();
                return true;
            }
            return false;
        }

        public virtual void Activate() {
            IsActive = true;
        }

        public virtual void Deactivate() {
            IsActive = false;
        }

        public virtual void Toggle() {
            try {
                Log.Called();
                Log.Debug(Environment.StackTrace);
                if(IsActive) Deactivate();
                else Activate();
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        public static DefaultTool DefaultTool => ToolsModifierControl.GetTool<DefaultTool>();

        static ToolBase prevtool_;
        public static ToolBase Prevtool {
            get {
                if(!prevtool_) prevtool_ = null;
                return prevtool_;
            }
            set => prevtool_ = value;
        }

        public static void SetTool(ToolBase tool) {
            LogCalled(tool);
            if(!tool || !ToolsModifierControl.toolController) return;
            if(ToolsModifierControl.toolController.CurrentTool == tool) return;

            Prevtool = ToolsModifierControl.toolController.CurrentTool;
            if(MainPanel.ClearInfoPanelsOnToolChanged) {
                if(!ToolsModifierControl.keepThisWorldInfoPanel)
                    WorldInfoPanel.HideAllWorldInfoPanels();
                GameAreaInfoPanel.Hide();
                ToolsModifierControl.keepThisWorldInfoPanel = false;
            }
            ToolsModifierControl.toolController.CurrentTool = tool;
        }

        public static void UnsetTool(ToolBase tool) {
            LogCalled(tool);
            if(!tool || ToolsModifierControl.toolController?.CurrentTool != tool) return;

            if (!MainPanel.SwitchToPrevTool ||
                !prevtool_ ||
                prevtool_ == Singleton<BulldozeTool>.instance ||
                prevtool_ == DefaultTool) {
                SetTool(DefaultTool);
                Prevtool = null;
            } else {
                SetTool(Prevtool);
                Prevtool = null; // prevent exit loop.
            }
        }

    }
}
