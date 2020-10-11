using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System.Collections.Generic;
using UnityEngine;

namespace KianCommons.UI.Helpers {
    // usage: sub to eventCheckChanged.
    // set Title
    public class UICheckboxDropDownExt : UICheckboxDropDown {
        static Vector2 SIZE = new Vector2(120, 30);

        public string Text {
            get => (triggerButton as UIButton).text;
            set => (triggerButton as UIButton).text = value;
        }

        public List<int> selectedIndeces {
            get {
                List<int> ret = new List<int>(items.Length);
                for (int i = 0; i < items.Length; ++i) {
                    if (GetChecked(i))
                        ret.Add(i);
                }
                return ret;
            }
        }

        public override void Awake() {
            base.Awake();
            // created before start so that user can set text.
            this.triggerButton = AddUIComponent<UIButton>();
            Text = "None";
        }
        public override void Start() {
            base.Start();
            atlas = TextureUtil.InMapEditor;
            size = SIZE;
            listBackground = "GenericPanelLight";
            itemHeight = 10;
            itemHover = "ListItemHover";
            itemHighlight = "ListItemHighlight";
            normalBgSprite = "ButtonMenu";
            disabledBgSprite = "ButtonMenuDisabled";
            hoveredBgSprite = "ButtonMenuHovered";
            focusedBgSprite = "ButtonMenu";
            listWidth = 500;
            listHeight = 1000;
            foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            popupColor = new Color32(45, 52, 61, 255);
            popupTextColor = new Color32(170, 170, 170, 255);
            zOrder = 1;
            textScale = 0.65f;
            verticalAlignment = UIVerticalAlignment.Middle;
            horizontalAlignment = UIHorizontalAlignment.Left;
            textFieldPadding = new RectOffset(8, 0, 0, 0);
            itemPadding = new RectOffset(3, 3, 3, 3);
            listPadding = new RectOffset(5, 5, 5, 5);
            uncheckedSprite = "check-unchecked";
            checkedSprite = "check-checked";

            UIButton button = triggerButton as UIButton;
            //button.atlas = // TODO uncomment this to change button atlas
            button.textPadding = new RectOffset(14, 0, -7, 0);
            button.size = this.size;
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenuFocused";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.zOrder = 0;
            button.textScale = 0.8f;
            button.relativePosition = new Vector3(0, 0);
        } // end Start()


        public static UICheckboxDropDownExt Add(UIPanel panel) {
            // work around dropdown going under top panel.
            UIPanel containing_panel = panel.AddUIComponent<UIPanel>();
            containing_panel.name = "ContainerOf_UICheckboxDropDownExt";
            containing_panel.size = SIZE;
            return containing_panel.AddUIComponent<UICheckboxDropDownExt>();
            // TODO set width to width of triger button
        }
    } // end class
} // end namespace
