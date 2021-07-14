namespace UnifiedUI.LifeCycle {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using KianCommons;
    using UnifiedUI.Helpers;
    using ColossalFramework.UI;
    public static class Test {
        static UIComponent button_;
        static ToolBase tool_;

        public class UUITestTool: ToolBase { }

        [Conditional("DEBUG")]
        public static void Run() {
            Log.Called();
            tool_ = ToolsModifierControl.toolController.gameObject.AddComponent<UUITestTool>();
            var sprites = UUISprites.CreateFromFile(LifeCycle.Instance.GetFullPath("uui_imt.png"));
            button_ = UUIHelpers.RegisterToolButton("Test", null, "test", sprites, tool_);
        }
    }
}

