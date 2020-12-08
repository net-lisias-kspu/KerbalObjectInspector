using System.Reflection;
using UnityEngine;

using ClickThroughFix;
using System;
using System.Collections.Generic;

namespace KerbalObjectInspector
{
    public class ValueEditor
    {
        private Rect editorRect;
        private int editorID;

        private Hierarchy hierarchy;

        private string[] enumValues;
        private Type currentValueType;
        private PropertyInfo currentProperty;
        private FieldInfo currentField;
        private Component currentComponent;

        private string input;

        public ValueEditor(Hierarchy hierarchy, int id, Component component, FieldInfo field = null, PropertyInfo property = null)
        {
            this.hierarchy = hierarchy;
            currentComponent = component;
            currentProperty = property;
            currentField = field;
            editorID = id;
            editorRect = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 300f, 150f);

            input = string.Empty;
            if (field != null)
            {
                currentValueType = field.FieldType;
                try { input = Serialization.Serialize(field.GetValue(currentComponent)); }
                catch { input = string.Empty; }
            }

            if (property != null)
            {
                currentValueType = property.PropertyType;
                try { input = Serialization.Serialize(property.GetValue(currentComponent)); }
                catch { input = string.Empty; }
            }


            if (typeof(Enum).IsAssignableFrom(currentValueType))
            {
                enumValues = Enum.GetNames(currentValueType);
            }
        }


        public void DrawGUI()
        {
            editorRect = ClickThruBlocker.GUILayoutWindow(editorID, editorRect, ValueEditorWindow, "Value editor", HighLogic.Skin.window);
        }

        private void ValueEditorWindow(int windowID)
        {
            GUILayout.BeginVertical(HighLogic.Skin.box);

            if (currentField != null)
            {
                object val = null;
                try { val = currentField.GetValue(currentComponent); }
                catch { val = null; }

                GUILayout.Label($"Field name : {currentField.Name}");
                GUILayout.Label($"Type : {currentValueType.Name}");
                GUILayout.Label($"Access : {(currentField.IsPublic ? "public" : "non-public")}");
                GUILayout.Label($"Current value : {(val != null ? val.ToString() : "null")}");
                GUILayout.BeginHorizontal(HighLogic.Skin.box);
                input = GUILayout.TextField(input, 100, GUILayout.Width(240f));

                if (GUILayout.Button("Set value") && !string.IsNullOrEmpty(input))
                {
                    try
                    {
                        if (Serialization.TryDeserialize(input, currentValueType, out object newValue))
                        {
                            currentField.SetValue(currentComponent, newValue);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
                GUILayout.EndHorizontal();
            }
            if (currentProperty != null)
            {
                object val = null;
                try { val = currentProperty.GetValue(currentComponent); }
                catch { val = null; }

                GUILayout.Label($"Property name : {currentProperty.Name}");
                GUILayout.Label($"Type : {currentValueType.Name}");
                GUILayout.Label($"Getter access : {(currentProperty.GetMethod.IsPublic ? "public" : "non-public")}");

                if (currentProperty.CanWrite)
                {
                    GUILayout.Label($"Setter access : {(currentProperty.SetMethod.IsPublic ? "public" : "non-public")}");
                }
                else
                {
                    GUILayout.Label($"Property is read-only");
                }

                GUILayout.Label($"Current value : {(val != null ? val.ToString() : "null")}");

                if (currentProperty.CanWrite)
                {
                    GUILayout.BeginHorizontal(HighLogic.Skin.box);
                    input = GUILayout.TextField(input, 100, GUILayout.Width(200f));

                    if (GUILayout.Button("Set") && !string.IsNullOrEmpty(input))
                    {
                        try
                        {
                            if (Serialization.TryDeserialize(input, currentValueType, out object newValue))
                            {
                                currentProperty.SetValue(currentComponent, newValue);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
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
    }
}