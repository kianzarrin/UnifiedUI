using ColossalFramework.UI;
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


        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            base.OnUpdate(realTimeDelta, simulationTimeDelta);
            try {
                if (!LoadingManager.instance.m_loadingComplete) return;
                //HandleEsc(); // TODO uncomment.
                CaptureToolChanged();
            } catch(Exception e) {
                Log.Exception(e);
            }
        }


        UIComponent PauseMenu { get; } = UIView.library.Get("PauseMenu");
        bool DefaultToolIsActive() => ToolsModifierControl.toolController.CurrentTool == ToolsModifierControl.GetTool<DefaultTool>();
        int framesESC = 0;
        void HandleEsc() {
            if (PauseMenu?.isVisible == true && !DefaultToolIsActive()){
                if (framesESC >= 1) {
                    UIView.library.Hide("PauseMenu");
                    ToolsModifierControl.SetTool<DefaultTool>();
                } else {
                    framesESC++;
                }
            } else {
                framesESC = 0;
            }
        }

        public delegate void ToolChangedHandler(ToolBase newTool);
        public static event ToolChangedHandler EventToolChanged;
        ToolBase prevTool;
        void CaptureToolChanged() {
            var currentTool = ToolsModifierControl.toolController.CurrentTool;
            if (!currentTool.enabled)
                Log.DebugWait($"WARNING: currentTool({currentTool}) is disabled", seconds: 1f);
            if (currentTool != prevTool) {
                if (EventToolChanged == null)
                    Log.Info("WARNING: EventToolChanged==null");
                //Log.Debug($"ThreadingExtension.OnUpdate(): invoking EventToolChanged. currentTool={currentTool} prevTool={prevTool}");
                prevTool = currentTool;
                EventToolChanged?.Invoke(currentTool);
            }
        }
    }
}
