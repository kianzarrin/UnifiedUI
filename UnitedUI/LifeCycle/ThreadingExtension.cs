using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;

namespace UnitedUI.LifeCycle {
    public class ThreadingExtension: ThreadingExtensionBase {
        public static ThreadingExtension Instance { get; private set; }
        public override void OnCreated(IThreading threading) {
            base.OnCreated(threading);
            Instance = this;
            prevTool = null;
            EventToolChanged = null;
        }
        public override void OnReleased() {
            base.OnReleased();
            Instance = null;
            EventToolChanged = null;
        }

        public delegate bool ToolChangedHandler(ToolBase newTool);
        public static event ToolChangedHandler EventToolChanged;

        ToolBase prevTool;
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            base.OnUpdate(realTimeDelta, simulationTimeDelta);
            var currentTool = ToolsModifierControl.toolController.CurrentTool;
            if (currentTool != prevTool) {
                prevTool = currentTool;
                EventToolChanged?.Invoke(currentTool);
            }
        }
    }
}
