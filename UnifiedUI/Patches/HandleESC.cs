using KianCommons;
using ColossalFramework.UI;
using HarmonyLib;
using System;
using System.Reflection;

namespace UnifiedUI.Patches {
    [HarmonyPatch]
    public static class HandleESC {
        public static void Refresh() {
            pauseMenu_ = null;
            defaultTool_ = null;
        }
        static UIComponent pauseMenu_;
        static DefaultTool defaultTool_;
        static UIComponent optionsContainer_;
        static UIComponent PauseMenu =>
            pauseMenu_ = pauseMenu_ ? pauseMenu_ : UIView.library.Get("PauseMenu");
        static DefaultTool DefaultTool => defaultTool_ =
            defaultTool_ ? defaultTool_ : ToolsModifierControl.GetTool<DefaultTool>();
        static UIComponent OptionsContainer =>
            optionsContainer_ = optionsContainer_ ? optionsContainer_ :UIView.Find<UITabContainer>("OptionsContainer");

        public static MethodBase TargetMethod() =>
              AccessTools.DeclaredMethod(typeof(ToolBase), "Update") ??
              AccessTools.DeclaredMethod(typeof(ToolBase), "FpsBoosterUpdate");

        static void Postfix(ToolBase __instance) {
            try {
                if (!GUI.Settings.HandleESC) return;
                if(OptionsContainer.isVisible) return;
                if (PauseMenu?.isVisible == true && __instance != DefaultTool) {
                    Log.Info($"{__instance} did not handle ESC key. Unified UI turns it off instead.", true);
                    ToolsModifierControl.SetTool<DefaultTool>();
                    UIView.library.Hide("PauseMenu");
                }
            }
            catch (Exception e){
                Log.Exception(e);
            }
        }
    }
}
