namespace UnitedUI.LifeCycle
{
    using ColossalFramework.UI;
    using KianCommons;
    using UnitedUI.GUI;
    using UnityEngine;
    using Util;

    public static class LifeCycle
    {
        public static void Load()
        {
            Log.Info("LifeCycle.Load() called");
            PluginUtil.Init();
            //HarmonyExtension.InstallHarmony();
            UIView.GetAView().AddUIComponent(typeof(MainPanel));
            UIView.GetAView().AddUIComponent(typeof(FloatingButton));
        }

        public static void Release() {
            Log.Info("LifeCycle.Release() called");

            FloatingButton.Instance?.Hide();
            Object.Destroy(FloatingButton.Instance);

            MainPanel.Instance?.Hide();
            Object.Destroy(MainPanel.Instance);

            //HarmonyExtension.UninstallHarmony();
        }

        static void Destroy<T>() where T : MonoBehaviour {
            Object obj = (Object)UIView.GetAView().gameObject.GetComponent<T>();
            if (obj != null) {
                Object.Destroy(obj);
            }
        }
    }
}
