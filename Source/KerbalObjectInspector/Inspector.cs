/*
	This file is part of Kerbal Object Inspector /L Unleashed
		© 2022 LisiasT
		© 2016-2022 linuxgurugamer
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
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using GUI = KSPe.UI.GUI;
using GUILayout = KSPe.UI.GUILayout;

namespace KerbalObjectInspector
{
    class Inspector
    {
        private static GUIStyle centeredLabel;
        private static GUIStyle leftAlignButton;
        private static GUIStyle labelNoWrap;

        private Vector2 inspectorScroll;
        private int inspectorID;

        public Rect rect;
        public bool isVisible;

        private Transform currentTransform;
        private Component currentComponent;
        private Hierarchy hierarchy;

        private bool showPrivateMember = false;
        private bool showStaticMember = false;
        private BindingFlags currentFlags = defaultFlags;
        private const BindingFlags defaultFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public Inspector(Hierarchy hierarchy, int id, Rect hierarchyRect)
        {
            if (centeredLabel == null)
            {
                centeredLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter , fontStyle = FontStyle.Bold};
                leftAlignButton = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold};
                labelNoWrap = new GUIStyle(HighLogic.Skin.label) { wordWrap = false };
            }

            this.hierarchy = hierarchy;
            inspectorID = id;
            rect = new Rect(hierarchyRect.x + hierarchyRect.width, hierarchyRect.y, 450f, 800f);
            inspectorScroll = Vector2.zero;
        }

        public void DrawGUI(Transform transform)
        {
            if (transform != null)
            {
                isVisible = true;
                currentTransform = transform;
                rect = GUILayout.Window(inspectorID, rect, InspectorWindow, "GameObject: " + transform.gameObject.name, HighLogic.Skin.window);
            }
            else
            {
                isVisible = false;
                currentComponent = null;
            }
        }

        private void InspectorWindow(int windowID)
        {
            inspectorScroll = GUILayout.BeginScrollView(inspectorScroll, HighLogic.Skin.scrollView, GUILayout.ExpandWidth(true));

            GUILayout.BeginHorizontal();

            GUILayout.Label("Active : ", HighLogic.Skin.label, GUILayout.Width(50f));
            if (GUILayout.Button(currentTransform.gameObject.activeSelf.ToString(), GUILayout.Width(50f)))
            {
                currentTransform.gameObject.SetActive(!currentTransform.gameObject.activeSelf);
            }
            GUILayout.Space(10f);
            GUILayout.Label("Layer : " + White(currentTransform.gameObject.layer.ToString() + " (" + LayerMask.LayerToName(currentTransform.gameObject.layer) + ")"), HighLogic.Skin.label, GUILayout.Width(150f));
            GUILayout.Label("Tag : " + White(currentTransform.gameObject.tag), HighLogic.Skin.label);

            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(HighLogic.Skin.box);

            GUILayout.BeginHorizontal();

            GUILayout.Label("World pos", HighLogic.Skin.label, GUILayout.Width(65f));
            GUILayout.Label("X: " + White(currentTransform.position.x.ToString("#,###,##0.0###")), HighLogic.Skin.label, GUILayout.Width(110f));
            GUILayout.Label("Y: " + White(currentTransform.position.y.ToString("#,###,##0.0###")), HighLogic.Skin.label, GUILayout.Width(110f));
            GUILayout.Label("Z: " + White(currentTransform.position.z.ToString("#,###,##0.0###")), HighLogic.Skin.label, GUILayout.Width(110f));

            GUILayout.EndHorizontal();

            
            GUILayout.BeginHorizontal();

            GUILayout.Label("Local pos", HighLogic.Skin.label, GUILayout.Width(65f));
            GUILayout.Label("X: " + White(currentTransform.localPosition.x.ToString("#,###,##0.0###")), HighLogic.Skin.label, GUILayout.Width(110f));
            GUILayout.Label("Y: " + White(currentTransform.localPosition.y.ToString("#,###,##0.0###")), HighLogic.Skin.label, GUILayout.Width(110f));
            GUILayout.Label("Z: " + White(currentTransform.localPosition.z.ToString("#,###,##0.0###")), HighLogic.Skin.label, GUILayout.Width(110f));

            GUILayout.EndHorizontal();

            
            GUILayout.BeginHorizontal();

            GUILayout.Label("Rotation", HighLogic.Skin.label, GUILayout.Width(65f));
            GUILayout.Label("X: " + White(currentTransform.rotation.eulerAngles.x.ToString("#,###,##0.0###")), HighLogic.Skin.label, GUILayout.Width(110f));
            GUILayout.Label("Y: " + White(currentTransform.rotation.eulerAngles.y.ToString("#,###,##0.0###")), HighLogic.Skin.label, GUILayout.Width(110f));
            GUILayout.Label("Z: " + White(currentTransform.rotation.eulerAngles.z.ToString("#,###,##0.0###")), HighLogic.Skin.label, GUILayout.Width(110f));

            GUILayout.EndHorizontal();

            
            GUILayout.BeginHorizontal();

            GUILayout.Label("Scale", HighLogic.Skin.label, GUILayout.Width(65f));
            GUILayout.Label("X: " + White(currentTransform.localScale.x.ToString("#,###,##0.0###")), HighLogic.Skin.label, GUILayout.Width(110f));
            GUILayout.Label("Y: " + White(currentTransform.localScale.y.ToString("#,###,##0.0###")), HighLogic.Skin.label, GUILayout.Width(110f));
            GUILayout.Label("Z: " + White(currentTransform.localScale.z.ToString("#,###,##0.0###")), HighLogic.Skin.label, GUILayout.Width(110f));

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.BeginVertical(HighLogic.Skin.box);

            DrawObjectMembers(currentTransform.gameObject, true);

            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();

            GUILayout.Label("<b>Components</b>", GUILayout.Width(100f));
            showPrivateMember = GUILayout.Toggle(showPrivateMember, Orange("Private members"), GUILayout.Width(150f));
            showStaticMember = GUILayout.Toggle(showStaticMember, Static("Static members"), GUILayout.Width(150f));

            currentFlags = defaultFlags;
            if (showPrivateMember) currentFlags |= BindingFlags.NonPublic;
            if (showStaticMember) currentFlags |= BindingFlags.Static;

            GUILayout.EndHorizontal();

            foreach (Component c in currentTransform.GetComponents<Component>())
            {
                DrawComponent(c);
            }

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        private void DrawComponent(Component component)
        {
            if (GUILayout.Button("<size=14>" + component.GetType().ToString() + "</size>", leftAlignButton, GUILayout.ExpandWidth(true)))
            {
                if (component == currentComponent)
                    currentComponent = null;
                else
                    currentComponent = component;
            }

            if (currentComponent != null && component == currentComponent)
            {
                GUILayout.BeginVertical(HighLogic.Skin.box, GUILayout.ExpandWidth(true));
                DrawObjectMembers(component);
                GUILayout.EndVertical();
            }
        }

        private static List<string> gameObjectVisibleMembers = new List<string>()
        {
            nameof(GameObject.layer),
            nameof(GameObject.activeSelf),
            nameof(GameObject.activeInHierarchy),
            nameof(GameObject.isStatic),
            nameof(GameObject.tag),
            nameof(GameObject.name),
            nameof(GameObject.hideFlags)
        };

        private void DrawObjectMembers(object target, bool isGameObject = false)
        {
            FieldInfo[] pubFields = target.GetType().GetFields(currentFlags);
            for (int i = 0; i < pubFields.Length; i++)
            {
                if (!pubFields[i].IsDefined(typeof(ObsoleteAttribute), true))
                {
                    object val = null;
                    try { val = pubFields[i].GetValue(target); }
                    catch { continue; }
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                    string valStr = val != null ? val.ToString() : "null";

                    bool isEditable = Serialization.CanParse(pubFields[i].FieldType);
                    bool isLong = valStr.Length > 500;

                    if (isEditable)
                    {
                        if (GUILayout.Button("Edit", GUILayout.Width(45f)))
                        {
                            hierarchy.editor = new ValueEditor(hierarchy, target, pubFields[i], null);
                        }
                    }
                    else if (isLong)
                    {
                        if (GUILayout.Button("View", GUILayout.Width(45f)))
                        {
                            hierarchy.editor = new ValueEditor(hierarchy, target, pubFields[i], null, true);
                        }
                    }
                    else
                    {
                        GUI.enabled = false;
                        GUILayout.Button("Field", GUILayout.Width(45f));
                        GUI.enabled = true;
                    }

                    if (isLong)
                    {
                        valStr = valStr.Substring(0, 500) + "\n[TRUNCATED]";
                    }

                    DrawValue(pubFields[i].Name, valStr, pubFields[i].IsPublic, pubFields[i].IsStatic);

                    GUILayout.EndHorizontal();
                }
            }

            PropertyInfo[] pubProperties = target.GetType().GetProperties(currentFlags);
            for (int i = 0; i < pubProperties.Length; i++)
            {
                if (isGameObject && !gameObjectVisibleMembers.Contains(pubProperties[i].Name))
                    continue;

                if (!pubProperties[i].IsDefined(typeof(ObsoleteAttribute), true))
                {
                    object val = null;
                    try { val = pubProperties[i].GetValue(target, null); }
                    catch { continue; }

                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                    string valStr = val != null ? val.ToString() : "null";

                    bool isEditable = Serialization.CanParse(pubProperties[i].PropertyType) && pubProperties[i].CanWrite;
                    bool isLong = valStr.Length > 500;

                    if (isEditable)
                    {
                        if (GUILayout.Button("Edit", GUILayout.Width(45f)))
                        {
                            hierarchy.editor = new ValueEditor(hierarchy, target, null, pubProperties[i]);
                        }
                    }
                    else if (isLong)
                    {
                        if (GUILayout.Button("View", GUILayout.Width(45f)))
                        {
                            hierarchy.editor = new ValueEditor(hierarchy, target, null, pubProperties[i], true); ;
                        }
                    }
                    else
                    {
                        GUI.enabled = false;
                        GUILayout.Button("Prop", GUILayout.Width(45f));
                        GUI.enabled = true;
                    }

                    if (isLong)
                    {
                        valStr = valStr.Substring(0, 500) + "\n[TRUNCATED]";
                    }

                    DrawValue(pubProperties[i].Name, valStr, pubProperties[i].GetGetMethod().IsPublic, pubProperties[i].GetGetMethod().IsStatic);

                    GUILayout.EndHorizontal();
                }
            }

            if (pubFields.Length == 0 && pubProperties.Length == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("No public fields or properties", HighLogic.Skin.label);
                GUILayout.EndHorizontal();
            }
        }

        private static void DrawValue(string name, string value, bool isPublic, bool isStatic)
        {
            if (!isPublic)
                name = Orange(name);

            if (isStatic)
                name = Static(name);

            if (value.Contains("\n"))
            {
                GUILayout.Label(name + ":\n" + White(value), labelNoWrap, GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.Label(name + ": " + White(value), labelNoWrap, GUILayout.ExpandWidth(true));
            }
        }

        private static string White(string str) => "<color=white>" + str + "</color>";
        private static string Orange(string str) => "<color=orange>" + str + "</color>";
        private static string Static(string str) => "<i>" + str + "</i>";
    }
}
