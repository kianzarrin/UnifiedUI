namespace UnifiedUI.LifeCycle
{
    using ColossalFramework.UI;
    using KianCommons;
    using UnifiedUI.GUI;
    using UnityEngine;
    using KianCommons.Patches;

    using PluginUtil = Util.PluginUtil;

    public static class LifeCycle
    {
        public static string HARMONY_ID = "CS.Kian.UnifiedUI";
        public static void Load()
        {
            Log.Info("LifeCycle.Load() called");
            PluginUtil.Init();
            HarmonyUtil.InstallHarmony(HARMONY_ID);
            UIView.GetAView().AddUIComponent(typeof(MainPanel));
            UIView.GetAView().AddUIComponent(typeof(FloatingButton));
        }

        public static void Release() {
            Log.Info("LifeCycle.Release() called");

            FloatingButton.Instance?.Hide();
            Object.Destroy(FloatingButton.Instance);

            MainPanel.Instance?.Hide();
            Object.Destroy(MainPanel.Instance);

            HarmonyUtil.UninstallHarmony(HARMONY_ID);
        }

        static void Destroy<T>() where T : MonoBehaviour {
            Object obj = (Object)UIView.GetAView().gameObject.GetComponent<T>();
            if (obj != null) {
                Object.Destroy(obj);
            }
        }
    }
}
