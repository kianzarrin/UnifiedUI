namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;
    using System;

    public class ExternalButton3 : ModButtonBase3 {
        public Action<ToolBase> OnToolChangedCallBack;
        public Action<bool> OnToggleCallBack;
        public ToolBase Tool;

        public void Release() => Destroy(gameObject);

        public override string Name => name;

        public static ExternalButton3 Create(
            UIComponent parent,
            string name,
            string tooltip,
            string spritesFile = null) { 
            var ret = parent.AddUIComponent<ExternalButton3>();
            ret.tooltip = tooltip;
            ret.name = name;
            if(!spritesFile.IsNullorEmpty())
            ret.atlas = ret.SetupAtlas(spritesFile);
            return ret;
        }

        private UITextureAtlas SetupAtlas(string file) => GetOrCreateAtlas(SuggestedAtlasName, file);

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
