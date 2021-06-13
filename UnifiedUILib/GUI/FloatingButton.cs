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
            absolutePosition = new Vector3(SavedX, SavedY);
            SetupDrag();
            started_ = true;
        }

        public void SetupDrag() {
            var dragHandler = new GameObject("UnifiedUI_FloatingButton_DragHandler");
            dragHandler.transform.parent = transform;
            dragHandler.transform.localPosition = Vector3.zero;
            drag_ = dragHandler.AddComponent<UIDragHandle>();

            drag_.width = width;
            drag_.height = height;
            drag_.enabled =  SavedDraggable;
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

        protected override void OnPositionChanged() {
            base.OnPositionChanged();
            Log.DebugWait(ThisMethod + " called",
                id: "OnPositionChanged called".GetHashCode() , seconds:0.2f,copyToGameLog:false);
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
            Log.DebugWait(message: "absolutePosition: " + absolutePosition,
                id: "absolutePosition: ".GetHashCode(), seconds: 0.2f, copyToGameLog: false);
        }

        protected override void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution) => OnPositionChanged();
    }
}
