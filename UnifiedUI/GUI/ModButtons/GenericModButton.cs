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
            HandleOriginalButtons();
            Settings.RefreshButtons += HandleOriginalButtons;
        }

        void CollectOriginalButtons() {
            originalButtons_.Clear();
            var a = GetOriginalButton();
            var b = GetOriginalButtons();
            if (a is not null)
                originalButtons_.Add(a);
            if(!b.IsNullorEmpty())
                originalButtons_.AddRange(b);
        }

        public void HandleOriginalButtons() {
            CollectOriginalButtons();
            foreach (var c in originalButtons_)
                c.gameObject.SetActive(!Settings.HideOriginalButtons);
        }

        public override void OnToolChanged(ToolBase newTool) {
            //Log.Debug($"GenericModButton.OnRefresh({newTool}) Name:{Name} Tool:{Tool}");
            HandleOriginalButtons();

            var tool = Tool;
            if (!tool)
                return;
            IsActive = newTool == Tool;
        }

        public virtual bool ShouldPopulate() {
            Log.Debug("GenericModButton.ShouldPopulate() called for " + Name, false);
            return Tool != null || Widnow != null || PluginExtensions.IsActive(Plugin);
        }

        public override void Activate() {
            Log.Debug("GenericModButton.Open() called for " + Name);
            base.Activate();
            var tool = Tool;

            if (tool) tool.enabled = true; // ToolsModifierControl.toolController.CurrentTool = tool;
            Widnow?.Show();
        }

        public override void Deactivate() {
            Log.Debug("GenericModButton.Close() called  for " + Name);
            base.Deactivate();
            var tool = Tool;
            if(tool && ToolsModifierControl.toolController?.CurrentTool == tool)
                ToolsModifierControl.SetTool<DefaultTool>();
            Widnow?.Hide();
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
        public static ToolBase GetTool(string typeName) {
            GameObject gameObject = ToolsModifierControl.toolController.gameObject;
            var ret = gameObject.GetComponents<ToolBase>()
                .Where(tool => tool.GetType().Name == typeName)
                .FirstOrDefault();
            if (ret == null)
                Log.Error("could not find tool: " + typeName);
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
                .GetComponentsInChildren<UIButton>(includeInactive:true) // TODO: is includeInactive this redundant?
                .Where(c => c is not GenericModButton && c.GetType().Name == typeName);
            if (ret.IsNullorEmpty())
                Log.Debug("could not find any button of type: " + typeName);
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
                Log.Debug("could not find any button with name: " + name);
            else
                Log.Debug($"FindButtons({name})->{ret.ToSTR()}");
            return ret.Select(c => c as UIComponent);
        }
    }
}
