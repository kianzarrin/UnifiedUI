namespace UnifiedUI.Helpers {
    using ColossalFramework.UI;
    using System.Reflection;
    using UnityEngine;

    public class UUICustomButton {
        internal UUICustomButton(UIComponent component) => Button = component;

        public UIComponent Button { get; private set; }

        PropertyInfo pIsActive_ => Button.GetType().GetProperty("IsActive");

        public bool IsPressed {
            get => (bool)pIsActive_.GetValue(Button, null);
            set => pIsActive_.SetValue(Button, value, null);
        }

        public void Release() => GameObject.Destroy(Button?.gameObject);
    }
}
