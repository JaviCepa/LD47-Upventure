using UnityEngine;
using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Torus) )]
	[CanEditMultipleObjects]
	public class TorusEditor : ShapeRendererEditor {

		SerializedProperty propRadius;
		SerializedProperty propRadiusSpace;
		SerializedProperty propThickness;
		SerializedProperty propThicknessSpace;

		public override void OnEnable() {
			base.OnEnable();
			SerializedObject so = serializedObject;
			propRadius = so.FindProperty( "radius" );
			propRadiusSpace = so.FindProperty( "radiusSpace" );
			propThickness = so.FindProperty( "thickness" );
			propThicknessSpace = so.FindProperty( "thicknessSpace" );
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			base.DrawProperties();
			ShapesUI.FloatInSpaceField( propRadius, propRadiusSpace );
			ShapesUI.FloatInSpaceField( propThickness, propThicknessSpace );
			serializedObject.ApplyModifiedProperties();
		}

	}

}