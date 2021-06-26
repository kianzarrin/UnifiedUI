namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using static KianCommons.ReflectionHelpers;
    using KianCommons;
    using UnityEngine;
    using ColossalFramework;

    public class FloatingButton : ButtonBase {
        public static FloatingButton Instance { get; private set; }
        public static readonly SavedFloat SavedX = new SavedFloat(
            "ButtonX", MainPanel.FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "ButtonY", MainPanel.FileName, 100, true);
        public static readonly SavedBool SavedDraggable = new SavedBool(
            "ButtonDraggable", MainPanel.FileName, def:false, true);

        public override string SpritesFile => "uui.png";
        public override bool EmbededSprite => true;

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
            LogCalled();
            base.Start();
            Instance = this;
            SetupDrag();
            started_ = true;
            LoadPosition();
        }

        public void SetupDrag() {
            var dragHandler = new GameObject("UnifiedUI_FloatingButton_DragHandler");
            dragHandler.transform.parent = transform;
            dragHandler.transform.localPosition = Vector3.zero;
            drag_ = dragHandler.AddComponent<UIDragHandle>();

            drag_.width = width;
            drag_.height = height;
            drag_.enabled =  SavedDraggable;
            drag_.eventMouseUp += Drag_eventMouseUp; 
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

        protected override void OnDoubleClick(UIMouseEventParameter p) {
            LogCalled();
            base.OnDoubleClick(p);
            IsDragable = !IsDragable;
            if (!moving_)
                Toggle();
        }


        #region pos
        public bool Responsive => MainPanel.InLoadedGame && started_;
        private void Drag_eventMouseUp(UIComponent component, UIMouseEventParameter eventParam) {
            if (!Responsive) return;
            LogCalled();
            FitToScreen();

            SavedX.value = absolutePosition.x;
            SavedY.value = absolutePosition.y;
            Log.Info("absolutePosition: " + absolutePosition, copyToGameLog: false);
        }
        void LoadPosition() {
            absolutePosition = new Vector3(SavedX, SavedY);
            FitToScreen();
        }
        void FitToScreen() {
            Vector2 resolution = GetUIView().GetScreenResolution();
            absolutePosition = new Vector2(
                Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));
        }

        protected override void OnPositionChanged() {
            base.OnPositionChanged();
            if (!Responsive) return;

            Vector2 delta = new Vector2(absolutePosition.x - SavedX, absolutePosition.y - SavedY);
            moving_ = delta.sqrMagnitude > 1f;
            // TODO move main panel by delta.
        }
        #endregion
    }
}
