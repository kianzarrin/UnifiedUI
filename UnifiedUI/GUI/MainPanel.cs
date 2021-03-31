namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using GUI.ModButtons;
    using PluginUtil = Util.PluginUtil;
    using System.Collections.Generic;
    using System.Linq;
    using static KianCommons.ReflectionHelpers;
    using UnifiedUI.LifeCycle;

    public class MainPanel : UIAutoSizePanel {
        public static readonly SavedFloat SavedX = new SavedFloat(
            "PanelX", UUISettings.FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "PanelY", UUISettings.FileName, 150, true);
        public static readonly SavedBool SavedDraggable = new SavedBool(
            "PanelDraggable", UUISettings.FileName, def: false, true);
        const string DEFAULT_GROUP = "group1";

        public string AtlasName => $"{GetType().FullName}_rev" + this.VersionOf();
        string spriteFileName_ = "MainPanel.png";

        private UILabel lblCaption_;
        private UIDragHandle dragHandle_;
        UIAutoSizePanel containerPanel_;

        public List<ModButtonBase> ModButtons;

        bool started_ = false;
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
            ModButtons = new List<ModButtonBase>();
            builtinKeyNavigation = true;
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            Instance = null;
            base.OnDestroy();
        }

        public override void Start() {
            base.Start();
            Log.Debug("MainPanel started");
            name = "MainPanel";

            SetupSprites();

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
            containerPanel_ = body;

            {
                var g1 = Find<UIPanel>(DEFAULT_GROUP);
                if(g1 == null)
                    g1 = AddGroup(body, DEFAULT_GROUP);
                else 
                    body.AttachUIComponent(g1.gameObject);
                var panel = g1;
            
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

        public ExternalButton Register(
            string name, string groupName, string tooltip, string spritefile) {
            var g = Find<UIPanel>(DEFAULT_GROUP);
            if(g == null)
                g = AddGroup(this, DEFAULT_GROUP);
            else
                this.AttachUIComponent(g.gameObject);

            var c = ExternalButton.Create(
                parent: g,
                name: name,
                tooltip: tooltip,
                spritesFile: spritefile);
            ModButtons.Add(c);
            return c;
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

        UIAutoSizePanel AddGroup(UIPanel parent, string name) {
            var g = AddPanel(parent);
            g.backgroundSprite = "GenericPanelWhite";
            g.color = new Color32(170, 170, 170, byte.MaxValue);
            g.name = name;
            return g;
        }

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
            Log.Info(ThisMethod + " called started_=" + started_);
            if (!started_)
                return;
            Show();
            Refresh();
        }

        public void Close() {
            Log.Info(ThisMethod + " called." + Environment.StackTrace);
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
            foreach(var btn in ModButtons) {
                if(btn is GenericModButton btn2)
                    btn2.HandleOriginalButtons();
            }
        }

        #region Hotkeys

        public Dictionary<SavedInputKey, Action> CustomHotkeys = new Dictionary<SavedInputKey, Action>();
        public Dictionary<SavedInputKey, Func<bool>> CustomActiveHotkeys = new Dictionary<SavedInputKey, Func<bool>>();

        static UIComponent PauseMenu => UIView.library.Get("PauseMenu");
        static bool ToolIsDefault => ToolsModifierControl.toolController.CurrentTool == ToolsModifierControl.GetTool<DefaultTool>();

        public void HandleHotkeys() {
            if(UIView.HasModalInput() || UIView.HasInputFocus())
                return;

            if(ModButtons.Any(b => b.AvoidCollision())) {
                Log.Info("Active Key pressed");
                return;
            }

            if(CustomActiveHotkeys.Any(pair => pair.Value != null && pair.Value.Invoke() && pair.Key.IsKeyUp())) {
                Log.Info("Active Key pressed");
                return;
            }

            foreach(var button in ModButtons) {
                if(button.HandleHotKey())
                    return;
            }

            foreach(var pair in CustomHotkeys) {
                if(pair.Key.IsKeyUp()) {
                    pair.Value?.Invoke();
                    return;
                }
            }
        }
        #endregion
    }
}

