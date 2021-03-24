namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;
    using System;

    public class ExternalCustomButton : ButtonBase {
        private string spritesPath_;
        public Action<ToolBase> OnToolChangedCallBack;
        public Action<bool> OnToggleCallBack;
        public ToolBase Tool;

        public override string SpritesFileName => spritesPath_;

        public void Release() => Destroy(gameObject);

        public static ExternalCustomButton Create(
            UIComponent parent,
            string name,
            string tooltip,
            string spritesFile) { 
            var ret = parent.AddUIComponent<ExternalCustomButton>();
            ret.tooltip = tooltip;
            ret.name = name;
            ret.spritesPath_ = spritesFile;
            return ret;
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }
        public override void OnToolChanged(ToolBase newTool) {
            try {
                if(Tool) IsActive = newTool == Tool;
                OnToolChangedCallBack?.Invoke(newTool);
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        public override void Activate() {
            try { 
                base.Activate();
                OnToggleCallBack?.Invoke(true);
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }
        public override void Deactivate() {
            try {
                Log.Debug("ExternalCustomButton.Deactivate() called  for " + Name);
                base.Deactivate();
                if(Tool && ToolsModifierControl.toolController?.CurrentTool == Tool)
                    ToolsModifierControl.SetTool<DefaultTool>();
                OnToggleCallBack?.Invoke(false);
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}
