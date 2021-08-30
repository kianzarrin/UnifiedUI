namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using JetBrains.Annotations;
    using KianCommons;
    using KianCommons.IImplict;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class MultiRowPanel : UIPanel {
        public readonly static SavedBool GroupSeperator =
            new SavedBool("Seperator", MainPanel.FileName, false, true);
        public readonly static SavedInt Cols =
            new SavedInt("Cols", MainPanel.FileName, 10, true);

        public override void Awake() {
            try {
                Log.Called();
                base.Awake();
                backgroundSprite = "GenericPanelWhite";
                color = new Color32(170, 170, 170, byte.MaxValue);
                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Vertical;
                padding = autoLayoutPadding = new RectOffset();
            } catch (Exception ex) { ex.Log(); }
        }

        bool started_ = false;
        public override void Start() {
            try {
                Log.Called();
                base.Start();
                Arrange();
                Invalidate();
            } catch (Exception ex) { ex.Log(); }
            started_ = true;
        }

        public override void OnDestroy() {
            Log.Called();
            base.OnDestroy();
        }


        public Hashtable ButtomGroups = new Hashtable();
        public string GroupName(UIComponent button) => (string)ButtomGroups[button];

        public void AddButton(UIComponent c, string groupName) {
            try {
                groupName ??= "group1";
                ButtomGroups[c] = groupName;
                if (started_)
                    Arrange();
            } catch(Exception ex) { ex.Log(); }
        }

        public void Arrange() {
            try {
                Log.Called();
                var sortedButtoms = ButtomGroups.Keys.Cast<UIComponent>().OrderBy(GroupName);

                autoFitChildrenHorizontally = autoFitChildrenVertically = false;
                int col = 0;
                string prevGroup = null;
                UIPanel row = null;
                foreach (var item in sortedButtoms) {
                    if (col == 0) row = AddUIComponent<RowPanel>();

                    string groupName = (string)ButtomGroups[item];
                    if (col != 0 && GroupSeperator && prevGroup != groupName) {
                        AddSeperator(row);
                    }
                    prevGroup = groupName;
                    row.AttachUIComponent(item.gameObject);

                    if(item.isVisibleSelf)
                        col = (++col) % Cols;
                }

                DeleteEmptyRows();
                autoFitChildrenHorizontally = autoFitChildrenVertically = true;
            } catch (Exception ex) { ex.Log(); }
        }

        const string ROW_NAME = "UUI_row";
        const string SEPERATOR_NAME = "UUI_seperator";
        public IEnumerable<RowPanel> Rows => GetComponentsInChildren<RowPanel>();
        public IEnumerable<RowPanel> EmptyRows => Rows.Where(IsRowEmpty);

        static bool IsRowEmpty(RowPanel row) {
            return !row.GetComponentsInChildren<UIComponent>()
                .Where(item => item.name != ROW_NAME && item.name != SEPERATOR_NAME)
                .Any();
        }

        public void DeleteEmptyRows() {
            try {
                var emptyrows = EmptyRows.ToArray();
                Log.Info($"deleting {emptyrows.Length} empty rows (total = {Rows.Count()})");
                foreach (var row in emptyrows) {
                    RemoveUIComponent(row);
                    Destroy(row.gameObject);
                }
            } catch(Exception ex) { ex.Log(); }
        }

        public UIComponent AddSeperator(UIComponent parent) {
            Log.Called();
            var panel = parent.AddUIComponent<UIPanel>();
            panel.size = new Vector2(10, 40);
            panel.name = SEPERATOR_NAME;
            return panel;
        }

        public class RowPanel: UIPanel, IEnablablingObject {
            public override void Awake() {
                base.Awake();
                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Horizontal;
                autoFitChildrenHorizontally = autoFitChildrenVertically = true;
                name = ROW_NAME;
            }

            public override void Start() {
                base.Start();
                eventSizeChanged -= Rearrange;

                // eventSizeChanged is called very late. that is why we need 1 frame of delay.
                (this as MonoBehaviour).Invoke(nameof(ListenToEvents),0); 
            }


            [UsedImplicitly]
            public void ListenToEvents() {
                Log.Called();
                eventSizeChanged += Rearrange;
            }

            void Rearrange(UIComponent _, Vector2 __) {
                if (isVisible) {
                    MainPanel.Instance.RearrangeIfOpen();
                }
            }
        }
    }
}
