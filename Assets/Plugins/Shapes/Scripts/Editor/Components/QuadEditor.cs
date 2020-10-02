using System;
using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Quad) )]
	[CanEditMultipleObjects]
	public class QuadEditor : ShapeRendererEditor {

		SerializedProperty propA;
		SerializedProperty propB;
		SerializedProperty propC;
		SerializedProperty propD;
		SerializedProperty propColorMode;
		SerializedProperty propColorB;
		SerializedProperty propColorC;
		SerializedProperty propColorD;
		SerializedProperty propAutoSetD;

		public override void OnEnable() {
			base.OnEnable();
			SerializedObject so = serializedObject;
			propA = so.FindProperty( "a" );
			propB = so.FindProperty( "b" );
			propC = so.FindProperty( "c" );
			propD = so.FindProperty( "d" );
			propColorMode = so.FindProperty( "colorMode" );
			propColorB = so.FindProperty( "colorB" );
			propColorC = so.FindProperty( "colorC" );
			propColorD = so.FindProperty( "colorD" );
			propAutoSetD = so.FindProperty( "autoSetD" );
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			base.PropertyFieldBlendMode();
			EditorGUILayout.PropertyField( propColorMode );

			bool dEnabled = propAutoSetD.boolValue == false;
			Vector3 dAuto = ( target as Quad ).DAuto;

			switch( (Quad.QuadColorMode)propColorMode.enumValueIndex ) {
				case Quad.QuadColorMode.Single:
					ShapesUI.PosColorField( "A", propA, base.propColor );
					ShapesUI.PosColorField( "B", propB, base.propColor, false );
					ShapesUI.PosColorField( "C", propC, base.propColor, false );
					ShapesUI.PosColorFieldSpecialOffState( "D", propD, dAuto, base.propColor, false, dEnabled );
					break;
				case Quad.QuadColorMode.Horizontal:
					ShapesUI.PosColorField( "A", propA, base.propColor );
					ShapesUI.PosColorField( "B", propB, base.propColor, false );
					ShapesUI.PosColorField( "C", propC, propColorC );
					ShapesUI.PosColorFieldSpecialOffState( "D", propD, dAuto, propColorC, false, dEnabled );
					break;
				case Quad.QuadColorMode.Vertical:
					ShapesUI.PosColorField( "A", propA, propColorD );
					ShapesUI.PosColorField( "B", propB, propColorB );
					ShapesUI.PosColorField( "C", propC, propColorB, false );
					ShapesUI.PosColorFieldSpecialOffState( "D", propD, dAuto, propColorD, false, dEnabled );
					break;
				case Quad.QuadColorMode.PerCorner:
					ShapesUI.PosColorField( "A", propA, base.propColor );
					ShapesUI.PosColorField( "B", propB, propColorB );
					ShapesUI.PosColorField( "C", propC, propColorC );
					ShapesUI.PosColorFieldSpecialOffState( "D", propD, dAuto, propColorD, true, dEnabled );
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			using( new EditorGUILayout.HorizontalScope() ) {
				GUILayout.Label( GUIContent.none, GUILayout.Width( ShapesUI.POS_COLOR_FIELD_LABEL_WIDTH ) );
				EditorGUILayout.PropertyField( propAutoSetD, GUIContent.none, GUILayout.Width( 16 ) );
				GUILayout.Label( "Auto-set D" );
			}

			serializedObject.ApplyModifiedProperties();
		}

	}

}