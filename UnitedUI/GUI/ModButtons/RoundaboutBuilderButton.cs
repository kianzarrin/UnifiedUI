using ColossalFramework.UI;
using System;

namespace UnitedUI.GUI.ModButtons {
    public class RoundaboutBuilderButton : GenericButton {
        public static RoundaboutBuilderButton Instance;
        public RoundaboutBuilderButton() : base() => Instance = this;
        public override string SpritesFileName => "B.png";

        UIPanel UIWindow => UIView.GetAView().FindUIComponent("RAB_ToolOptionsPanel") as UIPanel;

        protected override void OnClick(UIMouseEventParameter p) {
            // Commented out to ignore base behaviour
            // base.OnClick(p);
            UIWindow.GetType().GetMethod("Toggle").Invoke(UIWindow, null);
            OnToolChanged(ToolsModifierControl.toolController.CurrentTool);
        }

        public override void OnToolChanged(ToolBase newTool) {
            IsActive = newTool.GetType().Namespace.Contains("RoundaboutBuilder");
        }
    }
}
