namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using static KianCommons.ReflectionHelpers;
    using KianCommons;
    using UnityEngine;
    using ColossalFramework;
    using KianCommons.UI;
    using KianCommons.Plugins;

    public class FloatingButton : ButtonBase {
        public static readonly SavedFloat SavedX = new SavedFloat(
            "ButtonX", MainPanel.FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "ButtonY", MainPanel.FileName, 100, true);

        string spritesFile = "uui.png";

        const string UNLOCK_RING_SPRITE_NAME = "UnlockRing";

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
                    unlockRing_.spriteName = value ? UNLOCK_RING_SPRITE_NAME : ""; // instead of isVisible I do this to show tooltip.
                }
            }
        }

        bool started_ = false;
        public override void Start() {
            LogCalled();
            base.Start();
            SetupAtlas();
            AddUnlockRing();
            SetupDrag();
            started_ = true;
            Refresh();
            LoadPosition();
        }

        void SetupAtlas() {
            string []names = new string[] { BG_NORMAL, BG_HOVERED, BG_PRESSED, BG_DISABLED, UNLOCK_RING_SPRITE_NAME};
            atlas = GetOrCreateAtlas(SuggestedAtlasName, spritesFile, true, names);
        }

        public void AddUnlockRing() {
            unlockRing_ = AddUIComponent<UISprite>();
            unlockRing_.name = "unlock ring";
            unlockRing_.atlas = atlas;
            unlockRing_.relativePosition = default;
            unlockRing_.size = size;
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

        protected override void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution) {
            base.OnResolutionChanged(previousResolution, currentResolution);
            LoadPosition();
        }
        #endregion
    }
}
