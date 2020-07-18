namespace UnitedUI
{
    using System.Reflection;
    using HarmonyLib;
    using KianCommons;

    public static class HarmonyExtension
    {
        public static string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        public static string HARMONY_ID = "CS.Kian." + AssemblyName; 

        public static void InstallHarmony()
        {
            Log.Info("Patching...");
            var harmony = new Harmony(HARMONY_ID);
            harmony.PatchAll();
            Log.Info("Patched.");
        }

        public static void UninstallHarmony()
        {
            Log.Info("UnPatching...");
            var harmony = new Harmony(HARMONY_ID);
            harmony.UnpatchAll(HARMONY_ID);
            Log.Info("UnPatched.");
        }
    }
}