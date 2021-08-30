namespace UnifiedUI.GUI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using System.Collections;
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
            foreach (var item in ButtomGroups.Keys.Cast<UIComponent>()) {
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

        public void Arrange() {
            try {
                Log.Called();
                var sortedButtoms = ButtomGroups.Keys.Cast<UIComponent>().OrderBy(GroupName);

                int col = 0;
                int row = 0;
                string prevGroup = null;
                foreach (var item in sortedButtoms) {
                    string groupName = (string)ButtomGroups[item];
                    if (col != 0 && GroupSeperator && prevGroup != groupName) {
                        AddSeperator(this);
                    }
                    prevGroup = groupName;

                    if (item.parent != this)
                        AttachUIComponent(item.gameObject);
                    item.eventVisibilityChanged -= Rearrange;
                    item.eventVisibilityChanged += Rearrange; item.relativePosition = ButtonBase.SIZE * new Vector2(col, row);


                    if (item.isVisibleSelf) {
                        col = ++col;
                        if(col >= Cols) {
                            col = 0;
                            row++;
                        }
                    }
                }

                if (col > 0) row++;
                if (row > 0) col = Cols;
                size = ButtonBase.SIZE * new Vector2(col, row);
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
