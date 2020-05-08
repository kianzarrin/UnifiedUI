namespace UnitedUI.LifeCycle
{
    using UnitedUI.Util;

    public static class LifeCycle
    {
        public static void Load()
        {
            Log.Info("LifeCycle.Load() called");
            PluginUtil.Init();
            HarmonyExtension.InstallHarmony();
            SerializableDataExtension.LoadGlobalConfig();
        }

        public static void Release()
        {
            Log.Info("LifeCycle.Release() called");
            HarmonyExtension.UninstallHarmony();
            SerializableDataExtension.SaveGlobalConfig();
        }
    }
}
