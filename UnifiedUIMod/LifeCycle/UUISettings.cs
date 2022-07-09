namespace UnifiedUI.LifeCycle {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnifiedUI.GUI;
    using UnifiedUI.Util;
    using System.Reflection;
    using UnifiedUI.Helpers;
    using UnifiedUI.Tool;

    public static class UUISettings {
        public const string CONFLICTS_PANEL_NAME = "Conflicts_keymapping";

        public static bool InUUIConflictPanel(this UIComponent c) {
            return c.GetComponentInParent<UIKeymappingsPanel>()?.name == CONFLICTS_PANEL_NAME;
        }

        static UUISettings() {
            // Creating setting file - from SamsamTS
            if (GameSettings.FindSettingsFileByName(MainPanel.FileName) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = MainPanel.FileName } });
            }
        }

        public readonly static SavedBool HideOriginalButtons = new SavedBool("HideOriginalButtons", MainPanel.FileName, true, true);
        public readonly static SavedBool HandleESC = new SavedBool("HandleESC", MainPanel.FileName, true, true);


        public static List<SavedInputKey> DisabledKeys = new List<SavedInputKey>();

        public static void ReviveDisabledKeys() {
            foreach(var key in DisabledKeys)
                ReflectionHelpers.SetFieldValue(key, "m_AutoUpdate", true);
            DisabledKeys.Clear();
        }

        public static void Reset() {
            MainPanel.RowInstance_?.Close();
            HideOriginalButtons.value = true;
            HandleESC.value = true;
            UUIGrabberData.ResetSettings();
            FloatingButton.ResetSettings();
            MainPanel.ResetSettings();
            MultiRowPanel.ResetSettings();
        }

        public static void OnSettingsUI(UIHelper helper) {
            try {
                Log.Debug(Environment.StackTrace, false);

                helper.AddButton("Reset", Reset);


                //var showCheckBox2 = helper.AddCheckbox(
                //    "Handle ESC key (esc key exits current tool if any).",
                //    HandleESC,
                //    val => {
                //        HandleESC.value = val;
                //        Log.Info("HandleESC set to " + val);
                //    }) as UICheckBox;

                if (!Helpers.InStartupMenu) {
                    var g1 = helper.AddGroup("Conflicts") as UIHelper;
                    (g1.self as UIComponent).eventVisibilityChanged += (c, __) => {
                        if(c.isVisible)
                            Collisions(g1);
                    };
                    Collisions(g1);
                }

                helper.AddSavedToggle("Activate hotkeys on release", KeyExtensions.KeyUpActivated).tooltip =
                    "on : hotkeys work when releasing the button.\n" +
                    "off: hotkeys work when pressing  the button.";
                helper.AddSavedClampedIntTextfield("Number of columns", MultiRowPanel.Cols, 0, 100, TryRefresh);
                helper.AddSavedToggle("put Separator between groups", MultiRowPanel.GroupSeperator, TryRefresh);
                helper.AddSavedToggle("Hold control to drag", MainPanel.ControlToDrag, TryRefresh);
                helper.AddSavedToggle("Switch to previous tool on disable", MainPanel.SwitchToPrevTool);
                helper.AddSavedToggle("Clear info panels when tool changes", MainPanel.ClearInfoPanelsOnToolChanged);

                {
                    var g2 = helper.AddGroup("legacy") as UIHelper;
                    var hideCheckBox = g2.AddSavedToggle(
                        "Hide original activation buttons (legacy)",
                        HideOriginalButtons,
                        TryRefresh);
                    hideCheckBox.tooltip = "this feature is stale only works on a few mods. each mod owner should handle this independently.";
                }
            } catch(Exception e) {
                Log.Exception(e);
            }
        }

        static void TryRefresh() {
            if (MainPanel.Exists)
                MainPanel.Instance.RearrangeIfOpen();
        }

        public static void Collisions(UIHelper helper) {
            try {
                Log.Debug(Environment.StackTrace);
                if(MainPanel.Instance is MainPanel mainPanel) {

                    var keys = new List<SavedInputKey>();
                    foreach(var b in mainPanel.ModButtons) {
                        if(b?.ActivationKey != null)
                            keys.Add(b.ActivationKey);
                    }
                    keys.AddRange(mainPanel.CustomHotkeys.Keys);

                    // clear group.
                    foreach(var c in (helper.self as UIPanel).components)
                        GameObject.Destroy(c.gameObject);

                    // add conflicts:
                    var keymappingsPanel = helper.AddKeymappingsPanel();
                    keymappingsPanel.component.name = CONFLICTS_PANEL_NAME;

                    for(int i1 = 0; i1 < keys.Count; ++i1) {
                        for(int i2 = i1 + 1; i2 < keys.Count; ++i2) {
                            var key1 = keys[i1];
                            var key2 = keys[i2];
                            var conflict = key1 != key2 && key1.value == key2.value;
                            if(conflict) {
                                var file1 = (string)ReflectionHelpers.GetFieldValue(key1, "m_FileName");
                                var file2 = (string)ReflectionHelpers.GetFieldValue(key2, "m_FileName");
                                var b1 = keymappingsPanel.AddKeymapping($"{file1}.{key1.name}", key1);
                                var b2 = keymappingsPanel.AddKeymapping($"{file2}.{key2.name}", key2);
                                b1.eventKeyDown += OnConflictResolved;
                                b1.eventMouseDown += OnConflictResolved;
                                b2.eventKeyDown += OnConflictResolved;
                                b2.eventMouseDown += OnConflictResolved;
                                Log.Warning($"Collision Detected: " +
                                    $"{file1}.{key1.name}:'{key1}' collides with " +
                                    $"{file2}.{key2.name}:'{key2}'");
                            }
                        }
                    }

                    if(keymappingsPanel.GetComponentsInChildren<UIButton>().IsNullorEmpty())
                        keymappingsPanel.component.AddUIComponent<UILabel>().text = "None";
                }
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        static void OnConflictResolved(UIComponent c, UIComponentEventParameter p) {
            try {
                var mi =
                    c?.objectUserData
                    ?.GetType()
                    ?.GetMethod(nameof(UnsavedInputKey.OnConflictResolved), types: new Type [0] ,throwOnError: false);
                mi?.Invoke(c.objectUserData, null);
            } catch(Exception ex) {
                ex.Log(false);
            }
        }
    }
}
