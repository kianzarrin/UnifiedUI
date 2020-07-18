namespace UnitedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons.UI;
    using KianCommons;
    using UnityEngine;
    using ColossalFramework;

    public class FloatingButton : UIButton {
        public static FloatingButton Instance { get; private set; }
        public static readonly SavedFloat SavedX = new SavedFloat(
            "PanelX", Settings.FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "PanelY", Settings.FileName, 0, true);
        public static readonly SavedBool Draggable = new SavedBool(
            "ButtonDraggable", Settings.FileName, def:false, true);

        const string IconNormal = "IconNormal";
        const string IconHovered = "IconHovered";
        const string IconPressed = "IconPressed";
        public static string AtlasName = "UnitedUIFloatingButton_rev" +
            typeof(FloatingButton).Assembly.GetName().Version.Revision;
        public const int SIZE = 64;

        public bool active_ = false;
        public bool IsActive {
            get=> active_;
            set { if (value) Activate(); else Deactivate(); }
        } 

        private UIDragHandle Drag { get; set; }

        public override void Start() {
            Log.Debug("FloatingButton.Start() is called.");
            base.Start();
            name = nameof(UIButtonExt);
            absolutePosition = new Vector3(SavedX, SavedY);
            size = new Vector2(SIZE, SIZE);

            SetupSprites();
            SetupDrag();
            // m_TooltipBox = GetUIView()?.defaultTooltipBox; // Set up the tooltip

            Show();
            canFocus = false;
            Unfocus();
            Invalidate();

            Instance = this;
            Log.Debug("FloatingButton.Start() done!");
        }

        public UITextureAtlas SetupSprites() {
            normalBgSprite = "ButtonMenu";
            hoveredBgSprite = "ButtonMenuHovered";
            pressedBgSprite = "ButtonMenuPressed";
            disabledBgSprite = "ButtonMenuDisabled";

            string[] spriteNames = new string[] { IconNormal, IconHovered, IconPressed };
            var atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas) {
                atlas = TextureUtil.CreateTextureAtlas("A.png", AtlasName, SIZE, SIZE, spriteNames);
            }
            Log.Debug("atlas name is: " + atlas.name);
            this.atlas = atlas;
            Deactivate();
            return atlas;
        }

        public void SetupDrag() {
            var dragHandler = new GameObject("UnitedUI_FloatingButton_DragHandler");
            dragHandler.transform.parent = transform;
            dragHandler.transform.localPosition = Vector3.zero;
            Drag = dragHandler.AddComponent<UIDragHandle>();

            Drag.width = width;
            Drag.height = height;
            Drag.enabled = !Draggable.value;
        }

        public void Activate() {
            focusedFgSprite = normalFgSprite = disabledFgSprite = IconPressed;
            hoveredFgSprite = IconPressed;
            pressedFgSprite = IconPressed;
            Invalidate();
            active_ = true;
        }

        public void Deactivate() {
            focusedFgSprite = normalFgSprite = disabledFgSprite = IconNormal;
            hoveredFgSprite = IconHovered;
            pressedFgSprite = IconPressed;
            Invalidate();
            active_ = false;
        }

        protected override void OnClick(UIMouseEventParameter p) {
            Log.Debug("FloatingButton.OnClick() called");
            base.OnClick(p);
            if (IsActive)
                Activate();
            else
                Deactivate();

            // TODO action.
        }

        protected override void OnPositionChanged() {
            base.OnPositionChanged();
            Log.Debug("OnPositionChanged called");

            Vector2 resolution = GetUIView().GetScreenResolution();

            absolutePosition = new Vector2(
                Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            SavedX.value = absolutePosition.x;
            SavedY.value = absolutePosition.y;
            Log.Debug("absolutePosition: " + absolutePosition);
        }

    }
}
