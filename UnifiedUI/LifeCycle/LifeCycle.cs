namespace UnifiedUI.LifeCycle {
    //using CitiesHarmony.API;
    using ColossalFramework.UI;
    using ICities;
    using KianCommons;
    using System.Diagnostics;
    using UnifiedUI.GUI;
    using KianCommons.Plugins;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnifiedUI.GUI.ModButtons;

    public static class LifeCycle {
        public static string HARMONY_ID = "CS.Kian.UnifiedUI";

        public static SimulationManager.UpdateMode UpdateMode => SimulationManager.instance.m_metaData.m_updateMode;
        public static LoadMode Mode => (LoadMode)UpdateMode;
        public static string Scene => SceneManager.GetActiveScene().name;

        public static void Enable() {
            Log.Debug("Testing StackTrace:\n" + new StackTrace(true).ToString(), copyToGameLog: false);
            KianCommons.UI.TextureUtil.EmbededResources = false;
            Log.VERBOSE = false;

            if(HelpersExtensions.InGameOrEditor)
                HotReload();
        }

        public static void Disable() {
            Release(); // in case of hot unload
        }

        public static void OnLevelUnloading() {
            Release(); // called when loading new game or exiting to main menu.
        }

        public static void HotReload() {
            Load();
        }

        public static void Load() {
            Log.Info("LifeCycle.Load() called");
            PluginUtil.Init();

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

        public static bool HasMainPanel => UIView.GetAView().GetComponentInChildren<MainPanel>();

        public static void Release() {
            Log.Info("LifeCycle.Release() called");
            MainPanel.Release();
            UUISettings.ReviveDisabledKeys();
        }
    }
}
