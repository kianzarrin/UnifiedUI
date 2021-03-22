namespace UnifiedUI.API {
    using ColossalFramework.UI;
    using System;
    using UnifiedUI.GUI;

    public static class UUIMod {
        public static UIComponent Register
            (string name, string groupName, string tooltip, string spritefile, Action<bool> onToggle, Action<ToolBase> onToolChanged = null) =>
            MainPanel.Instance.Register(name, groupName, spritefile, tooltip, onToggle, onToolChanged);

        public static UIComponent Register
            (string name, string groupName, string tooltip, string spritefile, ToolBase tool) =>
            MainPanel.Instance.Register(name, groupName, spritefile, tooltip, tool);
    }
}
