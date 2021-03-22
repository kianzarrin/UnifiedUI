namespace UnifiedUI.API {
    using ColossalFramework.UI;
    using System;
    using UnifiedUI.GUI;
    using UnifiedUI.LifeCycle;

    public static class UUIMod {
        public static UIComponent Register
            (string name, string groupName, string tooltip, string spritefile, Action<bool> onToggle, Action<ToolBase> onToolChanged = null) {
            if(!MainPanel.Instance)
                LifeCycle.Load();
            return MainPanel.Instance.Register(name, groupName, tooltip, spritefile,  onToggle, onToolChanged);
        }

        public static UIComponent Register
            (string name, string groupName, string tooltip, string spritefile, ToolBase tool) {
            if(!MainPanel.Instance)
                LifeCycle.Load();
            return MainPanel.Instance.Register(name, groupName, tooltip, spritefile,  tool);
        }
    }
}
