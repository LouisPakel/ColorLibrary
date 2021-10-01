using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace ArchNet.Library.Colors
{
    [CustomEditor(typeof(ColorLibrary))]
    public class ColorLibraryEditor : UnityEditor.Editor
    {
        int _listCount;
        int _lastDefaultValue;
        int _colorIndexToDelete = -1;
        string[] _enumValues;
        bool _forceUpdate = false;

        EditorCoroutine _saveCoroutine = null;

        SerializedProperty _colorList = null;
        SerializedProperty _enumPath = null;
        SerializedProperty _keyType = null;
        SerializedProperty _expandedSettings = null;
        SerializedProperty _forceDefaultValue = null;
        SerializedProperty _defaultValue = null;

        ColorLibrary manager;

        GUIStyle _warningInfos = null;

        private void OnEnable()
        {
            _warningInfos = new GUIStyle();
            _warningInfos.normal.textColor = Color.red;
            _warningInfos.fontStyle = FontStyle.Bold;

            manager = target as ColorLibrary;
        }

        private void OnDisable()
        {
            manager = null;
            _warningInfos = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _colorList = serializedObject.FindProperty("_colorList");
            _enumPath = serializedObject.FindProperty("_enumPath");
            _keyType = serializedObject.FindProperty("_keyType");
            _expandedSettings = serializedObject.FindProperty("_expandedSettings");
            _forceDefaultValue = serializedObject.FindProperty("_forceDefaultValue");
            _defaultValue = serializedObject.FindProperty("_defaultValue");

            // Key Type
            EditorGUILayout.LabelField("Key Type", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_keyType, new GUIContent(""));

            EditorGUILayout.Space(10);

            if (manager.GetKeyType() > 0 && false == manager.isKeyTypeUpToDate(_keyType.intValue))
            {
                if (EditorUtility.DisplayDialog("Warning", "This will clear set properties?\nDo you want to continue?", "Yes", "No"))
                {
                    this.ResetEnumPath();
                    this.ResetColorList();
                }
                else
                {
                    _keyType.intValue = manager.GetKeyType();
                }
            }
            _keyType.serializedObject.ApplyModifiedProperties();

            // Enum key type
            if (_keyType.intValue == 1)
            {
                EditorGUILayout.LabelField("Full Namespace Enum Path", EditorStyles.boldLabel);

                if (_enumPath.serializedObject.hasModifiedProperties)
                {
                    _enumPath.serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.PropertyField(_enumPath, new GUIContent(""));

                EditorGUILayout.Space(10);
            }

            // Settings
            _expandedSettings.isExpanded = EditorGUILayout.Foldout(_expandedSettings.isExpanded, "Settings");
            if (_expandedSettings.isExpanded)
            {
                // Force Default Value
                _forceDefaultValue.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Force Default Value"), _forceDefaultValue.boolValue);

                _lastDefaultValue = _defaultValue.intValue;
                if (_forceDefaultValue.boolValue)
                {
                    if (_keyType.intValue == 1)
                    {
                        if (_enumValues != null && _enumValues.Length > 0)
                        {
                            _defaultValue.intValue = EditorGUILayout.Popup(_defaultValue.intValue, _enumValues);
                            if (_defaultValue.serializedObject.hasModifiedProperties)
                            {
                                this.SetColorListDefaultValue();
                                _forceUpdate = true;
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No enum values found", _warningInfos);
                        }
                    }
                    if (_keyType.intValue == 2)
                    {
                        EditorGUILayout.PropertyField(_defaultValue, new GUIContent(""));
                        if (_defaultValue.serializedObject.hasModifiedProperties)
                        {
                            this.SetColorListDefaultValue();
                            _forceUpdate = true;
                        }
                    }
                }
                else if (_forceDefaultValue.serializedObject.hasModifiedProperties)
                {
                    _defaultValue.intValue = 0;
                    this.SetColorListDefaultValue();
                    _forceUpdate = true;
                }
            }

            EditorGUILayout.Space(10);

            // Add / Remove Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add a color"))
            {
                if (true == IsConditionsOK())
                {
                    manager.AddAColor();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Color Library
            EditorGUILayout.LabelField("Color Library", EditorStyles.boldLabel);

            if (true == IsConditionsOK())
            {
                this.HandleDatasDisplay();
                this.SaveColorListIfNecessary();
            }
            else
            {
                EditorGUILayout.LabelField("Missing parameters!", _warningInfos);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SetColorListDefaultValue()
        {
            SerializedProperty colorData;
            SerializedProperty Key;

            _listCount = _colorList.arraySize;
            for (int i = 0; i < _listCount; i++)
            {
                colorData = _colorList.GetArrayElementAtIndex(i);
                Key = colorData.FindPropertyRelative("_colorKey");

                if (Key.intValue == _lastDefaultValue)
                {
                    Key.intValue = _defaultValue.intValue;
                }
            }
        }

        private void HandleDatasDisplay()
        {
            SerializedProperty colorData;
            SerializedProperty colorValue;
            SerializedProperty colorKey;

            _listCount = _colorList.arraySize;
            for (int i = 0; i < _listCount; i++)
            {
                colorData = _colorList.GetArrayElementAtIndex(i);

                colorValue = colorData.FindPropertyRelative("_colorValue");
                colorKey = colorData.FindPropertyRelative("_colorKey");

                colorData.isExpanded = EditorGUILayout.Foldout(colorData.isExpanded, new GUIContent("Color " + i));
                if (colorData.isExpanded)
                {
                    EditorGUILayout.BeginHorizontal();
                    colorValue.colorValue = EditorGUILayout.ColorField(colorValue.colorValue);
                    switch (_keyType.intValue)
                    {
                        // NONE
                        case 0:
                            break;

                        // ENUM
                        case 1:
                            colorKey.intValue = EditorGUILayout.Popup(colorKey.intValue, _enumValues, GUILayout.MaxWidth(200));
                            break;

                        // INT
                        case 2:
                            EditorGUILayout.LabelField("=>", GUILayout.MaxWidth(20));
                            EditorGUILayout.PropertyField(colorKey, new GUIContent(""), GUILayout.MaxWidth(200));
                            break;
                    }
                    if (GUILayout.Button("Delete"))
                    {
                        if (EditorUtility.DisplayDialog("Warning", "Are you sure to delete this color?", "Yes", "No"))
                        {
                            colorData.isExpanded = false;
                            _colorList.DeleteArrayElementAtIndex(i);
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(5);
                }
            }
        }

        private void ResetEnumPath()
        {
            _enumPath.stringValue = "";
            _enumPath.serializedObject.ApplyModifiedProperties();
        }

        private void ResetColorList()
        {
            SerializedProperty colorData;
            _listCount = _colorList.arraySize;
            for (int i = 0; i < _listCount; i++)
            {
                colorData = _colorList.GetArrayElementAtIndex(i);
                colorData.isExpanded = false;
            }
            _colorList.ClearArray();
            _enumPath.serializedObject.ApplyModifiedProperties();
        }

        private bool IsConditionsOK()
        {
            if (_keyType.intValue == 0)
            {
                return false;
            }
            else if (_keyType.intValue == 1)
            {
                if (true == string.IsNullOrEmpty(_enumPath.stringValue))
                {
                    return false;
                }

                _enumValues = manager.GetEnumValues(_enumPath.stringValue);
                if (_enumValues == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void SaveColorListIfNecessary()
        {
            if ((_colorList.serializedObject.hasModifiedProperties || this._forceUpdate) && _colorList.arraySize > 0)
            {
                if (_saveCoroutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(_saveCoroutine);
                }
                _saveCoroutine = EditorCoroutineUtility.StartCoroutine(SaveColorList(), _colorList.serializedObject.targetObject);

                this._forceUpdate = false;
            }
            _colorList.serializedObject.ApplyModifiedProperties();
        }

        private IEnumerator SaveColorList()
        {
            yield return new EditorWaitForSeconds(0.5f);
            manager.SaveDictionnary();
            yield return null;
        }
    }
}
