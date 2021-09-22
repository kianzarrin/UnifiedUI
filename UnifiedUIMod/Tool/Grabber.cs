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

    public class UUIGrabberData : XMLData<UUIGrabberData>  {
        public class ButtonData {
            public string[] Parents;
            public string FullTypeName;
            public string Name;

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
                return Name ==
                    data2.Name &&
                    FullTypeName == data2.FullTypeName &&
                    Parents.SequenceEqual(data2.Parents);
            }
        }

        public List<ButtonData> Buttons;

        public void TakeSnapShot() {
            Buttons.Clear();
            foreach (var b in Grabber.Instance.Buttons) {
                Buttons.Add(ButtonData.Create(b.Key, b.Value.OriginalParent));
            }
        }

        public void TryRestore() {
            var buttons = GameObject.FindObjectsOfType<UIButton>();
            foreach (var button in buttons) {
                ButtonData remove = null;
                foreach (var item in Buttons) {
                    if (item.Matches(button)) {
                        remove = item;
                        break;
                    }
                }
                Buttons.Remove(remove);
                Grabber.Instance.AddButton(button);
            }
        }
    }

    internal class Grabber : UUIToolBase<Grabber> {
        public struct Metadata {
            public UIComponent OriginalParent;
            public Vector2 OriginalSize;
            public Vector2 OriginalPos;
        }

        public Dictionary<UIButton, Metadata> Buttons = new Dictionary<UIButton, Metadata>();

        protected override void Awake() {
            try {
                Log.Called();
                base.Awake();
                var sprite = TextureUtil.GetTextureFromFile(LifeCycle.Instance.GetFullPath("pin.png"));
                UUIHelpers.RegisterToolButton("grabber", null, "grabber", this, sprite);
            } catch(Exception ex) { ex.Log(); }
        }

        protected override void OnToolGUI(Event e) {
            try {
                base.OnToolGUI(e);
                var button = HoveredButton;

                if (button) {
                    var rect = GetRect(button);
                    if (button.parent == MainPanel.Instance.MultiRowPanel) {
                        if (Buttons.TryGetValue(button, out var data)) {
                            var style = Input.GetMouseButton(1) ? OrangeRectStyle : RedRectStyle;
                            GUI.Box(rect, string.Empty, style);
                            FloatingText = "Right click => detach button";
                            if(e.type == EventType.MouseUp && e.button == 1) {
                                RemoveButton(button);
                                e.Use();
                            }
                        } else {
                            FloatingText = "move cursor to button to move UnifiedUI panel.";
                            return;
                        }
                    } else {
                        var style = Input.GetMouseButton(1) ? OrangeRectStyle : GreenRectStyle;
                        GUI.Box(rect, string.Empty, style);
                        FloatingText = "Right click => Move button to UnifiedUI panel";
                        if (e.type == EventType.MouseUp && e.button == 1) {
                            AddButton(button);
                            e.Use();
                        }
                    }
                }
            } catch (Exception ex) { ex.Log(); }
        }

        public void AddButton(UIButton button) {
            try {
                Log.Called();
                Assertion.NotNull(button, "button");
                Buttons[button] = new Metadata {
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
                var data = Buttons[button];
                Assertion.NotNull(data, "data");
                MainPanel.Instance.MultiRowPanel.RemoveButton(button);
                button.eventSizeChanged -= ButtonSizeChanged;
                if (data.OriginalParent == null) {
                    UIView.GetAView().AttachUIComponent(button.gameObject);
                } else {
                    data.OriginalParent.AttachUIComponent(button.gameObject);
                }
                Buttons.Remove(button);
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
