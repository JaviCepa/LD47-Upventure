using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Line) )]
	[CanEditMultipleObjects]
	public class LineEditor : ShapeRendererEditor {

		// Line specific things
		SerializedProperty propGeometry;
		SerializedProperty propStart;
		SerializedProperty propEnd;
		SerializedProperty propColorEnd;
		SerializedProperty propColorMode;
		SerializedProperty propThickness;
		SerializedProperty propThicknessSpace;
		SerializedProperty propDashed;
		SerializedProperty propDashSize;
		SerializedProperty propDashOffset;
		SerializedProperty propEndCaps;

		public override void OnEnable() {
			base.OnEnable();

			SerializedObject so = serializedObject;
			propGeometry = so.FindProperty( "geometry" );
			propColorMode = so.FindProperty( "colorMode" );
			propColorEnd = so.FindProperty( "colorEnd" );
			propStart = so.FindProperty( "start" );
			propEnd = so.FindProperty( "end" );
			propThickness = so.FindProperty( "thickness" );
			propThicknessSpace = so.FindProperty( "thicknessSpace" );
			propDashSize = so.FindProperty( "dashSize" );
			propDashed = so.FindProperty( "dashed" );
			propDashOffset = so.FindProperty( "dashOffset" );
			propEndCaps = so.FindProperty( "endCaps" );
		}


		public override void OnInspectorGUI() {
			SerializedObject so = serializedObject;
			bool multiselecting = so.isEditingMultipleObjects;
			so.Update();

			base.PropertyFieldBlendMode();

			bool updateGeometry = false;

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( propGeometry, new GUIContent( "Geometry" ) );
			if( EditorGUI.EndChangeCheck() )
				updateGeometry = true;

			// shape (positions & thickness)
			ShapesUI.BeginGroup();
			EditorGUILayout.PropertyField( propStart );
			EditorGUILayout.PropertyField( propEnd );
			ShapesUI.FloatInSpaceField( propThickness, propThicknessSpace );
			ShapesUI.EndGroup();

			// style (color, caps, dashes)
			ShapesUI.BeginGroup();
			EditorGUILayout.PropertyField( propColorMode );
			if( (Line.LineColorMode)propColorMode.enumValueIndex == Line.LineColorMode.Single ) {
				base.PropertyFieldColor();
			} else {
				using( new EditorGUILayout.HorizontalScope() ) {
					EditorGUILayout.PrefixLabel( "Colors" );
					base.PropertyFieldColor( GUIContent.none );
					EditorGUILayout.PropertyField( propColorEnd, GUIContent.none );
				}
			}

			using( new EditorGUILayout.HorizontalScope() ) {
				EditorGUILayout.PrefixLabel( "End Caps" );
				if( ShapesUI.DrawTypeSwitchButtons( propEndCaps, ShapesAssets.LineCapButtonContents, 20 ) )
					updateGeometry = true;
			}

			using( new EditorGUILayout.HorizontalScope() ) {
				EditorGUILayout.PropertyField( propDashed );
				using( new EditorGUI.DisabledGroupScope( propDashed.boolValue == false ) ) {
					ShapesUI.PropertyLabelWidth( propDashSize, "size", 30, GUILayout.Width( 60 ) );

					bool smol = Screen.width < 300;
					string label = smol ? "ofs" : "offset";
					int labelWidth = Screen.width < 300 ? 20 : 40;
					ShapesUI.PropertyLabelWidth( propDashOffset, label, labelWidth, GUILayout.Width( 80 ) );
					GUILayout.Label( " " );
				}

				ShapesUI.EndGroup();
			}

			so.ApplyModifiedProperties();

			if( updateGeometry ) {
				foreach( Line line in targets.Cast<Line>() )
					line.UpdateMesh();
			}
		}
	}

}