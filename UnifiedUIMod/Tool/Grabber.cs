namespace UnifiedUI.Tool {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.Serialization;
    using UnifiedUI.GUI;
    using UnityEngine;
    using UnifiedUI.Helpers;
    using KianCommons.UI;
    using LifeCycle;
    using KianCommons.IImplict;

    public class UUIGrabberData : XMLData<UUIGrabberData>  {
        public class ButtonData {
            public string Name;
            public string FullTypeName;
            public string[] Parents;

            public static string [] GetParents(UIComponent parent) {
                List<string> parents = new List<string>();
                for (; parent != null; parent = parent.parent) {
                    parents.Add(parent.name);
                }
                return parents.ToArray();
            }

            public static ButtonData Create(UIButton button) {
                return new ButtonData {
                    Name = button.name,
                    FullTypeName = button.GetType().FullName,
                    Parents = GetParents(button.parent),
                };
            }
            public static ButtonData Create(UIButton button, UIComponent parent) {
                return new ButtonData {
                    Name = button.name,
                    FullTypeName = button.GetType().FullName,
                    Parents = GetParents(parent),
                };
            }

            public bool Matches(UIButton button) {
                var data2 = Create(button);
                var ret = Matches(data2);
                if (ret) {
                    Log.Debug($"buton={button} | {this} matches {data2}", false);
                }
                return ret;
            }

            public bool Matches(ButtonData data2) {
                return
                    Name == data2.Name &&
                    FullTypeName == data2.FullTypeName &&
                    Parents.SequenceEqual(data2.Parents);
            }



            public override string ToString() => $"ButtonData(Name={Name} FullTypeName={FullTypeName} Parents={Parents.ToSTR()})";
        }

        public List<ButtonData> SavedButtons = new List<ButtonData>();

        public void SaveSnapShot() {
            TakeSnapShot();
            Save();
            SavedButtons.Clear(); // prevent conflict with DelayedStart
        }

        public void TakeSnapShot() {
            SavedButtons.Clear();
            foreach (var b in Grabber.Instance.GrabbedButtons) {
                SavedButtons.Add(ButtonData.Create(b.Key, b.Value.OriginalParent));
            }
        }

        public void TryRestore() {
            Log.Called();
            try {
                var buttons = GameObject.FindObjectsOfType<UIButton>();
                foreach (var button in buttons) {
                    ButtonData remove = null;
                    foreach (var item in SavedButtons) {
                        if (item.Matches(button)) {
                            remove = item;
                            break;
                        }
                    }
                    if (remove != null) {
                        SavedButtons.Remove(remove);
                        Grabber.Instance.AddButton(button);
                    }
                }
            } catch (Exception ex) { ex.Log(); }
        }
    }

    internal sealed class Grabber : UUIToolBase<Grabber> {
        public struct Metadata {
            public UIComponent OriginalParent;
            public Vector2 OriginalSize;
            public Vector2 OriginalPos;
        }

        public Dictionary<UIButton, Metadata> GrabbedButtons = new Dictionary<UIButton, Metadata>();

        protected override void Awake() {
            try {
                Log.Called();
                base.Awake();
                var sprite = TextureUtil.GetTextureFromFile(LifeCycle.Instance.GetFullPath("pin.png"));
                UUIHelpers.RegisterToolButton("grabber", null, "grabber [EXPERIMENTAL]", this, sprite);
                _ = UUIGrabberData.Instance;
                //tool does not remained enabled on start so lets use main menu instead.
                MainPanel.Instance.StartCoroutine(DelayedStart());
            } catch (Exception ex) { ex.Log(); }
        }

        public IEnumerator DelayedStart() {
            Log.Called();

            // wait a couple of frames
            yield return 0;
            yield return 0;

            static bool MoreExists() => !((UUIGrabberData.Instance?.SavedButtons).IsNullorEmpty());
            for (int i = 1; i <= 10 && MoreExists(); ++i) {
                UUIGrabberData.Instance?.TryRestore();
                yield return new WaitForSeconds(i);
                Log.Called("i="+i);
            }
            Log.Succeeded();
        }

        protected override void OnToolGUI(Event e) {
            try {
                base.OnToolGUI(e);
                var button = HoveredButton;
                FloatingText = "move cursor to button to move UnifiedUI panel.";

                if (button) {
                    var rect = GetRect(button);
                    if (button.parent == MainPanel.Instance.MultiRowPanel) {
                        if (GrabbedButtons.TryGetValue(button, out var data)) {
                            var style = Input.GetMouseButton(1) ? OrangeRectStyle : RedRectStyle;
                            GUI.Box(rect, string.Empty, style);
                            FloatingText = "Right click => detach button";
                            if(e.type == EventType.MouseUp && e.button == 1) {
                                RemoveButton(button);
                                UUIGrabberData.Instance.SaveSnapShot();
                                e.Use();
                            }
                        } else {
                            return;
                        }
                    } else {
                        var style = Input.GetMouseButton(1) ? OrangeRectStyle : GreenRectStyle;
                        GUI.Box(rect, string.Empty, style);
                        FloatingText =
                            "Right click => Move button to UnifiedUI panel\n" +
                            "Does not always work ... but you can undo.";
                        if (e.type == EventType.MouseUp && e.button == 1) {
                            AddButton(button);
                            UUIGrabberData.Instance.SaveSnapShot();
                            e.Use();
                        }
                    }
                }
            } catch (Exception ex) { ex.Log(); }
        }

        public void AddButton(UIButton button) {
            try {
                Log.Called();
                if(button is FloatingButton) {
                    Log.Info("skip adding UUI button.");
                    return;
                }
                Assertion.NotNull(button, "button");
                GrabbedButtons[button] = new Metadata {
                    OriginalParent = button.parent,
                    OriginalSize = button.size,
                    OriginalPos = button.relativePosition,
                };

                MainPanel.Instance.AttachAlien(button, "UUI_Grabber");
                button.size = new Vector2(ButtonBase.SIZE, ButtonBase.SIZE);
                button.eventSizeChanged -= ButtonSizeChanged;
                button.eventSizeChanged += ButtonSizeChanged;

            } catch(Exception ex) { ex.Log(); }
        }

        public void RemoveButton(UIButton button) {
            try {
                Log.Called();
                Assertion.NotNull(button, "button");
                var data = GrabbedButtons[button];
                Assertion.NotNull(data, "data");
                MainPanel.Instance.MultiRowPanel.RemoveButton(button);
                button.eventSizeChanged -= ButtonSizeChanged;
                if (data.OriginalParent == null) {
                    UIView.GetAView().AttachUIComponent(button.gameObject);
                } else {
                    data.OriginalParent.AttachUIComponent(button.gameObject);
                }
                GrabbedButtons.Remove(button);
                MainPanel.Instance.RearrangeIfOpen();

                StartCoroutine(RestoreButton());

                IEnumerator RestoreButton() {
                    yield return 0;
                    button.size = data.OriginalSize;
                    button.relativePosition = data.OriginalPos;
                    yield break;
                }
            } catch (Exception ex) { ex.Log(); }
        }

        public void RemoveAll() {
            var buttons = GrabbedButtons.Keys.ToArray();
            foreach (var button in buttons) {
                RemoveButton(button);
            }
            GrabbedButtons.Clear();
        }

        bool changing_;
        void ButtonSizeChanged(UIComponent button, Vector2 __) {
            if (changing_)
                return;
            try {
                changing_ = true;
                button.size= new Vector2(ButtonBase.SIZE, ButtonBase.SIZE);
            } catch(Exception ex) {
                ex.Log();
            } finally {
                changing_ = false;
            }
        }
    }
}
