namespace KianCommons {
    using CitiesHarmony.API;
    using HarmonyLib;
    using System.Reflection;
    using System;
    using System.Runtime.CompilerServices;

    public static class HarmonyUtil {
        static bool harmonyInstalled_ = false;
        public static void AssertCitiesHarmonyInstalled() {
            if (!HarmonyHelper.IsHarmonyInstalled) {
                string m =
                    "****** ERRRROOORRRRRR!!!!!!!!!! **************\n" +
                    "**********************************************\n" +
                    "    HARMONY MOD DEPENDANCY IS NOT INSTALLED!\n\n" +
                    "solution:\n" +
                    " - exit to desktop.\n" +
                    " - unsub harmony mod.\n" +
                    " - make sure harmony mod is deleted from the content folder\n" +
                    " - resub to harmony mod.\n" +
                    " - run the game again.\n" +
                    "**********************************************\n" +
                    "**********************************************\n";
                Log.Error(m);
                throw new Exception(m);
            }
        }

        public static void InstallHarmony(string harmonyID) {
            if (harmonyInstalled_) {
                Log.Info("skipping harmony installation because its already installed");
                return;
            }
            AssertCitiesHarmonyInstalled();
            Log.Info("Patching...");
            PatchAll(harmonyID);
            harmonyInstalled_ = true;
            Log.Info("Patched.");
        }

        /// <summary>
        /// assertion shall take place in a function that does not refrence Harmony.
        /// </summary>
        /// <param name="harmonyID"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void PatchAll(string harmonyID) {
            var harmony = new Harmony(harmonyID);
            harmony.PatchAll();
        }

        public static void UninstallHarmony(string harmonyID) {
            AssertCitiesHarmonyInstalled();
            Log.Info("UnPatching...");
            UnpatchAll(harmonyID);
            harmonyInstalled_ = false;
            Log.Info("UnPatched.");
        }

        /// <summary>
        /// assertion shall take place in a function that does not refrence Harmony.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void UnpatchAll(string harmonyID) {
            var harmony = new Harmony(harmonyID);
            harmony.UnpatchAll(harmonyID);
        }
    }
}