using ColossalFramework;

namespace UnifiedUI.GUI.ModButtons {
    public class NetworkDetectiveButton : GenericModButton {
        public static NetworkDetectiveButton Instance;
        public NetworkDetectiveButton() : base() => Instance = this;
        public override string SpritesFile => "uui_network_detective.png";
        public override string Tooltip => "Network Detective";
        public override ToolBase Tool => GetTool("NetworkDetectiveTool");

        public override SavedInputKey GetHotkey() =>
            ReplaceHotkey("ActivationShortcut", "NetworkDetective");
    }
}
