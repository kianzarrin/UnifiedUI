using ColossalFramework.UI;
using KianCommons;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace UnitedUI.GUI.ModButtons {
    public class RoundaboutBuilderButton : GenericModButton {
        public static RoundaboutBuilderButton Instance;
        public RoundaboutBuilderButton() : base() => Instance = this;
        public override string SpritesFileName => "uui_roundabout_builder.png";
        public override string Tooltip => "Roundabout builder";

        UIPanel UIWindow =>
            KianCommons.UI.UIUtils.GetCompenentsWithName<UIPanel>("RAB_ToolOptionsPanel")
            ?.FirstOrDefault()
            ?? throw new Exception("Could not found RAB_ToolOptionsPanel");

        public override UIComponent GetOriginalButton() =>
            FindObjectsOfType<UIButton>().Where(c => c.name == "RoundaboutButton").FirstOrDefault();

        protected override void OnClick(UIMouseEventParameter p) {
            // Commented out to ignore base behaviour
            // base.OnClick(p);
            var type = UIWindow.GetType();
            var methodInfo = type.GetMethod("Toggle");
            methodInfo.Invoke(UIWindow, null);
            OnRefresh(ToolsModifierControl.toolController.CurrentTool);
        }

        public override void OnRefresh(ToolBase newTool) {
            Log.Debug("RoundaboutBuilderButton.OnToolChanged(): newTool.namespace = " + newTool?.GetType()?.Namespace ?? "null");
            IsActive = newTool?.GetType()?.Namespace?.StartsWith("RoundaboutBuilder") ?? false;
        }
    }
}
