namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using System.Collections;
    using System.Linq;
    using UnityEngine;

    public class MultiRowPanel : UIPanel {
        #region settings
        const bool GroupSeperator_DEF = false;
        const int Cols_DEF = 10;

        public readonly static SavedBool GroupSeperator =
            new SavedBool("Seperator", MainPanel.FileName, GroupSeperator_DEF, true);
        public readonly static SavedInt Cols =
            new SavedInt("Cols", MainPanel.FileName, Cols_DEF, true);

        public static void ResetSettings() {
            GroupSeperator.value = GroupSeperator_DEF;
            Cols.value = Cols_DEF;
        }
        #endregion


        public override void Awake() {
            try {
                Log.Called();
                base.Awake();
                backgroundSprite = "GenericPanelWhite";
                color = new Color32(170, 170, 170, byte.MaxValue);
            } catch (Exception ex) { ex.Log(); }
        }

        bool started_ = false;
        public override void Start() {
            try {
                Log.Called();
                base.Start();
                Arrange();
                Invalidate();
                eventSizeChanged += (_, __) => MainPanel.Instance.Refresh();
            } catch (Exception ex) { ex.Log(); }
            started_ = true;
        }

        public override void OnDestroy() {
            Log.Called();
            foreach (var item in ButtomGroups.Keys.Cast<UIComponent>()) {
                if(item)
                    item.eventVisibilityChanged -= Rearrange;
            }
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

        public void RemoveButton(UIComponent c) {
            try {
                ButtomGroups.Remove(c);
                if (started_)
                    Arrange();
            } catch (Exception ex) { ex.Log(); }
        }

        public void Arrange() {
            try {
                Log.Called();
                var sortedButtoms =
                    ButtomGroups
                    ?.Keys
                    ?.Cast<UIComponent>()
                    ?.Where(item => item) // ignore destroyed items
                    ?.OrderBy(GroupName)
                    ?.ThenBy(item => item.name)
                    ?? Enumerable.Empty<UIComponent>();

                int col = 0;
                int row = 0;
                string prevGroup = null;
                foreach (var item in sortedButtoms) {
                    if (!item) continue; // ignore destroyed items
                    string groupName = (string)ButtomGroups[item];
                    if (col != 0 && GroupSeperator && prevGroup != groupName) {
                        AddSeperator(this);
                    }
                    prevGroup = groupName;

                    if (item.parent != this)
                        AttachUIComponent(item.gameObject);
                    item.eventVisibilityChanged -= Rearrange;
                    item.eventVisibilityChanged += Rearrange;
                    item.relativePosition = ButtonBase.SIZE * new Vector2(col, row);

                    if (item.isVisibleSelf) {
                        col = ++col;
                        if(col >= Cols) {
                            col = 0;
                            row++;
                        }
                    }
                }

                int col2 = col;
                int row2 = row;
                if (col > 0) row2++; // incomplete row.
                if (row > 0) col2 = Cols; // at least one full row.
                size = ButtonBase.SIZE * new Vector2(col2, row2);
            } catch (Exception ex) { ex.Log(); }
        }

        void Rearrange(UIComponent _, bool __) {
            Log.Called();
            MainPanel.Instance.RearrangeIfOpen();
        }

        const string SEPERATOR_NAME = "UUI_seperator";
        public UIComponent AddSeperator(UIComponent parent) {
            Log.Called();
            var panel = parent.AddUIComponent<UIPanel>();
            panel.size = new Vector2(10, 40);
            panel.name = SEPERATOR_NAME;
            return panel;
        }
    }
}
