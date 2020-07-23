namespace UnitedUI.GUI.ModButtons {
    public class NetworkDetectiveButton : GenericButton {
        public static NetworkDetectiveButton Instance;
        public NetworkDetectiveButton() : base() => Instance = this;
        public override string SpritesFileName => "B.png";
        public override ToolBase Tool => GetTool("NetworkDetectiveTool");
    }
}
