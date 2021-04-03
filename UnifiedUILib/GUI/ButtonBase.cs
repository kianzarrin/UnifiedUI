namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;

    public abstract class ButtonBase : UIButton {
        const string IconNormal = "IconNormal";
        const string IconHovered = "IconHovered";
        const string IconPressed = "IconPressed";
        const string IconDisabled = "IconDisabled";
        public string AtlasName => $"{GetType().FullName}_{Name}_rev"  + typeof(ButtonBase).VersionOf();
        public const int SIZE = 40;
        public abstract string SpritesFilePath { get; }
        public abstract bool EmbededSprite { get; }
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

                SetupSprites();
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

        public UITextureAtlas SetupSprites() {
            string[] spriteNames = new string[] { IconNormal, IconHovered, IconPressed, IconDisabled };
            var atlas = TextureUtil.GetAtlas(AtlasName);
            if(atlas == UIView.GetAView().defaultAtlas) {
                Texture2D texture2D;
                if(!EmbededSprite)
                    texture2D = TextureUtil.GetTextureFromFile(SpritesFilePath);
                else
                    texture2D = TextureUtil.GetTextureFromAssemblyManifest(SpritesFilePath);
                return TextureUtil.CreateTextureAtlas(texture2D, AtlasName, spriteNames);
            }
            Log.Debug("atlas name is: " + atlas.name, false);
            this.atlas = atlas;
            UseDeactiveSprites();
            return atlas;
        }

        public void UseActiveSprites() {
            // focusedBgSprite = can focus is set to false.
            normalBgSprite = IconPressed;
            hoveredBgSprite = IconPressed;
            pressedBgSprite = IconPressed;
            disabledBgSprite = IconDisabled;
            Invalidate();
            active_ = true;
        }

        public void UseDeactiveSprites() {
            // focusedBgSprite = can focus is set to false.
            normalBgSprite = IconNormal;
            hoveredBgSprite = IconHovered;
            pressedBgSprite = IconPressed;
            disabledBgSprite = IconDisabled;
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
                LogCalled();
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
