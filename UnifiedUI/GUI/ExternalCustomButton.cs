namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using UnifiedUI.API;

    public class ExternalCustomButton : ButtonBase {
        Action<ToolBase> OnToolChanged_;
        Func<bool> ShouldShow_;
        Action Activate_;
        Action Deactivate_;
        string SpritesFileName_;

        public void Release() => Destroy(gameObject);

        public static ExternalCustomButton Create(
            UIComponent parent,
            string spritesFile,
            Action activate,
            Action deactivate,
            Func<bool> shouldShow = null,
            Action<ToolBase> onToolChanged = null) {
            var ret = parent.AddUIComponent<ExternalCustomButton>();
            ret.Activate_ = activate;
            ret.Deactivate_ = deactivate;
            ret.ShouldShow_ = shouldShow;
            ret.OnToolChanged_ = onToolChanged;
            ret.SpritesFileName_ = spritesFile;
            return ret;
        }

        public static ExternalCustomButton Create
            (UIComponent parent, string name, string groupName, string tooltip, string spritefile, Action onToggle, Action<ToolBase> onToolChanged = null) {
            var ret = parent.AddUIComponent<ExternalCustomButton>();
            ret.Activate_ = onToggle;
            ret.Deactivate_ = onToggle;
            ret.OnToolChanged_ = onToolChanged;
            ret.SpritesFileName_ = spritefile;
            ret.tooltip = tooltip;
            ret.name = name;
            return ret;
        }


        public override string SpritesFileName => SpritesFileName_;
        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }
        public override void OnToolChanged(ToolBase newTool) {
            if (ShouldShow_ != null)
                isVisible = ShouldShow_.Invoke();
            OnToolChanged_?.Invoke(newTool);
        }
        public void Activate() {
            IsActive = true;
            Activate_();
        }
        public void Deactivate() {
            Log.Debug("ExternalButton.Deactivate() called  for " + Name);
            IsActive = false;
            Deactivate_();
        }
    }
}
