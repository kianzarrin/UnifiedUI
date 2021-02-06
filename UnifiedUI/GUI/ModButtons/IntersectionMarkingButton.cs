using ColossalFramework.UI;
using System.Collections.Generic;

namespace UnifiedUI.GUI.ModButtons {
    public class IntersectionMarkingButton : GenericModButton {
        public static IntersectionMarkingButton Instance;
        public IntersectionMarkingButton() : base() => Instance = this;
        public override string SpritesFileName => "uui_imt.png";
        public override string Tooltip => "Intersection marking tool";
        public override ToolBase Tool => GetTool("NodeMarkupTool");
        public override  IEnumerable<UIComponent> GetOriginalButtons() =>
            GetButtons("NodeMarkupButton");
    }
}
