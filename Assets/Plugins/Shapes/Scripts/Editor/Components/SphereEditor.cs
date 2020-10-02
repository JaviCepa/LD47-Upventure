using UnityEngine;
using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Sphere) )]
	[CanEditMultipleObjects]
	public class SphereEditor : ShapeRendererEditor {

		SerializedProperty propRadius;
		SerializedProperty propRadiusSpace;

		public override void OnEnable() {
			base.OnEnable();
			SerializedObject so = serializedObject;
			propRadius = so.FindProperty( "radius" );
			propRadiusSpace = so.FindProperty( "radiusSpace" );
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			base.DrawProperties();
			ShapesUI.FloatInSpaceField( propRadius, propRadiusSpace );
			serializedObject.ApplyModifiedProperties();
		}

	}

}