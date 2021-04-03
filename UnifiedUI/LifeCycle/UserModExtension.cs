namespace UnifiedUI.LifeCycle
{
    using System;
    using ICities;
    using KianCommons.IImplict;
    public class UserModExtension : IModWithSettings
    {
        public static Version ModVersion => typeof(UserModExtension).Assembly.GetName().Version;
        public static string VersionString => ModVersion.ToString(2);
        public string Name => "Unified UI " + VersionString;
        public string Description => "organsized UI for some other mods into one unified place.";

        public void OnEnabled() => LifeCycle.Enable();
        public void OnDisabled() => LifeCycle.Disable();
        public void OnSettingsUI(UIHelper helper) => UUISettings.OnSettingsUI(helper);
    }
}
