namespace UnifiedUI.Helpers {
    using ColossalFramework.UI;
    using System.Reflection;
    using UnityEngine;

    public class UUICustomButton {
        internal UUICustomButton(UIComponent component) => Button = component;

        /// <summary>
        /// UUI button. you can hide this if necessary.
        /// </summary>
        public UIComponent Button { get; private set; }

        PropertyInfo pIsActive_ => Button.GetType().GetProperty("IsActive");

        /// <summary>
        /// Use this to change buttons state when mod is closed externally (without clicking on the UUI button).
        /// </summary>
        public bool IsPressed {
            get => (bool)pIsActive_.GetValue(Button, null);
            set => pIsActive_.SetValue(Button, value, null);
        }

        public void Release() {
            Button = null;
            Button?.Destroy();
        }
    }
}
