namespace UnitedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons.UI;
    using KianCommons;
    using UnityEngine;
    using ColossalFramework;

    public class FloatingButton : UIButton {
        public static FloatingButton Instance { get; private set; }
        public static readonly SavedFloat SavedX = new SavedFloat(
            "ButtonX", Settings.FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "ButtonY", Settings.FileName, 0, true);
        public static readonly SavedBool SavedDraggable = new SavedBool(
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
            set { if (value) UseActiveSprites(); else UseDeactiveSprites(); }
        } 

        private UIDragHandle drag_ { get; set; }

        public bool IsDragable {
            get => SavedDraggable.value;
            set {
                SavedDraggable.value = value;
                if(drag_)
                    drag_.enabled = value;
            }
        }

        bool started_ = false;
        public override void Start() {
            Log.Debug("FloatingButton.Start() is called.");
            base.Start();
            name = nameof(FloatingButton);
            absolutePosition = new Vector3(SavedX, SavedY);
            size = new Vector2(40, 40);

            SetupSprites();
            SetupDrag();
            // m_TooltipBox = GetUIView()?.defaultTooltipBox; // Set up the tooltip

            canFocus = false;
            Show();
            Invalidate();

            Instance = this;
            started_ = true;
            Log.Debug("FloatingButton.Start() done!");
        }

        public override void OnDestroy() {
            Log.Debug("FloatingButton.OnDestroy() called!");
            Hide();
            base.OnDestroy();
        }

        public UITextureAtlas SetupSprites() {
            string[] spriteNames = new string[] { IconPressed, IconHovered, IconNormal };
            var atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas) {
                atlas = TextureUtil.CreateTextureAtlas("A.png", AtlasName, SIZE, SIZE, spriteNames);
            }
            Log.Debug("atlas name is: " + atlas.name);
            this.atlas = atlas;
            UseDeactiveSprites();
            return atlas;
        }

        public void SetupDrag() {
            var dragHandler = new GameObject("UnitedUI_FloatingButton_DragHandler");
            dragHandler.transform.parent = transform;
            dragHandler.transform.localPosition = Vector3.zero;
            drag_ = dragHandler.AddComponent<UIDragHandle>();

            drag_.width = width;
            drag_.height = height;
            drag_.enabled =  SavedDraggable;
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

        public void Open() {
            Log.Debug("FloatingButton.Open() called");
            IsActive = true;
            MainPanel.Instance.Open();
        }

        public void Close() {
            Log.Debug("FloatingButton.Close() called");
            IsActive = false;
            MainPanel.Instance.Close();
        }

        public void Toggle() {
            if (IsActive) Close();
            else Open();
        }

        bool moving_ = false;
        protected override void OnMouseUp(UIMouseEventParameter p) {
            Log.Debug("FloatingButton.OnMouseUp() called");
            base.OnMouseUp(p);
            moving_ = false; // OnMouseUp is called after onclick.
        }

        protected override void OnClick(UIMouseEventParameter p) {
            Log.Debug("FloatingButton.OnClick() called");
            base.OnClick(p);
            if (!moving_)
                Toggle();
        }

        protected override void OnDoubleClick(UIMouseEventParameter p) {
            Log.Debug("FloatingButton.OnDoubleClick() called");
            base.OnDoubleClick(p);
            IsDragable = !IsDragable;
            if (!moving_)
                Toggle();
        }

        protected override void OnPositionChanged() {
            base.OnPositionChanged();
            Log.Debug("OnPositionChanged called");
            if (!started_) return;
            moving_ = true;

            Vector2 resolution = GetUIView().GetScreenResolution();

            absolutePosition = new Vector2(
                Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            Vector2 delta = new Vector2(absolutePosition.x - SavedX, absolutePosition.y - SavedY);
            // TODO move main panel by delta.

            SavedX.value = absolutePosition.x;
            SavedY.value = absolutePosition.y;
            Log.Debug("absolutePosition: " + absolutePosition);
        }

    }
}
