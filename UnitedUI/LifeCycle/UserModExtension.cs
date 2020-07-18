namespace UnitedUI.LifeCycle
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
        public string Name => "United UI" + VersionString;
        public string Description => "organsized UI for some other mods into one place.";

        [UsedImplicitly]
        public void OnEnabled() {
            HarmonyHelper.EnsureHarmonyInstalled();
            if (HelpersExtensions.InGame)
                LifeCycle.Load();
        }

        [UsedImplicitly]
        public void OnDisabled() {
            LifeCycle.Release();
        }

        [UsedImplicitly]
        public void OnSettingsUI(UIHelperBase helper) {
            GUI.Settings.OnSettingsUI(helper);
        }
    }
}
