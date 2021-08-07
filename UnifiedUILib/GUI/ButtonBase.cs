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
        internal const string BG_NORMAL = "bg_normal";
        internal const string BG_HOVERED = "bg_hovered";
        internal const string BG_PRESSED = "bg_pressed";
        internal const string BG_DISABLED = "bg_disabled";

        internal protected string Icon = ICON;
        internal protected string BGNormal = BG_NORMAL;
        internal protected string BGHovered = BG_HOVERED;
        internal protected string BGPressed = BG_PRESSED;
        internal protected string BGDisabled = BG_DISABLED;

        public string SuggestedAtlasName => $"{GetType().FullName}_{Name}_rev" + typeof(ButtonBase).VersionOf();
        public const int SIZE = 40;

        public static string MainAtlasName => $"MainAtals_rev" + typeof(ButtonBase).VersionOf();

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
            } catch (Exception ex) { Log.Exception(ex); }
        }

        public override void Start() {
            try {
                Log.Debug(ThisMethod + " is called for " + Name, false);
                base.Start();
                name = Name;
                if (Tooltip != null) tooltip = Tooltip;
                disabledFgSprite = pressedFgSprite = hoveredFgSprite = normalFgSprite = Icon;
                pressedBgSprite = BGPressed;
                disabledBgSprite = BGDisabled;
                UseInactiveSprites();
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

        public static UITextureAtlas CreateMainAtlas() {
            try {
                Log.Called();
                Destroy(TextureUtil.GetAtlasOrNull(MainAtlasName));
                var spriteNames = new [] { BG_NORMAL, BG_HOVERED, BG_PRESSED, BG_DISABLED };
                var textures = spriteNames.Select(GetTexture).ToArray();
                return TextureUtil.CreateTextureAtlas(MainAtlasName, textures);
            } catch (Exception ex) { ex.Log(); }
            return null;

            static Texture2D GetTexture(string _spriteName) {
                var _texture = TextureUtil.GetTextureFromAssemblyManifest(_spriteName + ".png");
                _texture.name = _spriteName;
                return _texture;
            }
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

        public void SetIcon(Texture2D texture) {
            Assertion.Assert(texture, "texture");
            atlas = MainPanel.Instance.MainAtlas;
            AddTextureToAtlas(atlas, texture);
            Icon = texture.name;
        }

        public static void AddTextureToAtlas(UITextureAtlas atlas, Texture2D newTexture) {
            try {
                if (atlas.spriteNames.Contains(newTexture.name)) {
                    Log.Error("atlas already has " + newTexture.name);
                } else {
                    TextureUtil.AddTexturesToAtlas(atlas, new[] { newTexture });
                }
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public void UseActiveSprites() {
            normalBgSprite = BGPressed;
            hoveredBgSprite = BGPressed;
            Invalidate();
            active_ = true;
        }

        public void UseInactiveSprites() {
            normalBgSprite = BGNormal;
            hoveredBgSprite = BGHovered;
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
