namespace UnifiedUI.API {
    using System;
    using UnifiedUI.GUI;
    using UnityEngine;

    public class UUIMod {
        public MonoBehaviour Register
            (string name, string groupName, string tooltip, string spritefile, Action onToggle, Action<ToolBase> onToolChanged = null) =>
            MainPanel.Instance.Register(name, groupName, spritefile, tooltip, onToggle, onToolChanged);
    }
}
