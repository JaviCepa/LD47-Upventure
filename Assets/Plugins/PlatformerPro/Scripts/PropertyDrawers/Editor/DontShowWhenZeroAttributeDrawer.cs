using UnityEngine;
using UnityEditor;

namespace PlatformerPro
{
	/// <summary>
	/// Draws enums as a mask field.
	/// </summary>
	[CustomPropertyDrawer(typeof(DontShowWhenZeroAttribute))]
	public class DontShowWhenZeroAttributeDrawer : PropertyDrawer
	{
		override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			DontShowWhenZeroAttribute dontShowWhen = attribute as DontShowWhenZeroAttribute;
			SerializedProperty other = property.serializedObject.FindProperty (dontShowWhen.otherProperty);
			// Find properties of complex classes (TODO this could get it wrong should probably use a regex)
			if (other == null && property.propertyPath.Contains ("."))
			{
				other = property.serializedObject.FindProperty (property.propertyPath.Replace (property.name, dontShowWhen.otherProperty));
			}
			if (other == null || (other.type == "int" && other.intValue == 0 && dontShowWhen.showWhenZero) || (other.type == "int" &&other.intValue != 0 && !dontShowWhen.showWhenZero)
			    || (other.type == "float" && other.floatValue == 0 && dontShowWhen.showWhenZero)  || (other.type == "float" && other.floatValue > 0 && !dontShowWhen.showWhenZero) )
			{
				EditorGUI.PropertyField (position, property, label, true);
			}
		}

		override public float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			DontShowWhenZeroAttribute dontShowWhen = attribute as DontShowWhenZeroAttribute;
			SerializedProperty other = property.serializedObject.FindProperty (dontShowWhen.otherProperty);
			// Find properties of complex classes (TODO this could get it wrong should probably use a regex)
			if (other == null && property.propertyPath.Contains ("."))
			{
				other = property.serializedObject.FindProperty (property.propertyPath.Replace (property.name, dontShowWhen.otherProperty));
			}
			if (other == null || (other.type == "int" && other.intValue == 0 && dontShowWhen.showWhenZero) || (other.type == "int" &&other.intValue != 0 && !dontShowWhen.showWhenZero)
			    || (other.type == "float" && other.floatValue == 0 && dontShowWhen.showWhenZero)  || (other.type == "float" && other.floatValue > 0 && !dontShowWhen.showWhenZero) )
			{
				return EditorGUI.GetPropertyHeight(property);
			}
			return 0;
		}
	}

}