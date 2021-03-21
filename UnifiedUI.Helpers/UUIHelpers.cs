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

        public static UIComponent RegisterToolButton(
            string name, string groupName, string tooltip, string spritefile, ToolBase tool) {
            if (!IsUUIEnabled()) return null;
            var Register = CreateDelegate<RegisterHandlerTool>(GetUUI(), "Register");
            return Register(name, groupName, spritefile, tooltip, tool);
        }

        public static UUICustomButton RegisterCustomButton(
            string name, string groupName, string tooltip, string spritefile, Action<bool> onToggle, Action<ToolBase> onToolChanged = null) {
            if (!IsUUIEnabled()) return null;
            var Register = CreateDelegate<RegisterHandlerCustom>(GetUUI(), "Register");
            UIComponent compoennt = Register(name, groupName, spritefile, tooltip, onToggle, onToolChanged);
            return new UUICustomButton(compoennt);
        }

        public static void Destroy(this UIComponent button) => GameObject.Destroy(button?.gameObject);

        public static string GetModPath(this IUserMod userModInstance) =>
            Plugins.FirstOrDefault(p => p?.userModInstance == userModInstance)?.modPath;

        public static string GetFullPath(this IUserMod userModInstance, params string[] paths) {
            string ret = userModInstance.GetModPath();
            foreach(string path in paths)
                Path.Combine(ret, path);
            return ret;
        }

        public static bool IsUUIEnabled() {
            var uui = GetUUIPlugin();
            return uui != null && uui.isEnabled;
        }
    }
}
