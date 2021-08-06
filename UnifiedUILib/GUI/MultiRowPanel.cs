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
            new SavedBool("Seperator", MainPanel.FileName, true, true);
        public readonly static SavedInt Cols =
            new SavedInt("Cols", MainPanel.FileName, 10, true);

        public override void Awake() {
            try {
                base.Awake();
                backgroundSprite = "GenericPanelWhite";
                color = new Color32(170, 170, 170, byte.MaxValue);
                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Vertical;
                autoFitChildrenHorizontally = autoFitChildrenVertically = true;
            } catch (Exception ex) { ex.Log(); }
        }

        public Hashtable ButtomGroups = new Hashtable();
        public string GroupName(UIComponent button) => (string)ButtomGroups[button];

        public void AddButton(UIComponent c, string groupName) {
            try {
                ButtomGroups[c] = groupName;
                Arrange();
            } catch(Exception ex) { ex.Log(); }
        }

        public void Arrange() {
            var sortedButtoms = ButtomGroups.Keys.Cast<UIComponent>().OrderBy(GroupName);
            var oldRows = Rows;

            foreach (var item in sortedButtoms) {
                var row = AddRowPanel();
                string prevGroup = null;
                for (int col = 0; col < Cols; ++col) {
                    string groupName = (string)ButtomGroups[item];
                    if (GroupSeperator && prevGroup != groupName) {
                        AddSeperator(row);
                    }
                    prevGroup = groupName;
                    row.AttachUIComponent(item.gameObject);
                }
            }

            foreach (var row in oldRows)
                DestroyImmediate(row.gameObject);
        }

        public UIPanel[] Rows => GetComponents<UIPanel>().Where(item=>item.name == "row").ToArray();

        public UIPanel AddRowPanel() {
            var panel = AddUIComponent<UIPanel>();
            panel.autoLayout = true;
            panel.autoLayoutDirection = LayoutDirection.Horizontal;
            panel.autoFitChildrenHorizontally = autoFitChildrenVertically = true;
            panel.name = "row";
            return panel;
        }

        public UIComponent AddSeperator(UIComponent parent) {
            var panel = parent.AddUIComponent<UIPanel>();
            panel.size = new Vector2(10, 40);
            return panel;
        }
    }
}
