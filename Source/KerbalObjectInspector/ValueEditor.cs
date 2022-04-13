/*
	This file is part of Kerbal Object Inspector /L Unleashed
		© 2022 LisiasT
		© 2016-2022 linuxgurugamer
		© 2016 IRnifty

	Kerbal Object Inspector /L is licensed as follows:
		* GPL 3.0 : https://www.gnu.org/licenses/gpl-3.0.txt

	Kerbal Object Inspector /L is distributed in the hope that it will
	be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
	of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 2.0
	along with Kerbal Object Inspector /L.
	If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Reflection;
using UnityEngine;

namespace KerbalObjectInspector
{
    public class ValueEditor
    {
        private enum ValueType { Unknown, Field, Property };

        private int editorID;
        public Rect rect;

        private Hierarchy hierarchy;

        private string[] enumValues;
        private Type currentValueType;
        private PropertyInfo currentProperty;
        private FieldInfo currentField;
        private object currentObject;
        private ValueType valueType;
        private bool isLong;
        private bool canEdit;
        private Vector2 scrollPos;

        private string input;

        public ValueEditor(Hierarchy hierarchy, object target, FieldInfo field = null, PropertyInfo property = null, bool readOnly = false)
        {
            this.hierarchy = hierarchy;
            currentObject = target;
            currentProperty = property;
            currentField = field;
            editorID = hierarchy.GetInstanceID() + 2;
            rect = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 300f, 150f);
            scrollPos = Vector2.zero;

            if (field != null)
            {
                valueType = ValueType.Field;
                currentValueType = field.FieldType;
                canEdit = !readOnly;
            }
            else if (property != null)
            {
                valueType = ValueType.Property;
                currentValueType = property.PropertyType;
                canEdit = !readOnly && property.CanWrite;
            }
            else
            {
                valueType = ValueType.Unknown;
            }

            UpdateInput();

            isLong = input.Length > 30 || input.Contains("\n");

            if (typeof(Enum).IsAssignableFrom(currentValueType))
            {
                enumValues = Enum.GetNames(currentValueType);
            }
        }

        private void UpdateInput()
        {
            if (valueType == ValueType.Field)
            {
                try { input = Serialization.Serialize(currentField.GetValue(currentObject)); }
                catch { input = string.Empty; }
            }
            else if (valueType == ValueType.Property)
            {
                try { input = Serialization.Serialize(currentProperty.GetValue(currentObject, null)); }
                catch { input = string.Empty; }
            }
            else
            {
                input = string.Empty;
            }
        }


        public void DrawGUI()
        {
            rect = GUILayout.Window(editorID, rect, ValueEditorWindow, "Value editor", HighLogic.Skin.window);
        }

        private void ValueEditorWindow(int windowID)
        {
            GUILayout.BeginVertical(HighLogic.Skin.box);

            if (valueType == ValueType.Field)
            {
                GUILayout.Label($"Field name : {currentField.Name}");
                GUILayout.Label($"Type : {currentValueType.Name}");
                GUILayout.Label($"Access : {(currentField.IsPublic ? "public" : "non-public")}");

                DrawCurrentValue();
                DrawEditArea();

            }
            else if (valueType == ValueType.Property)
            {
                GUILayout.Label($"Property name : {currentProperty.Name}");
                GUILayout.Label($"Type : {currentValueType.Name}");
                GUILayout.Label($"Getter access : {(currentProperty.GetGetMethod().IsPublic ? "public" : "non-public")}");

                if (currentProperty.CanWrite)
                {
                    GUILayout.Label($"Setter access : {(currentProperty.GetSetMethod().IsPublic ? "public" : "non-public")}");
                }
                else
                {
                    GUILayout.Label($"Property is read-only");
                }

                DrawCurrentValue();
                DrawEditArea();
            }
            else
            {
                GUILayout.Label($"Error - unknown member type");
            }

            if (enumValues != null)
            {
                GUILayout.BeginVertical(HighLogic.Skin.box);
                GUILayout.Label($"Enum values :");
                foreach (string enumValue in enumValues)
                {
                    if (GUILayout.Button(enumValue, GUILayout.ExpandWidth(true)))
                    {
                        input = enumValue;
                    }
                }
                GUILayout.EndVertical();
            }


            if (GUILayout.Button("Close"))
            {
                hierarchy.editor = null;
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void DrawCurrentValue()
        {
            if (!isLong)
            {
                object val = null;
                if (valueType == ValueType.Field)
                {
                    try { val = currentField.GetValue(currentObject); }
                    catch { val = null; }
                }
                else if (valueType == ValueType.Property)
                {
                    try { val = currentProperty.GetValue(currentObject, null); }
                    catch { val = null; }
                }
                
                string valStr = val != null ? val.ToString() : "null";

                if (valStr.Contains("\n") || valStr.Length > 30)
                {
                    GUILayout.Label($"Current value :\n{valStr}");
                }
                else
                {
                    GUILayout.Label($"Current value :{valStr}");
                }
            }
        }


        private void DrawEditArea()
        {
            if (!isLong)
            {
                GUILayout.BeginHorizontal(HighLogic.Skin.box);
                input = GUILayout.TextField(input, GUILayout.Width(260f));

                if (canEdit)
                {
                    if (GUILayout.Button("Set") && !string.IsNullOrEmpty(input))
                    {
                        if (valueType == ValueType.Field) SetField(); 
                        else SetProperty();
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginVertical(HighLogic.Skin.box);
                scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(100f), GUILayout.ExpandWidth(true));
                input = GUILayout.TextArea(input, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                GUILayout.EndScrollView();
                if (canEdit)
                {
                    if (GUILayout.Button("Set value", GUILayout.Width(270f)) && !string.IsNullOrEmpty(input))
                    {
                        if (valueType == ValueType.Field) SetField();
                        else SetProperty();
                        UpdateInput();
                    }
                }
                GUILayout.EndVertical();
            }
        }

        private void SetField()
        {
            try
            {
                if (Serialization.TryDeserialize(input, currentValueType, out object newValue))
                {
                    currentField.SetValue(currentObject, newValue);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void SetProperty()
        {
            try
            {
                if (Serialization.TryDeserialize(input, currentValueType, out object newValue))
                {
                    currentProperty.SetValue(currentObject, newValue, null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}