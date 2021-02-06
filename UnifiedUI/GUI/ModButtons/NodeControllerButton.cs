using ColossalFramework.Plugins;
using ColossalFramework.UI;
using KianCommons.Plugins;

namespace UnifiedUI.GUI.ModButtons {
    public class NodeControllerButton : GenericModButton {
        public static NodeControllerButton Instance;
        public NodeControllerButton() : base() => Instance = this;
        public override string SpritesFileName => "uui_node_controller.png";
        public override string Tooltip => "Node Controller";
        public override ToolBase Tool => GetTool("NodeControllerTool");
        public override PluginManager.PluginInfo Plugin =>
            PluginUtil.GetPlugin("Node Controller");
        public override UIComponent GetOriginalButton() => GetButton("NodeControllerButton");
    }
}
