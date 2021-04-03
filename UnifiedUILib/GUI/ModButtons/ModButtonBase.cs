namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;

    public abstract class ModButtonBase : ButtonBase {
        public override bool EmbededSprite => false;
        protected override void OnClick(UIMouseEventParameter p) {
            Log.Debug(ThisMethod + " called  for " + Name, false);
            base.OnClick(p);
            Toggle();
        }
    }
}
