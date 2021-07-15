namespace UnifiedUI.LifeCycle {
    using ColossalFramework.UI;
    using ICities;
    using KianCommons;
    using KianCommons.IImplict;
    using KianCommons.Plugins;
    using UnifiedUI.GUI;
    using UnifiedUI.GUI.ModButtons;
    using static KianCommons.ReflectionHelpers;
    using ColossalFramework.Plugins;
    using System;

    static class Extension {
        internal static bool IsActive(this PluginManager.PluginInfo p, string maxVersion) =>
            p.IsActive() && p.userModInstance.VersionOf() < new Version(maxVersion);
    }

    public class LifeCycle : LifeCycleBase, IModWithSettings, IUserMod {
        public override string Name => "Unified UI " + VersionString;
        public override string Description => "organsized UI for some other mods into one unified place.";


        public override void Load() {
            LogCalled();
            KianCommons.UI.TextureUtil.EmbededResources = false;
            Log.VERBOSE = false;

            if (PluginUtil.GetNetworkDetective().IsActive("1.1"))
                MainPanel.Instance.AddButton<NetworkDetectiveButton>();

            if(PluginUtil.GetIMT().IsActive(" 1.7.5"))
                MainPanel.Instance.AddButton<IntersectionMarkingButton>();

            if(PluginUtil.GetRAB().IsActive("1.10"))
                MainPanel.Instance.AddButton<RoundaboutBuilderButton>();

            if(PluginUtil.GetPedestrianBridge().IsActive("2.1"))
                MainPanel.Instance.AddButton<PedestrianBridgeButton>();

            var nc = PluginUtil.GetNodeController();
            if(nc.IsActive())
            {
                var version = nc.userModInstance.VersionOf();
                bool create2 = version.Major == 2 && version.Minor < 3;
                bool create3 = version.Major == 3 && version < new Version("3.1.1");
                if (create2 || create3)
                    MainPanel.Instance.AddButton<NodeControllerButton>();
            }

            //Test.Run();
            //Test.Run2();
        }

        public override void UnLoad() {
            LogCalled();
            MainPanel.Release();
            UUISettings.ReviveDisabledKeys();
        }

        public static bool HasMainPanel => UIView.GetAView().GetComponentInChildren<MainPanel>();

        public static void Release() {
            Log.Info("LifeCycle.Release() called");
            MainPanel.Release();
            UUISettings.ReviveDisabledKeys();
        }

        public void OnSettingsUI(UIHelper helper) => UUISettings.OnSettingsUI(helper);
    }
}
