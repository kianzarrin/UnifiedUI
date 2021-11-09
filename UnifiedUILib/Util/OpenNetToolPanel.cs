namespace UnifiedUI.Util {
    using ColossalFramework.UI;
    using System.Collections;
    using System.Collections.Generic;
    using UnifiedUI.GUI;
    using KianCommons;
    public static class OpenToolPanelUtil {
        private static Dictionary<string, UIButton> _buttonsCache = new Dictionary<string, UIButton>();

        private static UIButton TSCloseButton => FindButton("TSCloseButton");
        private static UIButton FindButton(string name) {
            if(!_buttonsCache.TryGetValue(name, out UIButton button) || !button) {
                button = UIView.Find<UIButton>(name);
                _buttonsCache[name] = button;
            }
            return button;
        }
        public static void TryOpen() {
            var tool = ToolsModifierControl.toolController.CurrentTool;
            Log.Called(tool);
            var netTool = ToolsModifierControl.GetTool<NetTool>();
            var buildingTool = ToolsModifierControl.GetTool<BuildingTool>();

            if(tool == netTool) {
                ShowInPanel(netTool.m_prefab);
            } else if(tool == buildingTool ) {
                ShowInPanel(buildingTool.m_prefab);
            }
        }


        public static void ShowInPanel(PrefabInfo info) {
            Log.Called(info);
            if(!info) return;

            UIButton networkButton = FindButton(info.name);
            if(networkButton && !networkButton.isVisible) {
                // TSCloseButton.SimulateClick();

                UITabstrip subMenuTabstrip = null;
                UIScrollablePanel scrollablePanel = null;
                UIComponent current = networkButton, parent = networkButton.parent;
                int subMenuTabstripIndex = -1, menuTabstripIndex = -1;
                while(parent != null) {
                    if(current.name == "ScrollablePanel") {
                        subMenuTabstripIndex = parent.zOrder;
                        scrollablePanel = current as UIScrollablePanel;
                    }
                    if(current.name == "GTSContainer") {
                        menuTabstripIndex = parent.zOrder;
                        subMenuTabstrip = parent.Find<UITabstrip>("GroupToolstrip");
                    }
                    current = parent;
                    parent = parent.parent;
                }
                UITabstrip menuTabstrip = current.Find<UITabstrip>("MainToolstrip");
                if(scrollablePanel == null
                || subMenuTabstrip == null
                || menuTabstrip == null
                || menuTabstripIndex == -1
                || subMenuTabstripIndex == -1) return;
                menuTabstrip.selectedIndex = menuTabstripIndex;
                menuTabstrip.ShowTab(menuTabstrip.tabs[menuTabstripIndex].name);
                subMenuTabstrip.selectedIndex = subMenuTabstripIndex;
                subMenuTabstrip.ShowTab(subMenuTabstrip.tabs[subMenuTabstripIndex].name);

                // Clear filters
                UIPanel filterPanel = scrollablePanel.parent.Find<UIPanel>("FilterPanel");
                if(filterPanel != null) {
                    foreach(UIMultiStateButton c in filterPanel.GetComponentsInChildren<UIMultiStateButton>()) {
                        if(c.isVisible && c.activeStateIndex == 1) {
                            c.activeStateIndex = 0;
                        }
                    }
                }

                MainPanel.Instance.StartCoroutine(DoClick(scrollablePanel, networkButton));
            }
        }

        private static IEnumerator DoClick(UIScrollablePanel scrollablePanel, UIButton networkButton) {
            yield return 0;
            yield return 0;

            networkButton.SimulateClick();
            scrollablePanel.ScrollIntoView(networkButton);
        }


    }
}
