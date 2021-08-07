namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
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

                int col = 0;
                string prevGroup = null;
                UIPanel row = null;
                foreach (var item in sortedButtoms) {
                    if (col == 0) row = AddRowPanel();

                    string groupName = (string)ButtomGroups[item];
                    if (col != 0 && GroupSeperator && prevGroup != groupName) {
                        AddSeperator(row);
                    }
                    prevGroup = groupName;
                    row.AttachUIComponent(item.gameObject);
                    col = (++col) % Cols;
                }

                DeleteEmptyRows();

                FitChildren();
            } catch(Exception ex) { ex.Log(); }
        }

        const string ROW_NAME = "UUI_row";
        const string SEPERATOR_NAME = "UUI_seperator";
        public IEnumerable<UIPanel> Rows => GetComponentsInChildren<UIPanel>().Where(item => item.name == ROW_NAME);
        public IEnumerable<UIPanel> EmptyRows => Rows.Where(IsRowEmpty);

        static bool IsRowEmpty(UIPanel row) {
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

        public UIPanel AddRowPanel() {
            Log.Called();
            var panel = AddUIComponent<UIPanel>();
            panel.autoLayout = true;
            panel.autoLayoutDirection = LayoutDirection.Horizontal;
            panel.autoFitChildrenHorizontally = panel.autoFitChildrenVertically = true;
            panel.name = ROW_NAME;
            return panel;
        }

        public UIComponent AddSeperator(UIComponent parent) {
            Log.Called();
            var panel = parent.AddUIComponent<UIPanel>();
            panel.size = new Vector2(10, 40);
            panel.name = SEPERATOR_NAME;
            return panel;
        }
    }
}
