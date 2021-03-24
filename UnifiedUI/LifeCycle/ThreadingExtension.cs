using ColossalFramework.UI;
using ICities;
using KianCommons;
using System;

namespace UnifiedUI.LifeCycle {
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


        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            base.OnUpdate(realTimeDelta, simulationTimeDelta);
            try {
                if (!LoadingManager.instance.m_loadingComplete) return;
                UnifiedUI.GUI.MainPanel.Instance.HandleHotkeys();
                CaptureToolChanged();
            } catch(Exception e) {
                Log.Exception(e);
            }
        }

        public delegate void ToolChangedHandler(ToolBase newTool);
        public static event ToolChangedHandler EventToolChanged;
        ToolBase prevTool;
        void CaptureToolChanged() {
            var currentTool = ToolsModifierControl.toolController.CurrentTool;
            if(!currentTool) return;  // tool is being destroyed. do not poke around!
            if (!currentTool.enabled)
                Log.DebugWait($"WARNING: currentTool({currentTool}) is disabled", seconds: 1f);
            if (currentTool != prevTool) {
                if (EventToolChanged == null)
                    Log.Info("WARNING: EventToolChanged==null");
                //Log.Debug($"ThreadingExtension.OnUpdate(): invoking EventToolChanged. currentTool={currentTool} prevTool={prevTool}");
                prevTool = currentTool;
                EventToolChanged?.Invoke(currentTool);
                Patches.HandleESC.Refresh();
            }
        }
    }
}
