namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;


    public class ControlledDrag : UIDragHandle {
        public override void Awake() {
            base.Awake();
            tooltip = "Hold CTRL to move";
        }

        protected override void OnMouseMove(UIMouseEventParameter p) {
            if(Helpers.ControlIsPressed)
                base.OnMouseMove(p);
        }

    }
}
