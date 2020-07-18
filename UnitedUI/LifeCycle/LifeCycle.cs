namespace UnitedUI.LifeCycle
{
    using ColossalFramework.UI;
    using KianCommons;
    using UnitedUI.GUI;
    using UnityEngine;

    public static class LifeCycle
    {
        public static void Load()
        {
            Log.Info("LifeCycle.Load() called");
            PluginUtil.Init();
            //HarmonyExtension.InstallHarmony();
            UIView.GetAView().gameObject.AddComponent<FloatingButton>();
        }

        public static void Release()
        {
            Log.Info("LifeCycle.Release() called");
            Destroy<FloatingButton>();
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
