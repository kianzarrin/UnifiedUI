namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;
    using System;

    public class ExternalButton : ModButtonBase {
        private string spritesPath_;
        public Action<ToolBase> OnToolChangedCallBack;
        public Action<bool> OnToggleCallBack;
        public ToolBase Tool;

        public override string SpritesFileName => spritesPath_;

        public void Release() => Destroy(gameObject);

        public static ExternalButton Create(
            UIComponent parent,
            string name,
            string tooltip,
            string spritesFile) { 
            var ret = parent.AddUIComponent<ExternalButton>();
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
                Log.Info(ThisMethod + " called for " + Name);
                SetTool(Tool);
                base.Activate();
                OnToggleCallBack?.Invoke(true);
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }
        public override void Deactivate() {
            try {
                Log.Debug(ThisMethod +  " called  for " + Name);
                base.Deactivate();
                UnsetTool(Tool);
                OnToggleCallBack?.Invoke(false);
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}
