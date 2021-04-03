namespace UnifiedUI.Helpers {
    using ColossalFramework;
    using ColossalFramework.Plugins;
    using ColossalFramework.UI;
    using ICities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    public static class UUIHelpers {
        const string UUI_NAME = "UnifiedUI.API.UUIMod";

        /// <typeparam name="TDelegate">delegate type</typeparam>
        /// <returns>Type[] represeting arguments of the delegate.</returns>
        internal static Type[] GetParameterTypes<TDelegate>()
            where TDelegate : Delegate =>
            typeof(TDelegate)
            .GetMethod("Invoke")
            .GetParameters()
            .Select(p => p.ParameterType)
            .ToArray();

        /// <summary>
        /// Gets directly declared method based on a delegate that has
        /// the same name as the target method
        /// </summary>
        /// <param name="type">the class/type where the method is delcared</param>
        /// <param name="name">the name of the method</param>
        internal static MethodInfo GetMethod<TDelegate>(this Type type, string name) where TDelegate : Delegate {
            return type.GetMethod(
                name,
                types: GetParameterTypes<TDelegate>())
                ?? throw new Exception("could not find method " + name);
        }

        internal static TDelegate CreateDelegate<TDelegate>(Type type, string name) where TDelegate : Delegate {
            var method = type.GetMethod<TDelegate>(name);
            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), method);
        }

        internal static IEnumerable<PluginManager.PluginInfo> Plugins => PluginManager.instance.GetPluginsInfo();

        internal static PluginManager.PluginInfo GetUUIPlugin() =>
            Plugins.FirstOrDefault(p => p.IsUUI());


        static bool IsUUI(this PluginManager.PluginInfo p) =>
            p.userModInstance.GetType().Assembly.GetType(UUI_NAME) != null;

        internal static Assembly GetUUILib() {
            if (IsUUIEnabled())
                return GetUUIPlugin().GetAssemblies().Find(a => a.GetName().Name == "UnifiedUILib");
            
            Assembly ret = null;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies) {
                if (assembly.GetName().Name != "PrefabMetadata")
                    continue;
                if (ret == null || ret.GetName().Version < assembly.GetName().Version) {
                    ret = assembly;
                }
            }
            if (ret == null ) {
                string sAssemblies = string.Join("\n", assemblies.Select(asm => asm.ToString()).ToArray());
                throw new Exception("failed to get latest PrefabMetadata. assemblies are:\n" + sAssemblies);
            }
            return ret;
        }

        internal static Type GetUUI() => GetUUILib().GetType(UUI_NAME, throwOnError: true);

        internal delegate UIComponent RegisterCustomHandler
            (string name, string groupName, string tooltip, string spritefile,
            Action<bool> onToggle, Action<ToolBase> onToolChanged,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys);
        internal delegate UIComponent RegisterToolHandler
            (string name, string groupName, string tooltip, string spritefile, ToolBase tool,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys);
        internal delegate void RegisterHotkeysHandler(Action onToggle,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys);


        /// <summary>
        /// register a button to tie to the given tool.
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="spritefile">full path to the file that contains 4 40x40x button sprites(see example)</param>
        /// <param name="tool">the tool to tie the buttont to.</param>
        /// <param name="activationKey">hot key to trigger the button</param>
        /// <param name="activeKeys">turn off these hotkeys in other mods</param>
        /// <returns>component containing the button. you can hide this component if necessary.</returns>
        public static UIComponent RegisterToolButton(
            string name, string groupName, string tooltip, string spritefile, ToolBase tool,
            SavedInputKey activationKey = null, Dictionary<SavedInputKey, Func<bool>> activeKeys = null) {
            if (!IsUUIEnabled()) return null;
            var Register = CreateDelegate<RegisterToolHandler>(GetUUI(), "Register");
            return Register(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                spritefile: spritefile,
                tool: tool,
                activationKey: activationKey,
                activeKeys: activeKeys);
        }

        /// <summary>
        /// register a button to tie to the given tool.
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="spritefile">full path to the file that contains 4 40x40x button sprites(see example)</param>
        /// <param name="tool">the tool to tie the buttont to.</param>
        /// <param name="activationKey">hot key to trigger the button</param>
        /// <param name="activeKeys">turn off these hotkeys in other mods</param>
        /// <returns>component containing the button. you can hide this component if necessary.</returns>
        public static UIComponent RegisterToolButton(
            string name, string groupName, string tooltip, string spritefile, ToolBase tool,
            SavedInputKey activationKey, IEnumerable<SavedInputKey> activeKeys) {
            var activeKeys2 = new Dictionary<SavedInputKey, Func<bool>>();
            foreach (var key in activeKeys)
                activeKeys2[key] = null;

            return RegisterToolButton(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                spritefile: spritefile,
                tool: tool,
                activationKey: activationKey,
                activeKeys: activeKeys2);
        }


        /// <summary>
        /// register a custom button .
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="spritefile">full path to the file that contains 4 40x40x button sprites(see example)</param>
        /// <param name="onToggle">call-back for when the button is activated/deactivated</param>
        /// <param name="onToolChanged">call-back for when any active tool changes.</param>
        /// <param name="activationKey">hot key to trigger the button</param>
        /// <param name="activeKeys">hotkey->active dictionary. turns off these hotkeys in other mods while active</param>
        /// <returns>wrapper for the button which you can use to change the its state.</returns>
        public static UUICustomButton RegisterCustomButton(
            string name, string groupName, string tooltip, string spritefile,
            Action<bool> onToggle, Action<ToolBase> onToolChanged = null,
            SavedInputKey activationKey = null, Dictionary<SavedInputKey, Func<bool>> activeKeys = null) {
            if (!IsUUIEnabled()) return null;
            var Register = CreateDelegate<RegisterCustomHandler>(GetUUI(), "Register");
            UIComponent component = Register(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                spritefile: spritefile,
                onToggle: onToggle,
                onToolChanged: onToolChanged,
                activationKey: activationKey,
                activeKeys: activeKeys);
            return new UUICustomButton(component);
        }

        /// <summary>
        /// register a custom button .
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="spritefile">full path to the file that contains 4 40x40x button sprites(see example)</param>
        /// <param name="onToggle">call-back for when the button is activated/deactivated</param>
        /// <param name="onToolChanged">call-back for when any active tool changes.</param>
        /// <param name="activationKey">hot key to trigger the button</param>
        /// <param name="activeKeys">hotkey->active dictionary. turns off these hotkeys in other mods while active</param>
        /// <returns>wrapper for the button which you can use to change the its state.</returns>
        public static UUICustomButton RegisterCustomButton(
            string name, string groupName, string tooltip, string spritefile,
            Action<bool> onToggle, Action<ToolBase> onToolChanged,
            SavedInputKey activationKey, IEnumerable<SavedInputKey> activeKeys) {
            var activeKeys2 = new Dictionary<SavedInputKey, Func<bool>>();
            foreach (var key in activeKeys)
                activeKeys2[key] = null;

            return RegisterCustomButton(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                spritefile: spritefile,
                onToggle: onToggle,
                onToolChanged: onToolChanged,
                activationKey: activationKey,
                activeKeys: activeKeys2);
        }

        /// <summary>
        /// register hotkeys.
        /// </summary>
        /// <param name="onToggle">call back for when activationKey is pressed.</param>
        /// <param name="onToolChanged">call-back for when any active tool changes.</param>
        /// <param name="activationKey">hot key to toggle</param>
        /// <param name="activeKeys">hotkey->active dictionary. turns off these hotkeys in other mods while active</param>
        public static void RegisterHotkeys(
            Action onToggle, SavedInputKey activationKey = null, Dictionary<SavedInputKey, Func<bool>> activeKeys = null) {
            if (!IsUUIEnabled()) return;
            var Register = CreateDelegate<RegisterHotkeysHandler>(GetUUI(), "Register");
            Register(
                onToggle: onToggle,
                activationKey: activationKey,
                activeKeys: activeKeys);
        }

        /// <summary>
        /// Destroy all gameObjects, components, and children
        /// </summary>
        public static void Destroy(this Component button) => GameObject.Destroy(button?.gameObject);

        /// <summary>
        /// Gets the path to the mod that has the user mod instance.
        /// </summary>
        /// <param name="userModInstance">instance of IUserMod</param>
        /// <returns>path to mod</returns>
        public static string GetModPath(this IUserMod userModInstance) =>
            Plugins.FirstOrDefault(p => p?.userModInstance == userModInstance)?.modPath;

        /// <summary>
        /// Gets the full path to a file from the input mod
        /// </summary>
        /// <param name="userModInstance">instance of IUserMod</param>
        /// <param name="paths">diretory,file names to combine</param>
        /// <returns>full path to file.</returns>
        public static string GetFullPath(this IUserMod userModInstance, params string[] paths) {
            string ret = userModInstance.GetModPath();
            foreach (string path in paths)
                ret = Path.Combine(ret, path);
            return ret;
        }

        /// <summary>
        /// test if UnifiedUI is present and enable.
        /// </summary>
        public static bool IsUUIEnabled() {
            var uui = GetUUIPlugin();
            return uui != null && uui.isEnabled;
        }
    }
}
