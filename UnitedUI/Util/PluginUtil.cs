
namespace UnitedUI.Util {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using ColossalFramework.Plugins;
    public static class PluginUtil {
        internal static bool CSUREnabled;
        static bool IsCSUR(PluginManager.PluginInfo current) =>
            current.name.Contains("CSUR ToolBox") || 1959342332 == (uint)current.publishedFileID.AsUInt64;
        public static void Init() {
            CSUREnabled = false;
            foreach (PluginManager.PluginInfo current in PluginManager.instance.GetPluginsInfo()) {
                if (!current.isEnabled) continue;
                if (IsCSUR(current)) {
                    CSUREnabled = true;
                    Log.Debug(current.name + "detected");
                    return;
                }
            }
        }
    }
}
