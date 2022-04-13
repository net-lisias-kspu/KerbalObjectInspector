/*
	This file is part of Kerbal Object Inspector /L Unleashed
		© 2022 LisiasT
		© 2016 IRnifty

	Kerbal Object Inspector /L is double licensed, as follows:
		* GPL 3.0 : https://www.gnu.org/licenses/gpl-3.0.txt

	Kerbal Object Inspector /L is distributed in the hope that it will
	be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
	of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 2.0
	along with Kerbal Object Inspector /L.
	If not, see <https://www.gnu.org/licenses/>.
*/
using System.Reflection;
using UnityEngine;

using GUI = KSPe.UI.GUI;
using GUILayout = KSPe.UI.GUILayout;

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
            inspectorRect = new Rect(hierarchyRect.x + hierarchyRect.width, hierarchyRect.y, 500f, 1000f);
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

            GUI.DragWindow(new Rect(0f, 0f, 500f, 20f));
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
