using System.Reflection;
using UnityEngine;

namespace KerbalObjectInspector
{
    class Inspector
    {
        private Rect inspectorRect;
        private Vector2 inspectorScroll;
        private int inspectorID;

        private Transform currentTransform;

        public Inspector(int id, Rect hierarchyRect)
        {
            inspectorID = id;
            inspectorRect = new Rect(hierarchyRect.x + hierarchyRect.width, hierarchyRect.y, 500f, 800f);
            inspectorScroll = Vector2.zero;
        }

        public void DrawGUI(Transform transform)
        {
            if (transform != null)
            {
                currentTransform = transform;
                inspectorRect = GUILayout.Window(inspectorID, inspectorRect, InspectorWindow, "Inspector: " + transform.gameObject.name, HighLogic.Skin.window);
            }
        }

        private void InspectorWindow(int windowID)
        {
            inspectorScroll = GUILayout.BeginScrollView(inspectorScroll, HighLogic.Skin.scrollView);

            GUILayout.Label("<size=20>Layer: " + currentTransform.gameObject.layer.ToString() + "</size>", HighLogic.Skin.label);

            GUILayout.Label("<size=20>Transform</size>", HighLogic.Skin.label);

            GUILayout.BeginVertical(HighLogic.Skin.box);

            GUILayout.Label("Global Position", HighLogic.Skin.label);
            GUILayout.BeginHorizontal();

            GUILayout.Label("X: " + currentTransform.position.x.ToString("#,###,##0.0###"), HighLogic.Skin.label, GUILayout.Width(150f));
            GUILayout.Label("Y: " + currentTransform.position.y.ToString("#,###,##0.0###"), HighLogic.Skin.label, GUILayout.Width(150f));
            GUILayout.Label("Z: " + currentTransform.position.z.ToString("#,###,##0.0###"), HighLogic.Skin.label, GUILayout.Width(150f));

            GUILayout.EndHorizontal();

            GUILayout.Label("Local Position", HighLogic.Skin.label);
            GUILayout.BeginHorizontal();

            GUILayout.Label("X: " + currentTransform.localPosition.x.ToString("#,###,##0.0###"), HighLogic.Skin.label, GUILayout.Width(150f));
            GUILayout.Label("Y: " + currentTransform.localPosition.y.ToString("#,###,##0.0###"), HighLogic.Skin.label, GUILayout.Width(150f));
            GUILayout.Label("Z: " + currentTransform.localPosition.z.ToString("#,###,##0.0###"), HighLogic.Skin.label, GUILayout.Width(150f));

            GUILayout.EndHorizontal();

            GUILayout.Label("Rotation", HighLogic.Skin.label);
            GUILayout.BeginHorizontal();

            GUILayout.Label("X: " + currentTransform.rotation.eulerAngles.x.ToString("#,###,##0.0###"), HighLogic.Skin.label, GUILayout.Width(150f));
            GUILayout.Label("Y: " + currentTransform.rotation.eulerAngles.y.ToString("#,###,##0.0###"), HighLogic.Skin.label, GUILayout.Width(150f));
            GUILayout.Label("Z: " + currentTransform.rotation.eulerAngles.z.ToString("#,###,##0.0###"), HighLogic.Skin.label, GUILayout.Width(150f));

            GUILayout.EndHorizontal();

            GUILayout.Label("Scale", HighLogic.Skin.label);
            GUILayout.BeginHorizontal();

            GUILayout.Label("X: " + currentTransform.localScale.x.ToString("#,###,##0.0###"), HighLogic.Skin.label, GUILayout.Width(150f));
            GUILayout.Label("Y: " + currentTransform.localScale.y.ToString("#,###,##0.0###"), HighLogic.Skin.label, GUILayout.Width(150f));
            GUILayout.Label("Z: " + currentTransform.localScale.z.ToString("#,###,##0.0###"), HighLogic.Skin.label, GUILayout.Width(150f));

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            foreach (Component c in currentTransform.GetComponents<Component>())
            {
                if (c.GetType().ToString() != typeof(Transform).ToString())
                {
                    DrawComponent(c);
                }
            }

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        private void DrawComponent(Component component)
        {
            GUILayout.Label("<size=20>" + component.GetType().ToString() + "</size>", HighLogic.Skin.label);

            GUILayout.BeginVertical(HighLogic.Skin.box);

            DrawComponentFields(component);

            GUILayout.EndVertical();
        }

        private void DrawComponentFields(Component component)
        {
            FieldInfo[] pubFields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < pubFields.Length; i++)
            {
                object val = pubFields[i].GetValue(component);

                GUILayout.Label(pubFields[i].Name + ": " + (val != null ? val.ToString() : "null"), HighLogic.Skin.label);

                if (i < pubFields.Length - 1)
                {
                    GUILayout.Label(" ");
                }
            }

            if (pubFields.Length == 0)
            {
                GUILayout.Label("No public fields", HighLogic.Skin.label);
            }
        }
    }
}
