namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using UnityEngine;

    public class ControlledDrag : UIDragHandle {
        public override void Awake() {
            base.Awake();
            tooltip = "Hold CTRL to move";
        }

        protected override void OnMouseMove(UIMouseEventParameter p) {
            if(!MainPanel.ControlToDrag || Helpers.ControlIsPressed)
                base.OnMouseMove(p);
        }

        protected override void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution) {
            base.OnResolutionChanged(previousResolution, currentResolution);
            target.FitToScreen();
        }
    }
}
