namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;

    public abstract class ModButtonBase3 : ButtonBase3 {
        protected override void OnClick(UIMouseEventParameter p) {
            Log.Debug(ThisMethod + " called  for " + Name, false);
            base.OnClick(p);
            Toggle();
        }
    }
}
