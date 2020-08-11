using ICities;
using KianCommons;
using System;

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

        public delegate void ToolChangedHandler(ToolBase newTool);
        public static event ToolChangedHandler EventToolChanged;

        ToolBase prevTool;
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            base.OnUpdate(realTimeDelta, simulationTimeDelta);
            try {
                var currentTool = ToolsModifierControl.toolController.CurrentTool;
                if (currentTool != prevTool) {
                    prevTool = currentTool;
                    Log.Debug($"OnUpdate(): invoking EventToolChanged (EventToolChanged==null : {EventToolChanged == null})");
                    EventToolChanged?.Invoke(currentTool);
                }
            } catch(Exception e) {
                Log.Error(e.ToString() + "\n\t  ----logged at-----");
                throw e;
            }
        }
    }
}
