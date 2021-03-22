namespace UnifiedUI.LifeCycle
{
    using ICities;
    using KianCommons;

    public class LoadingExtention : LoadingExtensionBase
    {
        public override void OnLevelUnloading()
        {
            Log.Debug("LoadingExtention.OnLevelUnloading");
            LifeCycle.Release();
        }
    }
}
