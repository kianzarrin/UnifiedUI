namespace UnifiedUI.LifeCycle {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using KianCommons;
    using UnifiedUI.Helpers;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using UnityEngine;

    public static class Test {
        static UIComponent button_;
        static ToolBase tool_;
        static UUISprites Sprites => UUISprites.CreateFromFile(LifeCycle.Instance.GetFullPath("A.png"));
        static Texture2D Sprite => UUIHelpers.LoadTexture(LifeCycle.Instance.GetFullPath("A1.png"));

        public class UUITestTool: ToolBase { }

        [Conditional("DEBUG")]
        public static void Run() {
            Log.Called();
            tool_ = ToolsModifierControl.toolController.gameObject.AddComponent<UUITestTool>();
            button_ = UUIHelpers.RegisterToolButton("Tes1t", null, "test1", tool_, Sprites);
        }

        [Conditional("DEBUG")]
        public static void Run2() {
            Log.Called();
            tool_ = ToolsModifierControl.toolController.gameObject.AddComponent<UUITestTool>();
            var button = UUIHelpers.RegisterCustomButton("Test2", null, "test2", Sprites, onToggle: (bool val) => { } );
        }

        [Conditional("DEBUG")]
        public static void Run3() {
            Log.Called();
            tool_ = ToolsModifierControl.toolController.gameObject.AddComponent<UUITestTool>();
            button_ = UUIHelpers.RegisterToolButton("Test3", null, "test3", tool_, Sprite);
        }

    }
}

