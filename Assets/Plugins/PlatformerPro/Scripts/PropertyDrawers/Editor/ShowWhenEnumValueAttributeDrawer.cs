using UnityEngine;
using UnityEditor;

namespace PlatformerPro
{
	/// <summary>
	/// Draws enums as a mask field.
	/// </summary>
	[CustomPropertyDrawer(typeof(ShowWhenEnumValueAttribute))]
	public class ShowWhenEnumValueAttributeDrawer : PropertyDrawer
	{
		override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			ShowWhenEnumValueAttribute showWhenEnum = attribute as ShowWhenEnumValueAttribute;
			SerializedProperty other = property.serializedObject.FindProperty (showWhenEnum.otherProperty);
			// Find properties of complex classes (TODO this could get it wrong should probably use a regex)
			if (other == null && property.propertyPath.Contains ("."))
			{
				other = property.serializedObject.FindProperty (property.propertyPath.Replace (property.name, showWhenEnum.otherProperty));
			}
			if (IsMatched(showWhenEnum, other))
			{
				EditorGUI.PropertyField (position, property, label, true);
			}
		}

		override public float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			ShowWhenEnumValueAttribute showWhenEnum = attribute as ShowWhenEnumValueAttribute;
			SerializedProperty other = property.serializedObject.FindProperty (showWhenEnum.otherProperty);
			// Find properties of complex classes (TODO this could get it wrong should probably use a regex)
			if (other == null && property.propertyPath.Contains ("."))
			{
				other = property.serializedObject.FindProperty (property.propertyPath.Replace (property.name, showWhenEnum.otherProperty));
			}
			if (IsMatched(showWhenEnum, other))
			{
				return EditorGUI.GetPropertyHeight(property);
			}
			return 0;
		}

		bool IsMatched(ShowWhenEnumValueAttribute showWhenEnum, SerializedProperty other)
		{
			bool matched = false;
			if (other == null)
			{
				Debug.LogWarning("Couldn't find matching property for ShowWhenEnumValueAttribute");
				matched = true;
			} 
			else
			{
				for (int i = 0; i < showWhenEnum.enumValues.Length; i++)
				{
					if (showWhenEnum.enumValues[i] == other.enumValueIndex) matched = true;
				}
				if (showWhenEnum.showWhenNotMatched) matched = !matched;
			}
			return matched;
		}
	}

}