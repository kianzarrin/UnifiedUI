using ColossalFramework.UI;
using UnityEngine;
using System.Linq;

namespace UnitedUI.GUI.ModButtons {
    public class IntersectionMarkingButton : GenericModButton {
        public static IntersectionMarkingButton Instance;
        public IntersectionMarkingButton() : base() => Instance = this;
        public override string SpritesFileName => "uui_imt.png";
        public override string Tooltip => "Intersection marking tool";
        public override ToolBase Tool => GetTool("NodeMarkupTool");
        public override UIComponent GetOriginalButton() => GetButton("NodeMarkupButton");
    }
}
