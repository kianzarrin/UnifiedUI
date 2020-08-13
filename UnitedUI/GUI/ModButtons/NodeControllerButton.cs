using ColossalFramework.Plugins;

namespace UnitedUI.GUI.ModButtons {
    public class NodeControllerButton : GenericModButton {
        public static NodeControllerButton Instance;
        public NodeControllerButton() : base() => Instance = this;
        public override string SpritesFileName => "uui_node_controller.png";
        public override string Tooltip => "Node Controller";
        public override ToolBase Tool => GetTool("NodeControllerTool");
        public override PluginManager.PluginInfo Plugin => KianCommons.PluginUtil.GetPlugin("Node Controller");
    }
}
