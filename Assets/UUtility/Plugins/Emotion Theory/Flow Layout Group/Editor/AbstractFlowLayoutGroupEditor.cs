using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
	[CustomEditor(typeof(AbstractFlowLayoutGroup), true)]
	[CanEditMultipleObjects]
	/// <summary>
	/// Custom Editor for the HorizontalOrVerticalLayoutGroupEditor Component.
	/// Extend this class to write a custom editor for a component derived from HorizontalOrVerticalLayoutGroupEditor.
	/// </summary>
	public class AbstractFlowLayoutGroupEditor : Editor
	{
		SerializedProperty m_Padding;
		SerializedProperty m_Spacing;
		SerializedProperty m_LineSpacing;
		SerializedProperty m_ChildAlignment;
		SerializedProperty m_ChildControlWidth;
		SerializedProperty m_ChildControlHeight;
		SerializedProperty m_ChildScaleWidth;
		SerializedProperty m_ChildScaleHeight;
		SerializedProperty m_ReverseArrangement;

		public AbstractFlowLayoutGroup Target => target as AbstractFlowLayoutGroup;

		protected virtual void OnEnable()
		{
			m_Padding = serializedObject.FindProperty("m_Padding");
			m_Spacing = serializedObject.FindProperty("m_Spacing");
			m_LineSpacing = serializedObject.FindProperty("m_LineSpacing");
			m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");
			m_ChildControlWidth = serializedObject.FindProperty("m_ChildControlWidth");
			m_ChildControlHeight = serializedObject.FindProperty("m_ChildControlHeight");
			m_ChildScaleWidth = serializedObject.FindProperty("m_ChildScaleWidth");
			m_ChildScaleHeight = serializedObject.FindProperty("m_ChildScaleHeight");
			m_ReverseArrangement = serializedObject.FindProperty("m_ReverseArrangement");
		}
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			//EditorGUILayout.LabelField(string.Format("W: {0} | H: {1}", Target.GetWidth, Target.GetHeight));
			EditorGUILayout.PropertyField(m_Padding, true);
			EditorGUILayout.PropertyField(m_Spacing, true);
			EditorGUILayout.PropertyField(m_LineSpacing, true);
			EditorGUILayout.PropertyField(m_ChildAlignment, true);
			EditorGUILayout.PropertyField(m_ReverseArrangement, true);

			Rect rect = EditorGUILayout.GetControlRect();
			rect = EditorGUI.PrefixLabel(rect, -1, EditorGUIUtility.TrTextContent("Control Child Size"));
			rect.width = Mathf.Max(50, (rect.width - 4) / 3);
			EditorGUIUtility.labelWidth = 50;
			ToggleLeft(rect, m_ChildControlWidth, EditorGUIUtility.TrTextContent("Width"));
			rect.x += rect.width + 2;
			ToggleLeft(rect, m_ChildControlHeight, EditorGUIUtility.TrTextContent("Height"));
			EditorGUIUtility.labelWidth = 0;

			rect = EditorGUILayout.GetControlRect();
			rect = EditorGUI.PrefixLabel(rect, -1, EditorGUIUtility.TrTextContent("Use Child Scale"));
			rect.width = Mathf.Max(50, (rect.width - 4) / 3);
			EditorGUIUtility.labelWidth = 50;
			ToggleLeft(rect, m_ChildScaleWidth, EditorGUIUtility.TrTextContent("Width"));
			rect.x += rect.width + 2;
			ToggleLeft(rect, m_ChildScaleHeight, EditorGUIUtility.TrTextContent("Height"));
			EditorGUIUtility.labelWidth = 0;

			serializedObject.ApplyModifiedProperties();
		}

		void ToggleLeft(Rect position, SerializedProperty property, GUIContent label)
		{
			bool toggle = property.boolValue;
			EditorGUI.BeginProperty(position, label, property);
			EditorGUI.BeginChangeCheck();
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			toggle = EditorGUI.ToggleLeft(position, label, toggle);
			EditorGUI.indentLevel = oldIndent;
			if (EditorGUI.EndChangeCheck())
			{
				property.boolValue = property.hasMultipleDifferentValues ? true : !property.boolValue;
			}
			EditorGUI.EndProperty();
		}
	}
}
