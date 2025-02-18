using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Lattice.Editor
{
	[CustomPropertyDrawer(typeof(ShowIfAttribute))] 
	internal class ShowIfDrawer : PropertyDrawer
	{
		private ShowIfAttribute Attribute => attribute as ShowIfAttribute;

		private bool ShouldDraw(SerializedProperty property)
		{
			object[] values = Attribute.Values;
			SerializedProperty target = FindSiblingProperty(property, $"{Attribute.Name}");

			if (target == null)
				return true;

			if (target.propertyType == SerializedPropertyType.Enum)
			{
				int value = target.enumValueIndex;
				return (values.Length > 0)
					? values.Cast<int>().Contains(value)
					: value > 0;
			}

			return true;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (ShouldDraw(property)) return base.GetPropertyHeight(property, label);
			else return -1f * EditorGUIUtility.standardVerticalSpacing;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (ShouldDraw(property)) EditorGUI.PropertyField(position, property, label);
		}

		private static SerializedProperty FindSiblingProperty(SerializedProperty property, string sibling)
		{
			string[] path = property.propertyPath.Split('.');
			path[^1] = sibling;

			string parent = path[0];
			for (int i = 1; i < path.Length; i++)
			{
				parent += $".{path[i]}";
			}

			return property.serializedObject.FindProperty(parent);
		}
	}
}
