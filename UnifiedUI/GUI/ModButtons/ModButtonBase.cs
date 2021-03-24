namespace UnifiedUI.GUI
{
    using ColossalFramework.UI;
    using KianCommons.UI;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;
    using UnityEngine;
    using UnifiedUI.LifeCycle;
    using ColossalFramework.Plugins;
    using ColossalFramework;
    using System.Collections.Generic;
    using System;

    public abstract class ModButtonBase : ButtonBase
    {
        protected override void OnClick(UIMouseEventParameter p) {
            Log.Debug(ThisMethod + " called  for " + Name, false);
            base.OnClick(p);
            Toggle();
        }
    }
}
