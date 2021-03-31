namespace UnifiedUI.LifeCycle {
    //using CitiesHarmony.API;
    using ColossalFramework.UI;
    using ICities;
    using KianCommons;
    using System.Diagnostics;
    using UnifiedUI.GUI;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using PluginUtil = Util.PluginUtil;

    public static class LifeCycle {
        public static string HARMONY_ID = "CS.Kian.UnifiedUI";

        public static SimulationManager.UpdateMode UpdateMode => SimulationManager.instance.m_metaData.m_updateMode;
        public static LoadMode Mode => (LoadMode)UpdateMode;
        public static string Scene => SceneManager.GetActiveScene().name;

        public static void Enable() {
            Log.Debug("Testing StackTrace:\n" + new StackTrace(true).ToString(), copyToGameLog: false);
            KianCommons.UI.TextureUtil.EmbededResources = false;
            HelpersExtensions.VERBOSE = false;

            //HarmonyHelper.EnsureHarmonyInstalled();
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
            //HarmonyUtil.InstallHarmony(HARMONY_ID);
            var uiView = UIView.GetAView();
            if(!uiView.GetComponentInChildren<MainPanel>())
                uiView.AddUIComponent(typeof(MainPanel));
            if(!uiView.GetComponentInChildren<FloatingButton>())
                uiView.AddUIComponent(typeof(FloatingButton));
            
        }

        public static bool HasMainPanel => UIView.GetAView().GetComponentInChildren<MainPanel>();

        public static void Release() {
            Log.Info("LifeCycle.Release() called");

            FloatingButton.Instance?.Hide();
            Object.DestroyImmediate(FloatingButton.Instance);

            MainPanel.Instance?.Hide();
            Object.DestroyImmediate(MainPanel.Instance);

            Settings.ReviveDisabledKeys();

            //HarmonyUtil.UninstallHarmony(HARMONY_ID);
        }
    }
}
