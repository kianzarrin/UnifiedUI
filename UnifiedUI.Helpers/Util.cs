namespace UnifiedUI.API {
    using ColossalFramework.Plugins;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using ICities;
    using System.Collections.Generic;

    public static class Util {
        const string UUI_NAME = "UnifiedUI.API.UUIMod";

        static IEnumerable<PluginManager.PluginInfo> Plugins => PluginManager.instance.GetPluginsInfo();

        public static PluginManager.PluginInfo GetUUIPlugin() =>
            Plugins.FirstOrDefault(p => p.IsUUI());
        static bool IsUUI(this PluginManager.PluginInfo p) =>
            p.userModInstance.GetType().Assembly.GetType(UUI_NAME) != null;
        public static Assembly GetUUIAssembly() =>
            GetUUIPlugin().userModInstance.GetType().Assembly;

        public static IUUIMod GetUUIMod() {
            return GetUUIAssembly()
                ?.GetType(UUI_NAME)
                .GetMethod("get_Instance")
                .Invoke(null, null)
                as IUUIMod;
        }

        public static MonoBehaviour RegisterButton(IUUIButton button) => GetUUIMod().Register(button);
        public static void DestroyButton(MonoBehaviour button) => GameObject.Destroy(button?.gameObject);

        public static string GetModPath(IUserMod userModInstance) =>
            Plugins.FirstOrDefault(p => p?.userModInstance == userModInstance)?.modPath;


    }
}
