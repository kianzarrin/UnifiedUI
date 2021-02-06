using ColossalFramework.UI;
using System.Collections.Generic;

namespace UnifiedUI.GUI.ModButtons {
    public class PedestrianBridgeButton : GenericModButton {
        public static PedestrianBridgeButton Instance;
        public PedestrianBridgeButton() : base() => Instance = this;
        public override string SpritesFileName => "uui_pedestrian_bridge_builder.png";
        public override string Tooltip => "Automatic Pedestrian bridge builder";
        public override ToolBase Tool => GetTool("PedBridgeTool");
        public override IEnumerable<UIComponent> GetOriginalButtons() =>
            GetButtons("PedestrianBridgeButton");
    }
}
