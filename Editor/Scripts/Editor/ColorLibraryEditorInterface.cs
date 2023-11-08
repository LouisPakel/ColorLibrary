using UnityEditor;
using UnityEngine;

namespace Library.Editor
{
    [CustomEditor(typeof(ColorLibrary))]
    public class ColorLibraryEditorInterface : LibraryEditorInterface<ColorLibrary, Color>
    {
        protected override bool AreDifferent(Color a, Color b) => a != b;

        protected override Color DisplayGuiAndSelect(string label, Color currentValue) =>
            EditorGUILayout.ColorField(label, currentValue);
    }
}
