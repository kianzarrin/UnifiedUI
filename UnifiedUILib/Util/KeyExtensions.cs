namespace UnifiedUILib.Util {
    using ColossalFramework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnifiedUI.GUI;
    using UnityEngine;

    public static class KeyExtensions {
        public readonly static SavedBool KeyUpActivated =
            new SavedBool("KeyUpActivated", MainPanel.FileName, false, true);
        public static bool KeyActivated(this SavedInputKey key) =>
            KeyUpActivated ? key.IsKeyUp() : key.IsKeyDown();

        public static bool IsKeyDown(this SavedInputKey key) {
            int num = key.value;
            KeyCode keyCode = (KeyCode)(num & 268435455);
            return
                keyCode != KeyCode.None &&
                Input.GetKeyDown(keyCode) &&
                (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) == ((num & 1073741824) != 0) &&
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) == ((num & 536870912) != 0) &&
                (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr)) == ((num & 268435456) != 0);
        }
    }
}
