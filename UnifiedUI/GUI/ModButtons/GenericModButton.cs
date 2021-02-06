namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;
    using UnityEngine;
    using System;
    using System.Linq;
    using ColossalFramework.Plugins;
    using KianCommons.Plugins;
    using System.Collections.Generic;

    public abstract class GenericModButton : ButtonBase {
        public virtual ToolBase Tool => null;
        public virtual UIComponent Widnow => null;
        public virtual PluginManager.PluginInfo Plugin => null;
        private PluginManager.PluginInfo plugin_;

        List<UIComponent> originalButtons_ = new List<UIComponent>();
        /// <summary>button to turn off</summary>
        public virtual UIComponent GetOriginalButton() => null;
        public virtual IEnumerable<UIComponent> GetOriginalButtons() => null;

        public override void Awake() {
            base.Awake();
            plugin_ = Plugin;
        }

        public override void Start() {
            Log.Debug("GenericModButton.Start() is called for " + Name);
            base.Start();
            if (Tooltip != null) tooltip = Tooltip;
            HandleOriginalButton();
            Settings.RefreshButtons += HandleOriginalButton;
        }

        void PopulateOriginalButtons() {
            originalButtons_.Clear();
            var a = GetOriginalButton();
            var b = GetOriginalButtons();
            if (a is not null)
                originalButtons_.Add(a);
            if(!b.IsNullorEmpty())
                originalButtons_.AddRange(b);
        }

        public void HandleOriginalButton() {
            PopulateOriginalButtons();
            foreach (var c in originalButtons_)
                c.gameObject.SetActive(!Settings.HideOriginalButtons);
        }

        public override void OnRefresh(ToolBase newTool) {
            //Log.Debug($"GenericModButton.OnRefresh({newTool}) Name:{Name} Tool:{Tool}");
            HandleOriginalButton();

            var tool = Tool;
            if (!tool)
                return;
            IsActive = newTool == Tool;
        }

        public virtual bool ShouldPopulate() {
            Log.Debug("GenericModButton.ShouldPopulate() called for " + Name, false);
            return Tool != null || Widnow != null || PluginExtensions.IsActive(Plugin);
        }

        public virtual bool ShouldShow() => true;

        public virtual void Activate() {
            Log.Debug("GenericModButton.Open() called for " + Name);
            IsActive = true;
            var tool = Tool;

            if (tool) tool.enabled = true; // ToolsModifierControl.toolController.CurrentTool = tool;
            Widnow?.Hide();
        }

        public virtual void Deactivate() {
            Log.Debug("GenericModButton.Close() called  for " + Name);
            IsActive = false;
            if (Tool && ToolsModifierControl.toolController?.CurrentTool == Tool)
                ToolsModifierControl.SetTool<DefaultTool>();
            Widnow?.Show();
        }

        public virtual void Toggle() {
            if (IsActive) Deactivate();
            else Activate();
        }

        protected override void OnClick(UIMouseEventParameter p) {
            Log.Debug("GenericModButton.OnClick() called  for " + Name, false);
            base.OnClick(p);
            Toggle();
        }

        /// <summary>
        /// Get tool instance using reflrect naemsapce.typename.instance
        /// </summary>
        /// <param name="assemblyName">assembly to look for tool</param>
        /// <param name="fullName">namespace.typename of the class that contains the tool instance</param>
        /// <param name="instanceName">static field name pointing to the tool.</param>
        /// <returns></returns>
        public static ToolBase GetTool(string assemblyName, string fullName, string instanceName) {
            var fieldInfo = Type.GetType(fullName + ", " + assemblyName)?.GetField(instanceName);
            return fieldInfo?.GetValue(null) as ToolBase ?? throw new Exception("Could not find " + fullName);
        }

        /// <summary>
        /// returns the tool of a given name from  toolController.
        /// </summary>
        public static ToolBase GetTool(string name) {
            GameObject gameObject = ToolsModifierControl.toolController.gameObject;
            var ret = gameObject.GetComponents<ToolBase>()
                .Where(tool => tool.GetType().Name == name)
                .FirstOrDefault();
            if (ret == null)
                Log.Error("could not find tool: " + name);
            return ret;
        }

        /// <summary>
        /// returns the button of a given type name.
        /// </summary>
        public static UIButton GetButton(string typeName) {
            var ret = UIView.GetAView()
                .GetComponentsInChildren<UIButton>(includeInactive:true)
                .Where(c => c is not GenericModButton && c.GetType().Name == typeName)
                .FirstOrDefault();
            if (ret == null)
                Log.Error("could not find button: " + typeName);
            else
                Log.Debug($"GetButton({typeName})->{ret}");
            return ret;
        }

        /// <summary>
        /// returns all the button of a given type name.
        /// </summary>
        public static IEnumerable<UIComponent> GetButtons(string typeName) {
            var ret = UIView.GetAView()
                .GetComponentsInChildren<UIButton>(includeInactive:true)
                .Where(c => c is not GenericModButton && c.GetType().Name == typeName);
            if (ret.IsNullorEmpty())
                Log.Error("could not find any button of type: " + typeName);
            else
                Log.Debug($"GetButtons({typeName})->{ret.ToSTR()}");
            return ret.Select(c => c as UIComponent);
        }

                /// <summary>
        /// returns all the button of a given type name.
        /// </summary>
        public static IEnumerable<UIComponent> FindButtons(string name) {
            var ret = UIView.GetAView()
                .GetComponentsInChildren<UIButton>(includeInactive:true)
                .Where(c => c is not GenericModButton && c.name == name);
            if (ret.IsNullorEmpty())
                Log.Error("could not find any button with name: " + name);
            else
                Log.Debug($"FindButtons({name})->{ret.ToSTR()}");
            return ret.Select(c => c as UIComponent);
        }
    }
}
