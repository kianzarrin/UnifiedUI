namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using GUI.ModButtons;
    using Util;
    using PluginUtil = Util.PluginUtil;
    using System.Collections.Generic;

    public class MainPanel : UIAutoSizePanel {
        public static readonly SavedFloat SavedX = new SavedFloat(
            "PanelX", Settings.FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "PanelY", Settings.FileName, 150, true);
        public static readonly SavedBool SavedDraggable = new SavedBool(
            "PanelDraggable", Settings.FileName, def: false, true);

        public string AtlasName => $"{GetType().FullName}_rev" + this.VersionOf();
        string spriteFileName_ = "MainPanel.png";

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
            AutoSize2 = true;
            ModButtons = new List<GenericModButton>();
        }

        private UILabel lblCaption_;
        private UIDragHandle dragHandle_;
        UIAutoSizePanel containerPanel_;

        public List<GenericModButton> ModButtons;

        bool started_ = false;
        public override void Start() {
            base.Start();
            Log.Debug("MainPanel started");
            name = "MainPanel";

            SetupSprites();
            //backgroundSprite = "MenuPanel2";

            absolutePosition = new Vector3(SavedX, SavedY);

            {
                dragHandle_ = AddUIComponent<UIDragHandle>();
                dragHandle_.height = 20;
                dragHandle_.relativePosition = Vector3.zero;
                dragHandle_.target = parent;

                lblCaption_ = dragHandle_.AddUIComponent<UILabel>();
                lblCaption_.text = "UnifiedUI";
                lblCaption_.name = "UnifiedUI_title";
                lblCaption_.textScale = 0.75f;
            }

            var body = AddPanel();
            body.autoLayoutPadding = new RectOffset(2, 0, 2, 2);

            {
                var g1  = AddPanel(body);
                g1.backgroundSprite = "GenericPanelWhite";
                g1.color = new Color32(170, 170, 170, byte.MaxValue);
                var panel = g1;
                g1.name = "group1";
            
                if (PluginUtil.Instance.NetworkDetective.IsActive)
                    ModButtons.Add(panel.AddUIComponent<NetworkDetectiveButton>());

                if (PluginUtil.Instance.IntersectionMarking.IsActive)
                    ModButtons.Add(panel.AddUIComponent<IntersectionMarkingButton>());

                if (PluginUtil.Instance.RoundaboutBuilder.IsActive)
                    ModButtons.Add(panel.AddUIComponent<RoundaboutBuilderButton>());

                if (PluginUtil.Instance.PedestrianBridge.IsActive)
                    ModButtons.Add(panel.AddUIComponent<PedestrianBridgeButton>());

                if (PluginUtil.Instance.NodeController.IsActive)
                    ModButtons.Add(panel.AddUIComponent<NodeControllerButton>());
            }

            isVisible = false;
            started_ = true;
            Refresh();
        }

        public UITextureAtlas SetupSprites() {
            string[] spriteNames = new string[] { "background" };
            var atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas) {
                atlas = TextureUtil.CreateTextureAtlas(spriteFileName_, AtlasName, spriteNames);
            }
            Log.Debug("atlas name is: " + atlas.name, false);
            this.atlas = atlas;
            atlas.sprites[0].border = new RectOffset(8, 8, 13, 8);

            backgroundSprite = "background";

            return atlas;
        }

        UIAutoSizePanel AddPanel() => AddPanel(this);

        static UIAutoSizePanel AddPanel(UIPanel panel) {
            Assertion.AssertNotNull(panel, "panel");
            int pad_horizontal = 0;
            int pad_vertical = 0;
            UIAutoSizePanel newPanel = panel.AddUIComponent<UIAutoSizePanel>();
            Assertion.AssertNotNull(newPanel, "newPanel");
            newPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            newPanel.autoLayoutPadding =
                new RectOffset(pad_horizontal, pad_horizontal, pad_vertical, pad_vertical);
            newPanel.AutoSize2 = true;

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
            Log.DebugWait("OnPositionChanged called", id: "OnPositionChanged called".GetHashCode(), seconds: 0.2f, copyToGameLog: false);

            Vector2 resolution = GetUIView().GetScreenResolution();

            absolutePosition = new Vector2(
                Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            SavedX.value = absolutePosition.x;
            SavedY.value = absolutePosition.y;
            Log.DebugWait("absolutePosition: " + absolutePosition, id: "absolutePosition: ".GetHashCode(), seconds: 0.2f, copyToGameLog: false);
        }

        void Refresh() {
            RefreshButtons();
            //Invalidate();
            dragHandle_.width = lblCaption_.width;
            RefreshSizeRecursive();

            dragHandle_.width = width;
            lblCaption_.relativePosition = new Vector2((width - lblCaption_.width) * 0.5f, 3);
            dragHandle_.width = width;
            Invalidate();
        }

        void RefreshButtons() {
            foreach (var btn in ModButtons)
                btn.HandleOriginalButton();

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

