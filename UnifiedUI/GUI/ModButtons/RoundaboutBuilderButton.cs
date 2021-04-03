using ColossalFramework.UI;
using KianCommons;
using System;
using System.Linq;
using System.Collections.Generic;
using ColossalFramework;

namespace UnifiedUI.GUI.ModButtons {
    public class RoundaboutBuilderButton : GenericModButton {
        public static RoundaboutBuilderButton Instance;
        public RoundaboutBuilderButton() : base() => Instance = this;
        public override string SpritesFilePath => "uui_roundabout_builder.png";
        public override string Tooltip => "Roundabout builder";

        UIPanel UIWindow =>
            KianCommons.UI.UIUtils.GetCompenentsWithName<UIPanel>("RAB_ToolOptionsPanel")
            ?.FirstOrDefault()
            ?? throw new Exception("Could not found RAB_ToolOptionsPanel");

        public override IEnumerable<UIComponent> GetOriginalButtons() =>
            FindButtons("RoundaboutButton");

        public override SavedInputKey GetHotkey() {
            return ReplaceHotkey("modShortcut", "RoundaboutBuilder");
        }


        public override void Toggle() {
            // commented out to hide base implementiation:
            // base.Toggle()

            var type = UIWindow.GetType();
            var methodInfo = type.GetMethod("Toggle");
            methodInfo.Invoke(UIWindow, null);
            OnToolChanged(ToolsModifierControl.toolController.CurrentTool);
        }

        public override void OnToolChanged(ToolBase newTool) {
            HandleOriginalButtons();
            Log.Debug("RoundaboutBuilderButton.OnToolChanged(): newTool.namespace = " + newTool?.GetType()?.Namespace ?? "null");
            IsActive = newTool?.GetType()?.Namespace?.StartsWith("RoundaboutBuilder") ?? false;
        }

    }
}
