using KianCommons;
using ColossalFramework.UI;
using HarmonyLib;
using System;

namespace UnifiedUI.Patches {
    [HarmonyPatch(typeof(ToolBase), "Update")]
    static class HandleESC {
        static UIComponent PauseMenu { get; } = UIView.library.Get("PauseMenu");
        static DefaultTool defaultTool_ => ToolsModifierControl.GetTool<DefaultTool>();

        static void Postfix(ToolBase __instance) {
            try {
                if (!GUI.Settings.HandleESC) return;
                if (PauseMenu?.isVisible == true && __instance != defaultTool_) {
                    Log.Info($"{__instance} did not handle ESC key. Unified UI turns it off instead.",true);
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
