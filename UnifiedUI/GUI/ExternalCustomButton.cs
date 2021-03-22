namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;
    using System;

    public class ExternalCustomButton : ButtonBase {
        Action<ToolBase> OnToolChanged_;
        Action<bool> Toggle_;
        string SpritesFileName_;
        public override string SpritesFileName => SpritesFileName_;

        public void Release() => Destroy(gameObject);

        public static ExternalCustomButton Create(
            UIComponent parent,
            string name,
            string tooltip,
            string spritesFile,
            Action<bool> onToggle,
            Action<ToolBase> onToolChanged = null) {
            var ret = parent.AddUIComponent<ExternalCustomButton>();
            if(tooltip != null) ret.tooltip = tooltip;
            if(!string.IsNullOrEmpty(name)) ret.name = name;
            else ret.name = onToggle.Method.Name;
            ret.Toggle_ = onToggle;
            ret.OnToolChanged_ = onToolChanged;
            ret.SpritesFileName_ = spritesFile;
            return ret;
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }
        public override void OnToolChanged(ToolBase newTool) {
            OnToolChanged_?.Invoke(newTool);
        }
        public void Activate() {
            IsActive = true;
            Toggle_(true);
        }
        public void Deactivate() {
            Log.Debug("ExternalButton.Deactivate() called  for " + Name);
            IsActive = false;
            Toggle_(false);
        }
    }
}
