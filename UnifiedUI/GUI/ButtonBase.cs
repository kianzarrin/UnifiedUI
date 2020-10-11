namespace UnifiedUI.GUI
{
    using ColossalFramework.UI;
    using KianCommons.UI;
    using KianCommons;
    using UnityEngine;
    using UnifiedUI.LifeCycle;
    using ColossalFramework.Plugins;

    public abstract class ButtonBase : UIButton
    {
        const string IconNormal = "IconNormal";
        const string IconHovered = "IconHovered";
        const string IconPressed = "IconPressed";
        const string IconDisabled = "IconDisabled";
        public string AtlasName => $"{GetType().FullName}_rev" + typeof(ButtonBase).VersionOf();
        public const int SIZE = 40;

        public abstract string SpritesFileName { get; }
        public virtual string Name => GetType().Name;
        public virtual string Tooltip => null;
        private PluginManager.PluginInfo plugin_;

        public bool active_ = false;
        public bool IsActive
        {
            get => active_;
            set { if (value) UseActiveSprites(); else UseDeactiveSprites(); }
        }

        public override void Awake()
        {
            base.Awake();
            isVisible = true;
            size = new Vector2(SIZE, SIZE);
            canFocus = false;
            name = Name;
        }

        public override void Start()
        {
            Log.Debug("ButtonBase.Start() is called for " + Name, false);
            base.Start();
            if (Tooltip != null) tooltip = Tooltip;

            SetupSprites();
            // m_TooltipBox = GetUIView()?.defaultTooltipBox; // Set up the tooltip

            ThreadingExtension.EventToolChanged += OnRefresh;
        }

        public virtual void OnRefresh(ToolBase newTool) { }

        public override void OnDestroy()
        {
            Log.Debug("ButtonBase.OnDestroy() called for " + Name, false);
            Hide();
            base.OnDestroy();
        }

        public UITextureAtlas SetupSprites()
        {
            string[] spriteNames = new string[] { IconNormal, IconHovered, IconPressed, IconDisabled };
            var atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas)
            {
                atlas = TextureUtil.CreateTextureAtlas(SpritesFileName, AtlasName, SIZE, SIZE, spriteNames);
            }
            Log.Debug("atlas name is: " + atlas.name, false);
            this.atlas = atlas;
            UseDeactiveSprites();
            return atlas;
        }

        public void UseActiveSprites()
        {
            // focusedBgSprite = can focus is set to false.
            normalBgSprite = IconPressed;
            hoveredBgSprite = IconPressed;
            pressedBgSprite = IconPressed;
            disabledBgSprite = IconDisabled;
            Invalidate();
            active_ = true;
        }

        public void UseDeactiveSprites()
        {
            // focusedBgSprite = can focus is set to false.
            normalBgSprite = IconNormal;
            hoveredBgSprite = IconHovered;
            pressedBgSprite = IconPressed;
            disabledBgSprite = IconDisabled;
            Invalidate();
            active_ = false;
        }
    }
}
