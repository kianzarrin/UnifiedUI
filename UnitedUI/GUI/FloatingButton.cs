namespace UnitedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons.UI;
    using KianCommons;
    using UnityEngine;
    using ColossalFramework;

    public class FloatingButton : ButtonBase {
        public static FloatingButton Instance { get; private set; }
        public static readonly SavedFloat SavedX = new SavedFloat(
            "ButtonX", Settings.FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "ButtonY", Settings.FileName, 0, true);
        public static readonly SavedBool SavedDraggable = new SavedBool(
            "ButtonDraggable", Settings.FileName, def:false, true);

        public override string SpritesFileName => "uui.png";

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
            Instance = this;
            absolutePosition = new Vector3(SavedX, SavedY);
            SetupDrag();
            started_ = true;
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
        protected override void OnMouseDown(UIMouseEventParameter p) {
            base.OnMouseDown(p);
            moving_ = false;
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

            Vector2 resolution = GetUIView().GetScreenResolution();

            absolutePosition = new Vector2(
                Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            Vector2 delta = new Vector2(absolutePosition.x - SavedX, absolutePosition.y - SavedY);
            moving_ = delta.sqrMagnitude > 1f;
            // TODO move main panel by delta.

            SavedX.value = absolutePosition.x;
            SavedY.value = absolutePosition.y;
            Log.Debug("absolutePosition: " + absolutePosition);
        }

    }
}
