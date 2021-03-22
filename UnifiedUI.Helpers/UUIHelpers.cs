namespace UnifiedUI.Helpers {
    using ColossalFramework.Plugins;
    using ColossalFramework.UI;
    using System.Linq;
    using System;
    using System.IO;
    using System.Reflection;
    using UnityEngine;
    using ICities;
    using System.Collections.Generic;

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
            return (TDelegate)Delegate.CreateDelegate(type, method);
        }

        internal static IEnumerable<PluginManager.PluginInfo> Plugins => PluginManager.instance.GetPluginsInfo();

        internal static PluginManager.PluginInfo GetUUIPlugin() =>
            Plugins.FirstOrDefault(p => p.IsUUI());


        static bool IsUUI(this PluginManager.PluginInfo p) =>
            p.userModInstance.GetType().Assembly.GetType(UUI_NAME) != null;

        internal static Assembly GetUUIAssembly() => GetUUIPlugin().userModInstance.GetType().Assembly;
        internal static Type GetUUI() => GetUUIAssembly().GetType(UUI_NAME, throwOnError: true);

        internal delegate UIComponent RegisterHandlerCustom
            (string name, string groupName, string tooltip, string spritefile, Action<bool> onToggle, Action<ToolBase> onToolChanged = null);
        internal delegate UIComponent RegisterHandlerTool
            (string name, string groupName, string tooltip, string spritefile, ToolBase tool); 

        /// <summary>
        /// register a button to tie to the given tool.
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="spritefile">full path to the file that contains 4 40x40x button sprites(see example)</param>
        /// <param name="tool">the tool to tie the buttont to.</param>
        /// <returns>component containing the button. you can hide this component if necessary.</returns>
        public static UIComponent RegisterToolButton(
            string name, string groupName, string tooltip, string spritefile, ToolBase tool) {
            if (!IsUUIEnabled()) return null;
            var Register = CreateDelegate<RegisterHandlerTool>(GetUUI(), "Register");
            return Register(name, groupName, spritefile, tooltip, tool);
        }

        /// <summary>
        /// register a button to add to UUI.
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="spritefile">full path to the file that contains 4 40x40x button sprites(see example)</param>
        /// <param name="onToggle">call-back for when the button is activated/deactivated</param>
        /// <param name="onToolChanged">call-back for when any active tool changes.</param>
        /// <returns>wrapper for the button which you can use to change the its state.</returns>
        public static UUICustomButton RegisterCustomButton(
            string name, string groupName, string tooltip, string spritefile, Action<bool> onToggle, Action<ToolBase> onToolChanged = null) {
            if (!IsUUIEnabled()) return null;
            var Register = CreateDelegate<RegisterHandlerCustom>(GetUUI(), "Register");
            UIComponent compoennt = Register(name, groupName, spritefile, tooltip, onToggle, onToolChanged);
            return new UUICustomButton(compoennt);
        }

        /// <summary>
        /// Destroy all gameObjects, components, and children
        /// </summary>
        public static void Destroy(this UIComponent button) => GameObject.Destroy(button?.gameObject);

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
            foreach(string path in paths)
                Path.Combine(ret, path);
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
