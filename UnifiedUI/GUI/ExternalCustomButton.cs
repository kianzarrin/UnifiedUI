namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;
    using System;

    public class ExternalCustomButton : ButtonBase {
        Action<ToolBase> OnToolChanged_;
        ToolBase Tool_;
        Action<bool> Toggle_;
        string SpritesFileName_;

        public void Release() => Destroy(gameObject);

        public static ExternalCustomButton Create(
            UIComponent parent,
            string name,
            string tooltip,
            string spritesFile,
            Action<bool> onToggle,
            Action<ToolBase> onToolChanged = null) {
            var ret = parent.AddUIComponent<ExternalCustomButton>();
            ret.tooltip = tooltip;

            ret.name = name;
            ret.Toggle_ = onToggle;
            ret.OnToolChanged_ = onToolChanged;
            ret.SpritesFileName_ = spritesFile;
            return ret;
        }

        public static ExternalCustomButton Create
            (UIComponent parent, string name, string tooltip, string spritefile, ToolBase tool) {
            var ret = parent.AddUIComponent<ExternalCustomButton>();
            ret.SpritesFileName_ = spritefile;
            ret.tooltip = tooltip;
            ret.name = name;
            ret.Tool_ = tool;
            return ret;
        }


        public override string SpritesFileName => SpritesFileName_;
        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }
        public override void OnToolChanged(ToolBase newTool) {
            if(Tool_) IsActive = Tool_ == newTool;
            OnToolChanged_?.Invoke(newTool);
        }
        public void Activate() {
            IsActive = true;
            if(Tool_) Tool_.enabled = true;
            Toggle_(true);
        }
        public void Deactivate() {
            Log.Debug("ExternalButton.Deactivate() called  for " + Name);
            if(Tool_ && ToolsModifierControl.toolController?.CurrentTool == Tool_)
                ToolsModifierControl.SetTool<DefaultTool>();
            IsActive = false;
            Toggle_(false);
        }
    }
}
