using UnityEngine;
using UnityEditor;
using System.Linq;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Rectangle) )]
	[CanEditMultipleObjects]
	public class RectangleEditor : ShapeRendererEditor {
		Disc disc;
		SerializedProperty propRectType;
		SerializedProperty propCornerRadii;
		SerializedProperty propCornerRadiusMode;
		SerializedProperty propWidth;
		SerializedProperty propHeight;
		SerializedProperty propPivot;
		SerializedProperty propThickness;

		public override void OnEnable() {
			base.OnEnable();
			disc = target as Disc;
			SerializedObject so = serializedObject;
			propRectType = so.FindProperty( "type" );
			propCornerRadii = so.FindProperty( "cornerRadii" );
			propCornerRadiusMode = so.FindProperty( "cornerRadiusMode" );
			propWidth = so.FindProperty( "width" );
			propHeight = so.FindProperty( "height" );
			propPivot = so.FindProperty( "pivot" );
			propThickness = so.FindProperty( "thickness" );
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			base.DrawProperties();
			bool multiEditing = serializedObject.isEditingMultipleObjects;

			using( new EditorGUILayout.HorizontalScope() ) {
				EditorGUILayout.PrefixLabel( "Style" );
				ShapesUI.DrawTypeSwitchButtons( propRectType, ShapesAssets.RectTypeButtonContents, 20 );
			}


			EditorGUILayout.PropertyField( propPivot );
			using( new EditorGUILayout.HorizontalScope() ) {
				EditorGUILayout.PrefixLabel( "Size" );
				using( ShapesUI.TempLabelWidth( 14 ) ) {
					EditorGUILayout.PropertyField( propWidth, new GUIContent( "X" ), GUILayout.MinWidth( 20 ) );
					EditorGUILayout.PropertyField( propHeight, new GUIContent( "Y" ), GUILayout.MinWidth( 20 ) );
				}
			}

			bool isHollow = ( (Rectangle)target ).IsHollow;
			using( new EditorGUI.DisabledScope( !multiEditing && isHollow == false ) ) {
				EditorGUILayout.PropertyField( propThickness );
			}

			bool hasRadius = ( (Rectangle)target ).IsRounded;

			using( new EditorGUI.DisabledScope( hasRadius == false ) ) {
				EditorGUILayout.PropertyField( propCornerRadiusMode, new GUIContent( "Radius Mode" ) );
				CornerRadiusProperties();
			}

			serializedObject.ApplyModifiedProperties();
		}

		void CornerRadiusProperties() {
			Rectangle.RectangleCornerRadiusMode radiusMode = (Rectangle.RectangleCornerRadiusMode)propCornerRadiusMode.enumValueIndex;

			if( radiusMode == Rectangle.RectangleCornerRadiusMode.Uniform ) {
				using( var change = new EditorGUI.ChangeCheckScope() ) {
					EditorGUI.showMixedValue = propCornerRadii.hasMultipleDifferentValues;
					float newRadius = Mathf.Max( 0f, EditorGUILayout.FloatField( "Radius", propCornerRadii.vector4Value.x ) );
					EditorGUI.showMixedValue = false;
					if( change.changed && newRadius != propCornerRadii.vector4Value.x )
						propCornerRadii.vector4Value = new Vector4( newRadius, newRadius, newRadius, newRadius );
				}
			} else { // per-corner
				SerializedProperty[] components = propCornerRadii.GetVisibleChildren().ToArray();
				(int component, string label )[] corners = { ( 1, "↖" ), ( 2, "↗" ), ( 0, "↙" ), ( 3, "↘" ) };
				void CornerField( string label, int component ) => EditorGUILayout.PropertyField( components[component], new GUIContent( label ), GUILayout.Width( 64 ) );

				void RowFields( string label, int a, int b ) {
					using( ShapesUI.Horizontal ) {
						GUILayout.Label( label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
						using( ShapesUI.TempLabelWidth( 18 ) ) {
							CornerField( corners[a].label, corners[a].component );
							CornerField( corners[b].label, corners[b].component );
						}
					}
				}

				RowFields( "Radii", 0, 1 );
				RowFields( " ", 2, 3 );
			}
		}
	}

}