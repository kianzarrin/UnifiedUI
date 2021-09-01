namespace UnifiedUI.GUI {
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using System.Reflection;
    using UnityEngine;


    public class UIDragHandleRight : UIComponent {
        protected UIComponent m_Target;
        private Vector3 m_LastPosition;

        public UIComponent target {
            get {
                return m_Target;
            }
            set {
                m_Target = value;
            }
        }

        public override void Start() {
            base.Start();
            tooltip = "Hold Right-click to move";
            if (m_Target == null) {
                m_Target = base.parent;
            }
            if (base.size.magnitude <= 1.401298E-45f) {
                if (base.parent != null) {
                    base.size = new Vector2(m_Target.width, 30f);
                    base.anchor = (UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right);
                    base.relativePosition = Vector2.zero;
                    return;
                }
                base.size = new Vector2(200f, 25f);
            }
        }

        protected override void OnMouseDown(UIMouseEventParameter p) {
            if (target != null) {
                target.BringToFront();
            } else {
                base.GetRootContainer().BringToFront();
            }
            p.Use();
            Plane plane = new Plane(m_Target.transform.TransformDirection(Vector3.back), m_Target.transform.position);
            Ray ray = p.ray;
            float d = 0f;
            plane.Raycast(ray, out d);
            m_LastPosition = ray.origin + ray.direction * d;
            base.OnMouseDown(p);
        }

        protected override void OnMouseMove(UIMouseEventParameter p) {
            p.Use();
            if (p.buttons.IsFlagSet(UIMouseButton.Right)) {
                Ray ray = p.ray;
                float d = 0f;
                Vector3 inNormal = base.GetUIView().uiCamera.transform.TransformDirection(Vector3.back);
                Plane plane = new Plane(inNormal, m_LastPosition);
                plane.Raycast(ray, out d);
                Vector3 vector = (ray.origin + ray.direction * d).Quantize(m_Target.PixelsToUnits());
                Vector3 b = vector - m_LastPosition;
                Vector3[] corners = base.GetUIView().GetCorners();
                Vector3 position = (m_Target.transform.position + b).Quantize(m_Target.PixelsToUnits());
                Vector3 a = m_Target.pivot.TransformToUpperLeft(m_Target.size, m_Target.arbitraryPivotOffset);
                Vector3 a2 = a + new Vector3(m_Target.size.x, -m_Target.size.y);
                a *= m_Target.PixelsToUnits();
                a2 *= m_Target.PixelsToUnits();
                /*if (m_ConstrainToScreen)*/
                {
                    if (position.x + a.x < corners[0].x) {
                        position.x = corners[0].x - a.x;
                    }
                    if (position.x + a2.x > corners[1].x) {
                        position.x = corners[1].x - a2.x;
                    }
                    if (position.y + a.y > corners[0].y) {
                        position.y = corners[0].y - a.y;
                    }
                    if (position.y + a2.y < corners[2].y) {
                        position.y = corners[2].y - a2.y;
                    }
                }
                m_Target.transform.position = position;
                m_LastPosition = vector;
            }
            base.OnMouseMove(p);
        }

        protected override void OnMouseUp(UIMouseEventParameter p) {
            base.OnMouseUp(p);
            m_Target.MakePixelPerfect();
        }
    }

    public static class CallPixelsToUnitsExtension {
        delegate float DPixelsToUnits(UIComponent c);
        static MethodInfo mPixelsToUnits = typeof(UIComponent).GetMethod(nameof(PixelsToUnits), throwOnError: true);
        static DPixelsToUnits CallPixelsToUnits = (DPixelsToUnits)Delegate.CreateDelegate(typeof(DPixelsToUnits), mPixelsToUnits);

        public static float PixelsToUnits(this UIComponent c) => CallPixelsToUnits(c);
    }
}
