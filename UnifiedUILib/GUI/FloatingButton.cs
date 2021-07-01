namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using static KianCommons.ReflectionHelpers;
    using KianCommons;
    using UnityEngine;
    using ColossalFramework;
    using KianCommons.UI;
    using KianCommons.Plugins;

    public class FloatingButton : ButtonBase {
        public static FloatingButton Instance { get; private set; }
        public static readonly SavedFloat SavedX = new SavedFloat(
            "ButtonX", MainPanel.FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "ButtonY", MainPanel.FileName, 100, true);

        public override string SpritesFile => "uui.png";
        public override bool EmbededSprite => true;

        const string unlockRingSpriteName_ = "unlock";

        private UIDragHandle drag_ { get; set; }

        UISprite unlockRing_;

        void SetTooltip(string text) {
            tooltip = unlockRing_.tooltip = drag_.tooltip = text;
        }

        static bool isDraggable_;
        public bool IsDragable {
            get => isDraggable_;
            set {
                if (isDraggable_ != value) {
                    isDraggable_ = value;
                    unlockRing_.spriteName = value ? unlockRingSpriteName_ : ""; // instead of isVisible I do this to show tooltip.
                }
            }
        }

        bool started_ = false;
        public override void Start() {
            LogCalled();
            base.Start();
            Instance = this;
            AddUnlockRing();
            SetupDrag();
            started_ = true;
            Refresh();
            LoadPosition();
        }

        public void AddUnlockRing() {
            string atlasName = AtlasName + "_ring";
            var _atlas = TextureUtil.GetAtlas(atlasName);
            if (_atlas == UIView.GetAView().defaultAtlas) {
                Texture2D texture2D = TextureUtil.GetTextureFromAssemblyManifest("unlock-ring.png");
                _atlas = TextureUtil.CreateTextureAtlas(texture2D, atlasName, new[] { unlockRingSpriteName_ });
            }
            unlockRing_ = AddUIComponent<UISprite>();
            unlockRing_.name = "unlock ring";
            unlockRing_.atlas = _atlas;
            unlockRing_.relativePosition = default;
        }

        public void SetupDrag() {
            var dragHandler = new GameObject("UnifiedUI_FloatingButton_DragHandler");
            dragHandler.transform.parent = transform;
            dragHandler.transform.localPosition = Vector3.zero;
            drag_ = dragHandler.AddComponent<ControlledDrag>();

            drag_.size = size;
            drag_.eventMouseUp += Drag_eventMouseUp;
        }

        public void Refresh() {
            if(MainPanel.ControlToDrag)
                SetTooltip("hold CTRL to move");
            else
                SetTooltip("");
            Invalidate();
        }

        public override void Activate() {
            LogCalled();
            base.Activate();
            MainPanel.Instance.Open();
        }

        public override void Deactivate() {
            LogCalled();
            base.Deactivate();
            MainPanel.Instance.Close();
        }

        bool moving_ = false;
        protected override void OnMouseDown(UIMouseEventParameter p) {
            base.OnMouseDown(p);
            moving_ = false;
        }

        protected override void OnClick(UIMouseEventParameter p) {
            LogCalled();
            base.OnClick(p);
            if (!moving_)
                Toggle();
        }

        [FPSBoosterSkipOptimizations]
        public override void Update() {
            base.Update();
            if (!started_) return;
            if (MainPanel.ControlToDrag)
                IsDragable = containsMouse && Helpers.ControlIsPressed;
            else
                IsDragable = true;
        }


        #region pos
        public bool Responsive => MainPanel.InLoadedGame && started_;
        private void Drag_eventMouseUp(UIComponent component, UIMouseEventParameter eventParam) {
            if (!Responsive) return;
            LogCalled();

            SavedX.value = absolutePosition.x;
            SavedY.value = absolutePosition.y;
            Log.Info("absolutePosition: " + absolutePosition, copyToGameLog: false);
        }
        void LoadPosition() {
            absolutePosition = new Vector3(SavedX, SavedY);
            this.FitToScreen();
        }

        protected override void OnPositionChanged() {
            base.OnPositionChanged();
            if (!Responsive) return;

            Vector2 delta = new Vector2(absolutePosition.x - SavedX, absolutePosition.y - SavedY);
            moving_ = delta.sqrMagnitude > 0f;
            // TODO move main panel by delta.
        }
        #endregion
    }
}
