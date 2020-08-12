namespace UnitedUI.GUI.ModButtons {
    public class NetworkDetectiveButton : GenericModButton {
        public static NetworkDetectiveButton Instance;
        public NetworkDetectiveButton() : base() => Instance = this;
        public override string SpritesFileName => "B.png";
        public override string Tooltip => "Network Detective";
        public override ToolBase Tool => GetTool("NetworkDetectiveTool");
    }
}
