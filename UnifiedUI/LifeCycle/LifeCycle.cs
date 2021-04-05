namespace UnifiedUI.LifeCycle {
    using ColossalFramework.UI;
    using ICities;
    using KianCommons;
    using KianCommons.IImplict;
    using KianCommons.Plugins;
    using UnifiedUI.GUI;
    using UnifiedUI.GUI.ModButtons;
    using static KianCommons.ReflectionHelpers;


    public class LifeCycle : LifeCycleBase, IModWithSettings, IUserMod {
        public override string Name => "Unified UI " + VersionString;
        public override string Description => "organsized UI for some other mods into one unified place.";

        public override void Load() {
            LogCalled();
            KianCommons.UI.TextureUtil.EmbededResources = false;
            Log.VERBOSE = false;

            if(PluginUtil.GetNetworkDetective().IsActive())
                MainPanel.Instance.AddButton<NetworkDetectiveButton>();

            if(PluginUtil.GetIMT().IsActive())
                MainPanel.Instance.AddButton<IntersectionMarkingButton>();

            if(PluginUtil.GetRAB().IsActive())
                MainPanel.Instance.AddButton<RoundaboutBuilderButton>();

            if(PluginUtil.GetPedestrianBridge().IsActive())
                MainPanel.Instance.AddButton<PedestrianBridgeButton>();

            if(PluginUtil.GetNodeController().IsActive())
                MainPanel.Instance.AddButton<NodeControllerButton>();

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
