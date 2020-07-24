namespace UnitedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using GUI.ModButtons;
    using Util;
    using PluginUtil = Util.PluginUtil;

    public class MainPanel : UIAutoSizePanel {
        public static readonly SavedFloat SavedX = new SavedFloat(
            "PanelX", Settings.FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "PanelY", Settings.FileName, 0, true);
        public static readonly SavedBool SavedDraggable = new SavedBool(
            "PanelDraggable", Settings.FileName, def: false, true);

        #region Instanciation
        public static MainPanel Instance { get; private set; }

        public static MainPanel Create() {
            var uiView = UIView.GetAView();
            MainPanel panel = uiView.AddUIComponent(typeof(MainPanel)) as MainPanel;
            return panel;
        }

        public static void Release() {
            Destroy(Instance);
        }

        #endregion Instanciation

        public override void Awake() {
            base.Awake();
            Instance = this;
        }

        private UILabel lblCaption_;
        private UIDragHandle dragHandle_;
        UIAutoSizePanel containerPanel_;

        bool started_ = false;
        public override void Start() {
            base.Start();
            Log.Debug("MainPanel started");

            //width = 150;
            name = "MainPanel";
            backgroundSprite = "MenuPanel2";
            absolutePosition = new Vector3(SavedX, SavedY);

            {
                dragHandle_ = AddUIComponent<UIDragHandle>();
                dragHandle_.height = 42;
                dragHandle_.relativePosition = Vector3.zero;
                dragHandle_.target = parent;

                lblCaption_ = dragHandle_.AddUIComponent<UILabel>();
                lblCaption_.text = "UnitedUI";
                lblCaption_.name = "UnitedUI_title";
                lblCaption_.textScale = .7f;

            }

            var body = AddPanel();
            body.autoLayoutPadding = new RectOffset(5, 5, 5, 5);

            var g1  = AddPanel();

            {
                var panel = AddPanel(g1);
                panel.backgroundSprite = "GenericPanelWhite";

                if (PluginUtil.Instance.NetworkDetective.IsActive)
                    panel.AddUIComponent<NetworkDetectiveButton>();

                if (PluginUtil.Instance.IntersectionMarking.IsActive)
                    panel.AddUIComponent<IntersectionMarkingButton>();

                if (PluginUtil.Instance.RoundaboutBuilder.IsActive)
                    panel.AddUIComponent<RoundaboutBuilderButton>();

                if (PluginUtil.Instance.PedestrianBridge.IsActive)
                    panel.AddUIComponent<PedestrianBridgeButton>();

                if (PluginUtil.Instance.NodeController.IsActive)
                    panel.AddUIComponent<NodeControllerButton>();

            }


            isVisible = false;
            started_ = true;
            Refresh();
        }

        UIAutoSizePanel AddPanel() => AddPanel(this);

        static UIAutoSizePanel AddPanel(UIPanel panel) {
            HelpersExtensions.AssertNotNull(panel, "panel");
            int pad_horizontal = 0;
            int pad_vertical = 0;
            UIAutoSizePanel newPanel = panel.AddUIComponent<UIAutoSizePanel>();
            HelpersExtensions.AssertNotNull(newPanel, "newPanel");
            newPanel.autoLayoutDirection = LayoutDirection.Vertical;
            newPanel.autoLayoutPadding =
                new RectOffset(pad_horizontal, pad_horizontal, pad_vertical, pad_vertical);
            return newPanel;
        }

        static UIPanel AddSpacePanel(UIPanel panel, int space) {
            panel = panel.AddUIComponent<UIPanel>();
            panel.width = panel.width;
            panel.height = space;
            return panel;
        }

        public void Open() {
            Log.Debug("MainPanel.Open() called started_="+started_);
            if (!started_)
                return;
            Show();
            Refresh();
        }

        public void Close() {
            Log.Debug("MainPanel.Close() called");
            Hide();
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

        public static void GoToInstance(InstanceID instanceID) {
            Vector3 pos = instanceID.Type switch
            {
                InstanceType.NetNode => instanceID.NetNode.ToNode().m_position,
                InstanceType.NetSegment => instanceID.NetSegment.ToSegment().m_middlePosition,
                _ => throw new NotImplementedException("instanceID.Type:" + instanceID.Type),
            };
            pos.y = Camera.main.transform.position.y;
            ToolsModifierControl.cameraController.SetTarget(instanceID, pos, true);
        }

        void Refresh() {
            RefreshButtons();
            //Invalidate();
            dragHandle_.width = lblCaption_.width;
            RefreshSizeRecursive();
            dragHandle_.width = width;
            lblCaption_.relativePosition = new Vector2((width - lblCaption_.width) * 0.5f, 14);
            dragHandle_.width = width;
            Invalidate();
        }

        void RefreshButtons() {
            // uncomment code bellow to support hot reload. start code also needs change.
            //if(NetworkDetectiveButton.Instance)
            //    NetworkDetectiveButton.Instance.isVisible = PluginUtil.Instance.NetworkDetective.IsActive;
            //if(NodeControllerButton.Instance)
            //    NodeControllerButton.Instance.isVisible = PluginUtil.Instance.NodeController.IsActive;
            //if (PedestrianBridgeButton.Instance)
            //    NetworkDetectiveButton.Instance.isVisible = PluginUtil.Instance.PedestrianBridge.IsActive;
            //if (IntersectionMarkingButton.Instance)
            //    NetworkDetectiveButton.Instance.isVisible = PluginUtil.Instance.IntersectionMarkup.IsActive;
            //if (RoundaboutBuilderButton.Instance)
            //    NetworkDetectiveButton.Instance.isVisible = PluginUtil.Instance.RoundaboutBuilder.IsActive;

        }
    }
}

