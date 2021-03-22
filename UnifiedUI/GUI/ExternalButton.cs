namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;

    public class ExternalButton : ButtonBase {
        public ToolBase Tool;
        string SpritesFileName_;
        public override string SpritesFileName => SpritesFileName_;

        public static ExternalButton Create(
            UIComponent parent,
            string name,
            string tooltip,
            string spritesFile,
            ToolBase tool) {
            var ret = parent.AddUIComponent<ExternalButton>();
            if (tooltip != null) ret.tooltip = tooltip;
            if(!string.IsNullOrEmpty(name)) ret.name = name;
            else ret.name = tool.GetType().FullName;
            ret.SpritesFileName_ = spritesFile;
            ret.Tool = tool;
            return ret;
        }

        public void Release() => Destroy(gameObject);

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public override void OnToolChanged(ToolBase newTool) {
            //Log.Debug($"GenericModButton.OnRefresh({newTool}) Name:{Name} Tool:{Tool}");
            if(Tool)
                IsActive = newTool == Tool;
        }

        public virtual void Activate() {
            Log.Debug("ExternalButton.Activate() called for " + Name);
            IsActive = true;
            if (Tool) Tool.enabled = true; // ToolsModifierControl.toolController.CurrentTool = tool;
        }

        public virtual void Deactivate() {
            Log.Debug("ExternalButton.Deactivate() called  for " + Name);
            IsActive = false;
            if (Tool && ToolsModifierControl.toolController?.CurrentTool == Tool)
                ToolsModifierControl.SetTool<DefaultTool>();
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
