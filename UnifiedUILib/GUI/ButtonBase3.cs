namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;
    using System.Linq;
    public abstract class ButtonBase3 : UIButton {
        internal const string ICON = "Icon";
        internal const string BG_NORMAL = "Normal";
        internal const string BG_ACTIVE = "Active";
        internal const string BG_HOVERED = "Hovered";
        internal const string BG_PRESSED = "Pressed";
        internal const string BG_DISABLED = "Disabled";

        internal protected string IconName = ICON;
        internal protected string BGNormal = BG_NORMAL;
        internal protected string BGActive = BG_ACTIVE;
        internal protected string BGHovered = BG_HOVERED;
        internal protected string BGPressed = BG_PRESSED;
        internal protected string BGDisabled = BG_DISABLED;

        public string SuggestedAtlasName => $"{GetType().FullName}_{Name}_rev"  + typeof(ButtonBase3).VersionOf();
        public const int SIZE = 40;
        public virtual string Name => GetType().Name;
        public virtual string Tooltip => null;

        public bool active_ = false;

        public SavedInputKey ActivationKey;
        public Dictionary<SavedInputKey, Func<bool>> ActiveKeys;

        public bool IsActive {
            get => active_;
            set { if(value) UseActiveSprites(); else UseDeactiveSprites(); }
        }

        public override void Awake() {
            try {
                base.Awake();
                normalFgSprite = hoveredFgSprite = pressedFgSprite = disabledFgSprite = IconName;
                isVisible = true;
                size = new Vector2(SIZE, SIZE);
                canFocus = false;
                name = Name;
                if(Tooltip != null) tooltip = Tooltip;
            } catch(Exception ex) { Log.Exception(ex); }
        }

        public override void Start() {
            try {
                Log.Debug(ThisMethod + " is called for " + Name, false);
                base.Start();
                UseDeactiveSprites();
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

        public static void AddTextureToAtlas(UITextureAtlas atlas, Texture2D texture) {
            try { 
                if (atlas.spriteNames.Contains(texture.name)) {
                    Log.Info("atlas already has " + texture.name);
                }
                TextureUtil.AddTexturesInAtlas(atlas, new[] { texture });
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public void UseActiveSprites() {
            // focusedBgSprite = can focus is set to false.
            normalBgSprite = BGPressed;
            hoveredBgSprite = BGHovered;
            pressedBgSprite = BGPressed;
            disabledBgSprite = BGDisabled;
            Invalidate();
            active_ = true;
        }

        public void UseDeactiveSprites() {
            // focusedBgSprite = can focus is set to false.
            normalBgSprite = BGNormal;
            hoveredBgSprite = BGHovered;
            pressedBgSprite = BGPressed;
            disabledBgSprite = BGDisabled;
            Invalidate();
            active_ = false;
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
