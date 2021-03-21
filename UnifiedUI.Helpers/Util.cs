namespace UnifiedUI.Helpers {
    using ColossalFramework.Plugins;
    using ColossalFramework.UI;
    using System.Linq;
    using System;
    using System.Reflection;
    using UnityEngine;
    using ICities;
    using System.Collections.Generic;

    public static class Util {
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

        public static PluginManager.PluginInfo GetUUIPlugin() =>
            Plugins.FirstOrDefault(p => p.IsUUI());
        static bool IsUUI(this PluginManager.PluginInfo p) =>
            p.userModInstance.GetType().Assembly.GetType(UUI_NAME) != null;
        internal static Assembly GetUUIAssembly() =>
            GetUUIPlugin().userModInstance.GetType().Assembly;

        internal delegate UIComponent RegisterHandler
            (string name, string groupName, string tooltip, string spritefile, Action onToggle, Action<ToolBase> onToolChanged = null);

        public static UIComponent RegisterButton(
            string name, string groupName, string tooltip, string spritefile, Action onToggle, Action<ToolBase> onToolChanged = null) {
            var asm = GetUUIAssembly();
            if (asm == null)
                return null;
            var tUUI = GetUUIAssembly()
                .GetType(UUI_NAME, throwOnError: true);
            var Register = CreateDelegate<RegisterHandler>(tUUI, "Register");
            return Register(name, groupName, spritefile, tooltip, onToggle, onToolChanged);
        }
        public static void DestroyButton(UIComponent button) => GameObject.Destroy(button?.gameObject);

        public static string GetModPath(IUserMod userModInstance) =>
            Plugins.FirstOrDefault(p => p?.userModInstance == userModInstance)?.modPath;
    }
}
