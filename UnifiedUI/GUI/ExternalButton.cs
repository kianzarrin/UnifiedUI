namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;

    public abstract class ExternalButton : ButtonBase {
        public ToolBase Tool;
        public UIComponent Window;

        public static ExternalButton Create(
            UIComponent parent,
            string tooltip = null,
            ToolBase tool = null,
            UIComponent window = null) {
            var ret = parent.AddUIComponent<ExternalButton>();
            if (tooltip != null) ret.tooltip = tooltip;
            ret.Tool = tool;
            ret.Window = window;
            return ret;
        }

        public void Release() => Destroy(gameObject);
        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public override void OnToolChanged(ToolBase newTool) {
            //Log.Debug($"GenericModButton.OnRefresh({newTool}) Name:{Name} Tool:{Tool}");
            IsActive = newTool == Tool;
        }

        public virtual bool ShouldShow() => true;

        public virtual void Activate() {
            Log.Debug("ExternalButton.Activate() called for " + Name);
            IsActive = true;
            if (Tool) Tool.enabled = true; // ToolsModifierControl.toolController.CurrentTool = tool;
            Window?.Show();
        }

        public virtual void Deactivate() {
            Log.Debug("ExternalButton.Deactivate() called  for " + Name);
            IsActive = false;
            if (Tool && ToolsModifierControl.toolController?.CurrentTool == Tool)
                ToolsModifierControl.SetTool<DefaultTool>();
            Window?.Hide();
        }

        public virtual void Toggle() {
            if (IsActive) Deactivate();
            else Activate();
        }

        protected override void OnClick(UIMouseEventParameter p) {
            Log.Debug("ExternalButton.OnClick() called  for " + Name, false);
            base.OnClick(p);
            Toggle();
        }
    }
}
