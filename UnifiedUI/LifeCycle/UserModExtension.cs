namespace UnifiedUI.LifeCycle
{
    using System;
    using JetBrains.Annotations;
    using ICities;
    using CitiesHarmony.API;
    using KianCommons;
    public class UserModExtension : IUserMod
    {
        public static Version ModVersion => typeof(UserModExtension).Assembly.GetName().Version;
        public static string VersionString => ModVersion.ToString(2);
        public string Name => "Unified UI " + VersionString;
        public string Description => "organsized UI for some other mods into one unified place.";

        [UsedImplicitly]
        public void OnEnabled() => LifeCycle.Enable();
        

        [UsedImplicitly]
        public void OnDisabled() => LifeCycle.Disable();

        [UsedImplicitly]
        public void OnSettingsUI(UIHelper helper) => GUI.UUISettings.OnSettingsUI(helper);
    }
}
