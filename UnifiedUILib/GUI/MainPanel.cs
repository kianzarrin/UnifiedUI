namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using static KianCommons.ReflectionHelpers;
    using KianCommons.Plugins;
    public class MainPanel : UIPanel {
        const string SPRITES_FILE_NAME = "MainPanel.png";
        const string DEFAULT_GROUP = "group1";
        public const string FileName = "UnifiedUI";

        public string AtlasName => $"{GetType().FullName}_rev" + this.VersionOf();
        public static readonly SavedFloat SavedX = new SavedFloat(
            "PanelX", FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "PanelY", FileName, 150, true);
        public readonly static SavedBool ControlToDrag =
            new SavedBool("ControlToDrag", FileName, false, true);


        public static SavedBool SwitchToPrevTool = new SavedBool("SwitchToPrevTool", FileName, true, true);
        public static SavedBool ClearInfoPanelsOnToolChanged = new SavedBool("ClearInfoPanelsOnToolChanged", FileName, false, true);

        public event Action EventRefreshButtons;

        HashSet<string> groups_ = new HashSet<string>();

        public void DoRefreshButtons() => EventRefreshButtons?.Invoke();

        private UILabel lblCaption_;
        private UIDragHandle dragHandle_;
        UIPanel containerPanel_;

        public List<ButtonBase> ModButtons;

        bool started_ = false;
        #region Instanciation

        static MainPanel instance_;
        public static MainPanel Instance => 
            instance_ ??= UIView.GetAView().AddUIComponent(typeof(MainPanel)) as MainPanel;

        public static MainPanel RowInstance_ => instance_;

        public static bool Exists => instance_;

        public static void Ensure() => _ = Instance;

        public static void Release() => DestroyImmediate(instance_?.gameObject);

        #endregion Instanciation

        public override void Awake() {
            base.Awake();
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoSize = autoFitChildrenHorizontally = autoFitChildrenVertically = true;
            instance_ = this;
            ModButtons = new List<ButtonBase>();
            builtinKeyNavigation = true;
            UIView.GetAView().AddUIComponent(typeof(FloatingButton));
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            instance_ = null;
            DestroyImmediate(FloatingButton.Instance?.gameObject);
            base.OnDestroy();
        }

        public override void Start() {
            try {
                base.Start();
                LogCalled();
                name = "MainPanel";

                SetupSprites();

                {
                    dragHandle_ = AddUIComponent<ControlledDrag>();
                    dragHandle_.height = 20;
                    dragHandle_.relativePosition = Vector3.zero;
                    dragHandle_.eventMouseUp += DragHandle__eventMouseUp;

                    lblCaption_ = dragHandle_.AddUIComponent<UILabel>();
                    lblCaption_.text = "UnifiedUI";
                    lblCaption_.name = "UnifiedUI_title";
                    lblCaption_.textScale = 0.75f;
                    lblCaption_.padding.top = 4;    
                }

                var body = AddPanel();
                body.autoLayoutPadding = new RectOffset(2, 0, 2, 2);
                containerPanel_ = body;

                foreach(string groupName in groups_)
                {
                    var group = Find<UIPanel>(groupName);
                    if (group == null)
                        group = AddGroup(body, groupName);
                    else
                        body.AttachUIComponent(group.gameObject);
                }

                isVisible = false;
                started_ = true;
                Refresh();
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        public ExternalButton Register(
            string name, string groupName, string tooltip, string spritefile = null) {
            var g = GetOrCreateGroup(groupName);
            var c = ExternalButton.Create(
                parent: g,
                name: name,
                tooltip: tooltip,
                spritesFile: spritefile);
            ModButtons.Add(c);
            return c;
        }

        public void AttachAlien(UIComponent alien, string groupName = null) {
            Assertion.NotNull(alien);
            alien.size = new Vector2(40, 40);
            var g = GetOrCreateGroup(groupName);
            g.AttachUIComponent(alien.gameObject);
        }

        public ButtonT AddButton<ButtonT>(string groupName = DEFAULT_GROUP)  where ButtonT: ButtonBase {
            var g = GetOrCreateGroup(groupName);
            var button = g.AddUIComponent<ButtonT>();
            ModButtons.Add(button);
            return button;
        }

        public UITextureAtlas SetupSprites() {
            string[] spriteNames = new string[] { "background" };
            var _atlas = TextureUtil.GetAtlas(AtlasName);
            if (_atlas == UIView.GetAView().defaultAtlas) {
                var texture = TextureUtil.GetTextureFromAssemblyManifest(SPRITES_FILE_NAME);
                _atlas = TextureUtil.CreateTextureAtlas(texture, AtlasName, spriteNames);
                _atlas.sprites[0].border = new RectOffset(8, 8, 13, 8);
            }

            Log.Debug("atlas name is: " + _atlas.name, false);
            atlas = _atlas;
            backgroundSprite = "background";
            return _atlas;
        }

        UIPanel GetOrCreateGroup(string groupName = null) {
            if (groupName.IsNullOrWhiteSpace()) groupName = DEFAULT_GROUP;
                    return Find<UIPanel>(groupName) ?? AddGroup(groupName);
        }

        UIPanel AddGroup(string name) {
            return AddGroup(containerPanel_ ?? this, name ?? DEFAULT_GROUP);
        }

        UIPanel AddGroup(UIPanel parent, string name) {
            var g = AddPanel(parent);
            g.backgroundSprite = "GenericPanelWhite";
            g.color = new Color32(170, 170, 170, byte.MaxValue);
            g.name = name;
            groups_.Add(name);
            return g;
        }

        UIPanel AddPanel() => AddPanel(this);

        static UIPanel AddPanel(UIPanel panel) {
            Assertion.AssertNotNull(panel, "panel");
            UIPanel newPanel = panel.AddUIComponent<UIPanel>();
            Assertion.AssertNotNull(newPanel, "newPanel");
            newPanel.autoLayout = true;
            newPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            newPanel.autoFitChildrenHorizontally = newPanel.autoFitChildrenVertically = true;

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

        #region pos
        private void DragHandle__eventMouseUp(UIComponent component, UIMouseEventParameter eventParam) {
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

        protected override void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution) {
            base.OnResolutionChanged(previousResolution, currentResolution);
            LoadPosition();
        }
        #endregion

        public void Refresh() {
            if (!Responsive) return;
            if(ControlToDrag)
                dragHandle_.tooltip = lblCaption_.tooltip = "hold CTRL to move";
            else
                dragHandle_.tooltip = lblCaption_.tooltip = "";

            FloatingButton.Instance?.Refresh();
            DoRefreshButtons();
            dragHandle_.width = Mathf.Max(this.width, lblCaption_.width);
            lblCaption_.relativePosition = new Vector2((width - lblCaption_.width) * 0.5f, 3);
            LoadPosition();
            Invalidate();
        }

        public static bool InLoadedGame =>
            !LoadingManager.instance.m_applicationQuitting &&
            !LoadingManager.instance.m_currentlyLoading &&
            LoadingManager.instance.m_loadingComplete;

        public bool Responsive => InLoadedGame && started_;


        //[FPSBoosterSkipOptimizations]
        public override void Update() {
            base.Update();
            isVisible = isVisible; // FPSBooster workaround
            if (!Responsive) return;
            try {
                HandleHotkeys();
                CaptureToolChanged();
            } catch(Exception e) {
                Log.Exception(e);
            }
        }

        #region Handle Tool Changed
        public delegate void ToolChangedHandler(ToolBase newTool);
        public event ToolChangedHandler EventToolChanged;
        ToolBase prevTool;
        void CaptureToolChanged() {
            var currentTool = ToolsModifierControl.toolController.CurrentTool;
            if(!currentTool) return;  // tool is being destroyed. do not poke around!
            if(!currentTool.enabled)
                Log.DebugWait($"WARNING: currentTool({currentTool}) is disabled", seconds: 1f);
            if(currentTool != prevTool) {
                if(EventToolChanged == null)
                    Log.Info("WARNING: EventToolChanged==null");
                //Log.Debug($"ThreadingExtension.OnUpdate(): invoking EventToolChanged. currentTool={currentTool} prevTool={prevTool}");
                prevTool = currentTool;
                EventToolChanged?.Invoke(currentTool);
            }
        }
        #endregion

        #region Hotkeys

        public Dictionary<SavedInputKey, Action> CustomHotkeys = new Dictionary<SavedInputKey, Action>();
        public Dictionary<SavedInputKey, Func<bool>> CustomActiveHotkeys = new Dictionary<SavedInputKey, Func<bool>>();

        static UIComponent PauseMenu => UIView.library.Get("PauseMenu");
        static bool ToolIsDefault => ToolsModifierControl.toolController.CurrentTool == ToolsModifierControl.GetTool<DefaultTool>();

        public void HandleHotkeys() {
            if(UIView.HasModalInput() || UIView.HasInputFocus())
                return;

            if (ModButtons.Any(b => b.AvoidCollision())) {
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

        