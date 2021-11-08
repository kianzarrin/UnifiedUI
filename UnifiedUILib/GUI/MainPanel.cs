namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;

    public class MainPanel : UIPanel {
        public class ContainerPanel : UIPanel {
            public override void Awake() {
                try {
                    base.Awake();
                    autoLayout = true;
                    autoLayoutDirection = LayoutDirection.Horizontal;
                    autoLayoutPadding = default;
                    padding = new RectOffset(2, 2, 2, 2);
                    //autoSize = autoFitChildrenHorizontally = autoFitChildrenVertically = true;
                } catch (Exception ex) { ex.Log(); }
            }

            public void FitToChildrenWithPadding() {
                FitChildrenHorizontally(2);
                FitChildrenVertically(2);
            }
        }


        public const string FileName = "UnifiedUI";

        static MainPanel() {
            // Creating setting file - from SamsamTS
            if (GameSettings.FindSettingsFileByName(FileName) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FileName } });
            }
        }

        const string SPRITES_FILE_NAME = "MainPanel.png";
        const string DEFAULT_GROUP = "group1";

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

        public MultiRowPanel MultiRowPanel;
        FloatingButton floatingButton_;

        public void DoRefreshButtons() => EventRefreshButtons?.Invoke();

        private UILabel lblCaption_;
        private UIDragHandle dragHandle_;
        ContainerPanel containerPanel_;

        public List<ButtonBase> ModButtons;

        public UITextureAtlas MainAtlas;


        bool started_ = false;
        bool opening_;
        #region Instantiation

        static MainPanel instance_;
        public static MainPanel Instance =>
            instance_ ??= UIView.GetAView().AddUIComponent(typeof(MainPanel)) as MainPanel;

        public static MainPanel RowInstance_ => instance_;

        public static bool Exists => instance_;

        public static void Ensure() => _ = Instance;

        public static void Release() => DestroyImmediate(instance_?.gameObject);

        #endregion Instantiation

        public override void Awake() {
            try {
                base.Awake();
                MainAtlas = ButtonBase.CreateMainAtlas();

                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Vertical;
                autoSize = autoFitChildrenHorizontally = autoFitChildrenVertically = true;

                instance_ = this;
                ModButtons = new List<ButtonBase>();
                builtinKeyNavigation = true;
                floatingButton_ = UIView.GetAView().AddUIComponent(typeof(FloatingButton)) as FloatingButton;
                MultiRowPanel = AddUIComponent<MultiRowPanel>();
            } catch (Exception ex) { ex.Log(); }
        }

        public override void OnDestroy() {
            Destroy(MainAtlas);
            this.SetAllDeclaredFieldsToNull();
            instance_ = null;
            DestroyImmediate(floatingButton_?.gameObject);
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
                    lblCaption_.padding.left = 2;
                }

                containerPanel_ = AddUIComponent<ContainerPanel>();
                Assertion.Assert(MultiRowPanel, "multiRowPanel_");
                containerPanel_.AttachUIComponent(MultiRowPanel.gameObject);


                //var panelw = containerPanel_.AddUIComponent<UIPanel>();
                //panelw.name = "horizontal final padding hack";
                //panelw.size = new Vector2(1, 1);

                //var panelh = AddUIComponent<UIPanel>();
                //panelh.name = "vertical final padding hack";
                //panelh.size = new Vector2(1, 2);

                started_ = true;
                Refresh();
                isVisible = false;
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public ExternalButton Register(
            string name, string groupName, string tooltip, string spritefile = null) {
            try {
                Log.Called(name, groupName, tooltip, spritefile);
                var c = ExternalButton.Create(
                    parent: MultiRowPanel,
                    name: name,
                    tooltip: tooltip,
                    spritesFile: spritefile);
                MultiRowPanel.AddButton(c, groupName);
                ModButtons.Add(c);
                return c;
            } catch (Exception ex) { ex.Log(); }
            return null;
        }

        public void AttachAlien(UIComponent alien, string groupName = DEFAULT_GROUP) {
            try {
                Assertion.NotNull(alien);
                alien.size = new Vector2(40, 40);
                MultiRowPanel.AttachUIComponent(alien.gameObject);
                MultiRowPanel.AddButton(alien, groupName);
            } catch (Exception ex) { ex.Log(); }
        }

        public ButtonT AddButton<ButtonT>(string groupName = DEFAULT_GROUP) where ButtonT : ButtonBase {
            try {
                var button = MultiRowPanel.AddUIComponent<ButtonT>();
                MultiRowPanel.AddButton(button, groupName);
                ModButtons.Add(button);
                return button;
            } catch (Exception ex) { ex.Log(); }
            return null;
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

        public void Open() {
            try {
                Log.Info(ThisMethod + " called started_=" + started_);
                if (!started_)
                    return;
                opening_ = true;
                Show();
                MultiRowPanel.Arrange();
                Refresh();
            } catch (Exception ex) { ex.Log(); }
            opening_ = false;
        }

        public void Close() {
            try {
                Log.Info(ThisMethod + " called.");
                Hide();
            } catch (Exception ex) { ex.Log(); }

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
            try {
                Log.Called();
                if (!Responsive) return;

                int visibleButtons = ModButtons.Count(_b => _b && _b.isVisibleSelf);
                Log.Info("Visible buttons = " + visibleButtons);
                floatingButton_.isVisible = visibleButtons > 0;
                isVisible &= visibleButtons > 0;
                if (visibleButtons == 0)
                    return;

                if (MultiRowPanel.Cols == 1 || visibleButtons <= 1)
                    lblCaption_.text = "UUI";
                else
                    lblCaption_.text = "UnifiedUI";

                if (ControlToDrag)
                    dragHandle_.tooltip = lblCaption_.tooltip = "hold CTRL to move";
                else
                    dragHandle_.tooltip = lblCaption_.tooltip = "";

                floatingButton_?.Refresh();
                DoRefreshButtons();
                dragHandle_.width = lblCaption_.width; // minimum width
                containerPanel_?.FitToChildrenWithPadding();
                RefreshDragAndCaptionPos();
                LoadPosition();
                //Invalidate();
            } catch (Exception ex) { ex.Log(); }
        }

        bool IsOpen => isVisible && !opening_;
        public void RearrangeIfOpen() {
            int visibleButtons = ModButtons.Count(_b => _b && _b.isVisibleSelf);
            isVisible &= floatingButton_.isVisible = visibleButtons > 0;
            if (IsOpen) {
                MultiRowPanel.Arrange();
                Refresh();
            }
        }

        void RefreshDragAndCaptionPos() {
            dragHandle_.width = Mathf.Max(this.width, lblCaption_.width);
            lblCaption_.relativePosition = new Vector2((width - lblCaption_.width) * 0.5f, 3);
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
            } catch (Exception e) {
                Log.Exception(e);
            }
        }

        #region Handle Tool Changed
        public delegate void ToolChangedHandler(ToolBase newTool);
        public event ToolChangedHandler EventToolChanged;
        ToolBase prevTool;
        void CaptureToolChanged() {
            var currentTool = ToolsModifierControl.toolController.CurrentTool;
            if (!currentTool) return;  // tool is being destroyed. do not poke around!
            if (!currentTool.enabled)
                Log.DebugWait($"WARNING: currentTool({currentTool}) is disabled", seconds: 1f);
            if (currentTool != prevTool) {
                if (EventToolChanged == null)
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
            if (UIView.HasModalInput() || UIView.HasInputFocus())
                return;

            if (ModButtons.Any(b => b && b.AvoidCollision())) {
                Log.Info("Active Key pressed");
                return;
            }

            if (CustomActiveHotkeys.Any(pair => pair.Value != null && pair.Value.Invoke() && pair.Key.IsKeyUp())) {
                Log.Info("Active Key pressed");
                return;
            }

            foreach (var button in ModButtons) {
                if (button && button.HandleHotKey())
                    return;
            }

            foreach (var pair in CustomHotkeys) {
                if (pair.Key.IsKeyUp()) {
                    pair.Value?.Invoke();
                    return;
                }
            }
        }
        #endregion
    }
}

