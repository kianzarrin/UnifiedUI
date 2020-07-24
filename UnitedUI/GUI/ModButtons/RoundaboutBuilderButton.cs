using ColossalFramework.UI;
using KianCommons;
using System;

namespace UnitedUI.GUI.ModButtons {
    public class RoundaboutBuilderButton : GenericButton {
        public static RoundaboutBuilderButton Instance;
        public RoundaboutBuilderButton() : base() => Instance = this;
        public override string SpritesFileName => "B.png";
        public override string Tooltip => "Roundabout builder";

        UIPanel UIWindow => UIView.GetAView().FindUIComponent<UIPanel>("RAB_ToolOptionsPanel") as UIPanel
            ?? throw new Exception("Could not found RAB_ToolOptionsPanel");

        protected override void OnClick(UIMouseEventParameter p) {
            // Commented out to ignore base behaviour
            // base.OnClick(p);
            var type = UIWindow.GetType();
            var methodInfo = type.GetMethod("Toggle");
            methodInfo.Invoke(UIWindow, null);
            OnToolChanged(ToolsModifierControl.toolController.CurrentTool);
        }

        public override void OnToolChanged(ToolBase newTool) {
            Log.Debug("RoundaboutBuilderButton.OnToolChanged(): newTool.namespace = " + newTool.GetType().Namespace);
            IsActive = newTool.GetType().Namespace.Contains("RoundaboutBuilder");
        }
    }
}
