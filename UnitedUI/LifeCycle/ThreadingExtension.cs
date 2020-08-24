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
                    if (EventToolChanged == null)
                        Log.Info("WARNING: EventToolChanged==null");
                    //Log.Debug($"ThreadingExtension.OnUpdate(): invoking EventToolChanged. currentTool={currentTool} prevTool={prevTool}");
                    prevTool = currentTool;
                    EventToolChanged?.Invoke(currentTool);
                }
            } catch(Exception e) {
                Log.Error(e.ToString() + "\n\t  ----logged at-----");
                throw e;
            }
        }
    }
}
