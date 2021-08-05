namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;

    public abstract class ButtonBase : UIButton {
        internal const string ICON = "Icon";
        internal const string BG_NORMAL = "BGNormal";
        internal const string BG_HOVERED = "BGHovered";
        internal const string BG_PRESSED = "BGPressed";
        internal const string BG_DISABLED = "BGDisabled";

        internal protected string Icon = ICON;
        internal protected string BGNormal = BG_NORMAL;
        internal protected string BGHovered = BG_HOVERED;
        internal protected string BGPressed = BG_PRESSED;
        internal protected string BGDisabled = BG_DISABLED;

        public string SuggestedAtlasName => $"{GetType().FullName}_{Name}_rev" + typeof(ButtonBase).VersionOf();
        public const int SIZE = 40;
        public virtual string Name => GetType().Name;
        public virtual string Tooltip => null;

        public bool active_ = false;

        public SavedInputKey ActivationKey;
        public Dictionary<SavedInputKey, Func<bool>> ActiveKeys;

        public bool IsActive {
            get => active_;
            set { if (value) UseActiveSprites(); else UseInactiveSprites(); }
        }

        public override void Awake() {
            try {
                base.Awake();
                isVisible = true;
                size = new Vector2(SIZE, SIZE);
                canFocus = false;
                name = Name;
                if (Tooltip != null) tooltip = Tooltip;
            } catch (Exception ex) { Log.Exception(ex); }
        }

        public override void Start() {
            try {
                Log.Debug(ThisMethod + " is called for " + Name, false);
                base.Start();
                disabledFgSprite = pressedFgSprite = hoveredFgSprite = normalFgSprite = Icon;
                hoveredBgSprite = BGHovered;
                pressedBgSprite = BGPressed;
                disabledBgSprite = BGDisabled;
                UseInactiveSprites();
                // m_TooltipBox = GetUIView()?.defaultTooltipBox; // Set up the tooltip

                MainPanel.Instance.EventToolChanged += OnToolChanged;
            } catch (Exception ex) { Log.Exception(ex); }
        }

        public virtual void OnToolChanged(ToolBase newTool) { }

        public override void OnDestroy() {
            Log.Debug(ThisMethod + " called for " + Name, false);
            Hide();
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }


        public static string MainAtlasName => $"MainAtals_rev" + typeof(ButtonBase).VersionOf();
        const string MAIN_ATLAS_FILE = "bg_sprites.png";
        public static UITextureAtlas CreateMainAtlas() {
            try {
                Log.Called();
                Destroy(TextureUtil.GetAtlasOrNull(MainAtlasName));
                var spriteNames = new string[] { BG_NORMAL, BG_HOVERED, BG_PRESSED, BG_DISABLED };
                var texture2D = TextureUtil.GetTextureFromFile(MAIN_ATLAS_FILE);
                return TextureUtil.CreateTextureAtlas(texture2D, MainAtlasName, spriteNames);
            } catch (Exception ex) { ex.Log(); }
            return null;
        }

        public static UITextureAtlas GetOrCreateAtlas(string atlasName, string spriteFile, bool embeded = false, string[] spriteNames = null) {
            try {
                Log.Called(atlasName, spriteFile, embeded, spriteNames);
                spriteNames ??= new string[] { BG_NORMAL, BG_HOVERED, BG_PRESSED, BG_DISABLED };
                var _atlas = TextureUtil.GetAtlas(atlasName);
                if (_atlas == UIView.GetAView().defaultAtlas) {
                    Texture2D texture2D = embeded ?
                        TextureUtil.GetTextureFromAssemblyManifest(spriteFile) :
                        TextureUtil.GetTextureFromFile(spriteFile);
                    _atlas = TextureUtil.CreateTextureAtlas(texture2D, atlasName, spriteNames);
                }
                return _atlas;
            } catch (Exception ex) {
                Log.Exception(ex);
                return TextureUtil.Ingame;
            }
        }

        public static void AddTextureToAtlas(UITextureAtlas atlas, Texture2D newTexture) {
            try {
                if (atlas.spriteNames.Contains(newTexture.name)) {
                    Log.Error("atlas already has " + newTexture.name);
                } else {
                    Rect[] regions = atlas.texture.PackTextures(
                       new[] { atlas.texture, newTexture }, atlas.padding, 4096, false);
                    atlas.sprites.Add(new UITextureAtlas.SpriteInfo {
                        name = newTexture.name,
                        texture = newTexture,
                        region = regions[1],
                    });
                    atlas.RebuildIndexes();
                }
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public void UseActiveSprites() {
            normalBgSprite = BGPressed;
            Invalidate();
            active_ = true;
        }

        public void UseInactiveSprites() {
            normalBgSprite = BGNormal;
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
