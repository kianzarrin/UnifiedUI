namespace UnifiedUI.API {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ColossalFramework.UI;
    using UnityEngine;
    using UnifiedUI.GUI;

    public class UUIMod : IUUIMod {
        private static UUIMod instance_;
        public UUIMod Instance => instance_ ??= new UUIMod();

        public MonoBehaviour Register(IUUIButton button) =>
            MainPanel.Instance.Register(button);
    }
}
