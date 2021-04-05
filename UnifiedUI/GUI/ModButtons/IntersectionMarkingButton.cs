using ColossalFramework.UI;
using System.Collections.Generic;
using ColossalFramework;
using static KianCommons.ReflectionHelpers;
using KianCommons;
using System;

namespace UnifiedUI.GUI.ModButtons {
    public class IntersectionMarkingButton : GenericModButton {
        public static IntersectionMarkingButton Instance;
        public IntersectionMarkingButton() : base() => Instance = this;
        public override string SpritesFileName => "uui_imt.png";
        public override string Tooltip => "Intersection marking tool";
        public override ToolBase Tool => GetTool("NodeMarkupTool");
        public override IEnumerable<UIComponent> GetOriginalButtons() =>
            GetButtons("NodeMarkupButton");

        public override SavedInputKey GetHotkey() {
            Log.Debug(ThisMethod + " called for " + Name);
            var file = "NodeMarkupSettingsFile";
            var keys = new[] {
                "DeleteAllShortcut",
                "ResetOffsetsShortcut",
                "AddFillerShortcut",
                "CopyMarkingShortcut",
                "PasteMarkingShortcut",
                "EditMarkingShortcut",
                "CreateEdgeLinesShortcut",
                "AddRuleShortcut",
                "SaveAsIntersectionTemplateShortcut",
                "CutLinesByCrosswalks"};

            ActiveKeys = new Dictionary<SavedInputKey, Func<bool>>();
            foreach(var key in keys) {
                var savedInputKey = GetHotkey(key, file);
                if(savedInputKey != null)
                    ActiveKeys[savedInputKey] = null;
            }

            return ReplaceHotkey("ActivationShortcut", file);
        }
    }
}
