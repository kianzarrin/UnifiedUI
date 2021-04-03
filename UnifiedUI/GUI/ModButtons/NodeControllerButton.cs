namespace UnifiedUI.GUI.ModButtons {
    using ColossalFramework;
    using ColossalFramework.Plugins;
    using ColossalFramework.UI;
    using KianCommons.Plugins;
    using System.Collections.Generic;

    public class NodeControllerButton : GenericModButton {
        public static NodeControllerButton Instance;
        public NodeControllerButton() : base() => Instance = this;
        public override string SpritesFile => "uui_node_controller.png";
        public override string Tooltip => "Node Controller";
        public override ToolBase Tool => GetTool("NodeControllerTool");
        public override PluginManager.PluginInfo Plugin =>
            PluginUtil.GetPlugin("Node Controller");
        public override IEnumerable<UIComponent> GetOriginalButtons() =>
            GetButtons("NodeControllerButton");
        public override SavedInputKey GetHotkey() =>
            ReplaceHotkey(name: "ActivationShortcut", fileName: "NodeController");
    }
}
