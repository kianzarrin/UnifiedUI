namespace UnifiedUI.API {
    using System;
    using UnifiedUI.GUI;
    using UnityEngine;
    using ColossalFramework.UI;



    public class UUIMod {
        public UIComponent Register
            (string name, string groupName, string tooltip, string spritefile, Action<bool> onToggle, Action<ToolBase> onToolChanged = null) =>
            MainPanel.Instance.Register(name, groupName, spritefile, tooltip, onToggle, onToolChanged);
        public UIComponent Register
            (string name, string groupName, string tooltip, string spritefile, ToolBase tool) =>
            MainPanel.Instance.Register(name, groupName, spritefile, tooltip, tool);
    }
}
