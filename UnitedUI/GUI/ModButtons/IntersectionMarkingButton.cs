namespace UnitedUI.GUI.ModButtons {
    public class IntersectionMarkingButton : GenericButton {
        public static IntersectionMarkingButton Instance;
        public IntersectionMarkingButton() : base() => Instance = this;
        public override string SpritesFileName => "B.png";
        public override ToolBase Tool => GetTool("NodeMarkupTool");
    }
}
