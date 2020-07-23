namespace UnitedUI.GUI.ModButtons {
    public class NodeControllerButton : GenericButton {
        public static NodeControllerButton Instance;
        public NodeControllerButton() : base() => Instance = this;
        public override string SpritesFileName => "B.png";
        public override ToolBase Tool => GetTool("NodeControllerTool");
    }
}
