namespace UnitedUI.LifeCycle
{
    using 

    public static class LifeCycle
    {
        public static void Load()
        {
            Log.Info("LifeCycle.Load() called");
            PluginUtil.Init();
            HarmonyExtension.InstallHarmony();
        }

        public static void Release()
        {
            Log.Info("LifeCycle.Release() called");
            HarmonyExtension.UninstallHarmony();
        }
    }
}
