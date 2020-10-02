using UnityEngine;
using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Disc) )]
	[CanEditMultipleObjects]
	public class DiscEditor : ShapeRendererEditor {

		Disc disc;
		SerializedProperty propDiscType;
		SerializedProperty propColorMode;

		SerializedProperty propColorOuterStart;
		SerializedProperty propColorInnerEnd;
		SerializedProperty propColorOuterEnd;

		SerializedProperty propAngRadStart;
		SerializedProperty propAngRadEnd;
		SerializedProperty propAngUnitInput;
		SerializedProperty propRadius;
		SerializedProperty propRadiusSpace;
		SerializedProperty propThickness;
		SerializedProperty propThicknessSpace;
		SerializedProperty propArcEndCaps;

		public override void OnEnable() {
			base.OnEnable();

			disc = target as Disc;
			SerializedObject so = serializedObject;
			propDiscType = so.FindProperty( "type" );
			propColorMode = so.FindProperty( "colorMode" );
			propColorOuterStart = so.FindProperty( "colorOuterStart" );
			propColorInnerEnd = so.FindProperty( "colorInnerEnd" );
			propColorOuterEnd = so.FindProperty( "colorOuterEnd" );
			propAngRadStart = so.FindProperty( "angRadiansStart" );
			propAngRadEnd = so.FindProperty( "angRadiansEnd" );
			propAngUnitInput = so.FindProperty( "angUnitInput" );
			propRadius = so.FindProperty( "radius" );
			propRadiusSpace = so.FindProperty( "radiusSpace" );
			propThickness = so.FindProperty( "thickness" );
			propThicknessSpace = so.FindProperty( "thicknessSpace" );
			propArcEndCaps = so.FindProperty( "arcEndCaps" );
		}


		public override void OnInspectorGUI() {
			serializedObject.Update();
			base.PropertyFieldBlendMode();

			// Color properties
			EditorGUILayout.PropertyField( propColorMode );
			switch( (Disc.DiscColorMode)propColorMode.enumValueIndex ) {
				case Disc.DiscColorMode.Single:
					base.PropertyFieldColor();
					break;
				case Disc.DiscColorMode.Radial:
					base.PropertyFieldColor( "Inner" );
					EditorGUILayout.PropertyField( propColorOuterStart, new GUIContent( "Outer" ) );
					break;
				case Disc.DiscColorMode.Angular:
					base.PropertyFieldColor( "Start" );
					EditorGUILayout.PropertyField( propColorInnerEnd, new GUIContent( "End" ) );
					break;
				case Disc.DiscColorMode.Bilinear:
					base.PropertyFieldColor( "Inner Start" );
					EditorGUILayout.PropertyField( propColorOuterStart, new GUIContent( "Outer Start" ) );
					EditorGUILayout.PropertyField( propColorInnerEnd, new GUIContent( "Inner End" ) );
					EditorGUILayout.PropertyField( propColorOuterEnd, new GUIContent( "Outer End" ) );
					break;
			}

			ShapesUI.DrawTypeSwitchButtons( propDiscType, ShapesAssets.DiscTypeButtonContents );
			if( propDiscType.enumValueIndex == (int)Disc.DiscType.Arc )
				ShapesUI.EnumToggleProperty( propArcEndCaps, "Round Caps" );
			ShapesUI.FloatInSpaceField( propRadius, propRadiusSpace );
			using( new EditorGUI.DisabledScope( disc.HasThickness == false && serializedObject.isEditingMultipleObjects == false ) )
				ShapesUI.FloatInSpaceField( propThickness, propThicknessSpace );
			DrawAngleProperties();
			serializedObject.ApplyModifiedProperties();
		}


		static GUILayoutOption[] angLabelLayout = { GUILayout.Width( 50 ) };

		void DrawAngleProperties() {
			using( new EditorGUI.DisabledScope( disc.HasSector == false && serializedObject.isEditingMultipleObjects == false ) ) {
				ShapesUI.AngleProperty( propAngRadStart, "Angle start", propAngUnitInput, angLabelLayout );
				ShapesUI.AngleProperty( propAngRadEnd, "Angle end", propAngUnitInput, angLabelLayout );
				GUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel( " " );
				GUIContent[] angLabels = ( Screen.width < 300 ) ? ShapesAssets.AngleUnitButtonContentsShort : ShapesAssets.AngleUnitButtonContents;
				ShapesUI.DrawTypeSwitchButtons( propAngUnitInput, angLabels, 15 );
				GUILayout.EndHorizontal();
			}
		}


	}

}