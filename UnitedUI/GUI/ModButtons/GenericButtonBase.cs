namespace UnitedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons.UI;
    using KianCommons;
    using UnityEngine;
    using ColossalFramework;
    using System;
    using System.Linq;

    public abstract class GenericButton : UIButton {
        const string IconNormal = "IconNormal";
        const string IconHovered = "IconHovered";
        const string IconPressed = "IconPressed";
        public string AtlasName => $"{GetType().FullName}_rev" + 
            typeof(GenericButton).Assembly.GetName().Version.Revision;
        public const int SIZE = 64;

        public abstract string SpritesFileName { get; }
        public virtual string Name => GetType().Name;
        public virtual ToolBase Tool => null;
        public virtual UIComponent Component => null;

        public bool active_ = false;
        public bool IsActive {
            get => active_;
            set { if (value) UseActiveSprites(); else UseDeactiveSprites(); }
        }

        bool started_ = false;
        public override void Start() {
            Log.Debug("GenericButton.Start() is called for " + Name);
            base.Start();
            name = Name;
            size = new Vector2(40, 40);

            SetupSprites();
            // m_TooltipBox = GetUIView()?.defaultTooltipBox; // Set up the tooltip

            canFocus = false;
            Show();
            Invalidate();


            started_ = true;
            Log.Debug("GenericButton.Start() done! for " + Name);
        }

        public virtual void OnToolChanged(ToolBase newTool) {
            var tool = Tool;
            if (!tool)
                return;
            IsActive = newTool == Tool;
        }

        public override void OnDestroy() {
            Log.Debug("GenericButton.OnDestroy() called for " + Name);
            Hide();
            base.OnDestroy();
        }

        public UITextureAtlas SetupSprites() {
            string[] spriteNames = new string[] { IconPressed, IconHovered, IconNormal };
            var atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas) {
                atlas = TextureUtil.CreateTextureAtlas(SpritesFileName, AtlasName, SIZE, SIZE, spriteNames);
            }
            Log.Debug("atlas name is: " + atlas.name);
            this.atlas = atlas;
            UseDeactiveSprites();
            return atlas;
        }

        public void UseActiveSprites() {
            focusedBgSprite = normalBgSprite = disabledBgSprite = IconPressed;
            hoveredBgSprite = IconPressed;
            pressedBgSprite = IconPressed;
            Invalidate();
            active_ = true;
        }

        public void UseDeactiveSprites() {
            focusedBgSprite = normalBgSprite = disabledBgSprite = IconNormal;
            hoveredBgSprite = IconHovered;
            pressedBgSprite = IconPressed;
            Invalidate();
            active_ = false;
        }

        public virtual void Activate() {
            Log.Debug("GenericButton.Open() called for " + Name);
            IsActive = true;
            var tool = Tool;
            if(tool)ToolsModifierControl.toolController.CurrentTool = tool;
            Component?.Hide();
        }

        public virtual void Deactivate() {
            Log.Debug("GenericButton.Close() called  for " + Name);
            IsActive = false;
            if (Tool && ToolsModifierControl.toolController?.CurrentTool == Tool)
                ToolsModifierControl.SetTool<DefaultTool>();
            Component?.Show();
        }

        public void Toggle() {
            if (IsActive) Deactivate();
            else Activate();
        }

        protected override void OnClick(UIMouseEventParameter p) {
            Log.Debug("GenericButton.OnClick() called  for " + Name);
            base.OnClick(p);
            Toggle();
        }

        public static ToolBase GetTool(string assemblyName, string fullName, string instanceName) {
            var fieldInfo = Type.GetType(fullName + ", " + assemblyName)?.GetField(instanceName);
            return fieldInfo?.GetValue(null) as ToolBase ?? throw new Exception("Could not find " + fullName);
        }

        public static ToolBase GetTool(string name) {
            GameObject gameObject = ToolsModifierControl.toolController.gameObject;
            var ret = gameObject.GetComponents<ToolBase>()
                .Where(tool => tool.GetType().Name == name)
                .FirstOrDefault();
            if (ret == null)
                Log.Error("could not find tool: " + name);
            return ret;
        }
    }
}
