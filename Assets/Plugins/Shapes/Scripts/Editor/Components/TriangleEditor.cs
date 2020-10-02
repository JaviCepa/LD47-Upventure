using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {
	
	[CustomEditor( typeof(Triangle) )]
	[CanEditMultipleObjects]
	public class TriangleEditor : ShapeRendererEditor {

		SerializedProperty propA;
		SerializedProperty propB;
		SerializedProperty propC;
		SerializedProperty propColorMode;
		SerializedProperty propColorB;
		SerializedProperty propColorC;
		
		public override void OnEnable() {
			base.OnEnable();
			SerializedObject so = serializedObject;
			propA = so.FindProperty( "a" );
			propB = so.FindProperty( "b" );
			propC = so.FindProperty( "c" );
			propColorMode = so.FindProperty( "colorMode" );
			propColorB = so.FindProperty( "colorB" );
			propColorC = so.FindProperty( "colorC" );
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			base.PropertyFieldBlendMode();
			EditorGUILayout.PropertyField( propColorMode );

			if( propColorMode.enumValueIndex == (int)Triangle.TriangleColorMode.Single ) {
				ShapesUI.PosColorField( "A", propA, base.propColor );
				ShapesUI.PosColorField( "B", propB, base.propColor, false );
				ShapesUI.PosColorField( "C", propC, base.propColor, false );
			} else {
				ShapesUI.PosColorField( "A", propA, base.propColor );
				ShapesUI.PosColorField( "B", propB, propColorB );
				ShapesUI.PosColorField( "C", propC, propColorC );
			}
			
			serializedObject.ApplyModifiedProperties();
		}
		
	}
	
}

