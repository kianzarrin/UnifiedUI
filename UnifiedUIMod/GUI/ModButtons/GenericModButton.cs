namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;
    using UnityEngine;
    using System;
    using System.Linq;
    using ColossalFramework.Plugins;
    using KianCommons.Plugins;
    using System.Collections.Generic;
    using ColossalFramework;
    using static KianCommons.ReflectionHelpers;
    using UnifiedUI.LifeCycle;
    using System.IO;
    using ICities;

    public abstract class GenericModButton : ModButtonBase {
        public virtual ToolBase Tool => null;
        public virtual UIComponent Widnow => null;
        public virtual PluginManager.PluginInfo Plugin => null;
        private PluginManager.PluginInfo plugin_;

        List<UIComponent> originalButtons_ = new List<UIComponent>();
        /// <summary>button to turn off</summary>
        ///
        public static String ModPath => PluginUtil.GetPlugin<LifeCycle>().modPath;

        public abstract string SpritesFileName { get; }
        public virtual string SpritesFile => Path.Combine(ModPath, SpritesFileName);

        public virtual UIComponent GetOriginalButton() => null;
        public virtual IEnumerable<UIComponent> GetOriginalButtons() => null;

        public override void Awake() {
            try {
                base.Awake();
                plugin_ = Plugin;
                atlas = GetOrCreateAtlas(SuggestedAtlasName, SpritesFile);
            } catch(Exception ex) { Log.Exception(ex); }
        }

        public override void Start() {
            try {
                Log.Debug("GenericModButton.Start() is called for " + Name);
                base.Start();
                HandleOriginalButtons();
                MainPanel.Instance.EventRefreshButtons += HandleOriginalButtons;
                ActivationKey ??= GetHotkey();
                Log.Info($"activation key for {Name} is {ActivationKey?.name}:{ActivationKey?.value}");
                Log.Info($"active keys for {Name} are {ActiveKeys.ToSTR()}");
            } catch(Exception ex) { Log.Exception(ex); }

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

        public virtual SavedInputKey GetHotkey() => null;

        public void HandleOriginalButtons() {
            CollectOriginalButtons();
            foreach (var c in originalButtons_)
                c.gameObject.SetActive(!UUISettings.HideOriginalButtons);
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
            SetTool(Tool);
            Widnow?.Show();
        }

        public override void Deactivate() {
            Log.Debug("GenericModButton.Close() called  for " + Name);
            base.Deactivate();
            UnsetTool(Tool);
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
            if(ret == null)
                Log.Debug("could not find button: " + typeName, false);
            else
                Log.Debug($"GetButton({typeName})->{ret}", false);
            return ret;
        }

        /// <summary>
        /// returns all the button of a given type name.
        /// </summary>
        public static IEnumerable<UIComponent> GetButtons(string typeName) {
            var ret = UIView.GetAView()
                .GetComponentsInChildren<UIButton>(includeInactive:true) // TODO: is includeInactive this redundant?
                .Where(c => c is not GenericModButton && c.GetType().Name == typeName);
            if(ret.IsNullorEmpty())
                Log.Debug("could not find any button of type: " + typeName, false);
                
            else
                Log.Debug($"GetButtons({typeName})->{ret.ToSTR()}", false);
            return ret.Select(c => c as UIComponent);
        }

        /// <summary>
        /// returns all the button of a given type name.
        /// </summary>
        public static IEnumerable<UIComponent> FindButtons(string name) {
            var ret = UIView.GetAView()
                .GetComponentsInChildren<UIButton>(includeInactive:true)
                .Where(c => c is not GenericModButton && c.name == name);
            if(ret.IsNullorEmpty())
                Log.Debug("could not find any button with name: " + name, false);
            else
                Log.Debug($"FindButtons({name})->{ret.ToSTR()}", false);
            return ret.Select(c => c as UIComponent);
        }

        public static SavedInputKey GetInputKey(string type, string field) {
            var t = Type.GetType(type);
            return (SavedInputKey)ReflectionHelpers.GetFieldValue(t, field);
        }

        public static SavedInputKey GetHotkey(string name, string fileName) {
            var options = UIView.Find<UITabContainer>("OptionsContainer");
            var templateKey = new SavedInputKey(name, fileName);
            foreach(var button in options.GetComponentsInChildren<UIButton>()) {
                if(button.objectUserData is SavedInputKey key && key == templateKey) {
                    if(button.InUUIConflictPanel()) continue;
                    return key;
                }
            }
            return null;
        }


        public static SavedInputKey ReplaceHotkey(string name, string fileName) {
            var options = UIView.Find<UITabContainer>("OptionsContainer");
            var newKey = new SavedInputKey(name, fileName, default, true);
            foreach(var button in options.GetComponentsInChildren<UIButton>()) {
                if(button.objectUserData is SavedInputKey oldKey && oldKey == newKey) {
                    if(button.InUUIConflictPanel()) continue;
                    // copy value across in case it does not exists.
                    // if I use newKey.value = oldKey.value that would also work
                    // but I don't want to save the value based on a whim.
                    var value = GetFieldValue(oldKey, "m_Value");
                    SetFieldValue(newKey, "m_Value", value); 

                    // hack to nuteralize original hotkey wihtout changing the value in config file.
                    SetFieldValue(oldKey, "m_AutoUpdate", false);
                    SetFieldValue(oldKey, "m_Value", (InputKey)0);
                    UUISettings.DisabledKeys.Add(oldKey);

                    button.objectUserData = newKey;
                    button.eventVisibilityChanged -= RefreshBindingButton;
                    button.eventVisibilityChanged += RefreshBindingButton;
                    return newKey;
                }
            }
            return null;
        }


        public static void RefreshBindingButton(UIComponent c, bool visible) {
            if(visible && c is UIButton button && button.objectUserData is SavedInputKey savedInputKey) {
                button.text = savedInputKey.ToLocalizedString("KEYNAME");
            }
        }


    }
}
