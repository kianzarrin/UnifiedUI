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

        public static ExternalCustomButton Create(UIComponent parent, IUUIButton button) {
            var ret = parent.AddUIComponent<ExternalCustomButton>();
            ret.Activate_ = button.OnToggle;
            ret.Deactivate_ = button.OnToggle;
            ret.ShouldShow_ = button.ShouldShow;
            ret.OnToolChanged_ = null;
            ret.SpritesFileName_ = button.SpritesFile;
            ret.tooltip = button.Tooltip;
            ret.name = button.Name;
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
