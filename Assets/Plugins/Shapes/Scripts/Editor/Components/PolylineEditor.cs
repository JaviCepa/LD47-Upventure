using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Polyline) )]
	[CanEditMultipleObjects]
	public class PolylineEditor : ShapeRendererEditor {

		SerializedProperty propPolyPoints;
		SerializedProperty propGeometry;
		SerializedProperty propJoins;
		SerializedProperty propClosed;
		SerializedProperty propThickness;
		SerializedProperty propThicknessSpace;

		ReorderableList pointList;

		const int MANY_POINTS = 20;
		[SerializeField] bool hasManyPoints;
		[SerializeField] bool showPointList = true;

		public override void OnEnable() {
			base.OnEnable();
			SerializedObject so = serializedObject;
			propPolyPoints = so.FindProperty( "polyPoints" );
			propJoins = so.FindProperty( "joins" );
			propGeometry = so.FindProperty( "geometry" );
			propClosed = so.FindProperty( "closed" );
			propThickness = so.FindProperty( "thickness" );
			propThicknessSpace = so.FindProperty( "thicknessSpace" );

			pointList = new ReorderableList( so, propPolyPoints, true, true, true, true ) {
				drawElementCallback = DrawPointElement,
				drawHeaderCallback = PointListHeader,
				onChangedCallback = ( x ) => UpdateMesh()
			};
			
			if( pointList.count > MANY_POINTS ) {
				hasManyPoints = true;
				showPointList = false;
			}
			
		}

		void UpdateMesh() => targets.Cast<Polyline>().ForEach( x => x.UpdateMesh( force: true ) );

		public override void OnInspectorGUI() {
			serializedObject.Update();
			base.DrawProperties();
			EditorGUILayout.PropertyField( propGeometry );
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( propJoins );
			ShapesUI.FloatInSpaceField( propThickness, propThicknessSpace );
			
			if( hasManyPoints ) { // to prevent lag when inspecting polylines with many points
				string foldoutLabel = showPointList ? "Hide" : "Show Points"; 
				showPointList = GUILayout.Toggle( showPointList, foldoutLabel, EditorStyles.foldout );
			}
			if( showPointList )
				pointList.DoLayoutList();
			
			if( serializedObject.ApplyModifiedProperties() )
				UpdateMesh();
		}

		void PointListHeader( Rect r ) {
			const int checkboxSize = 14;
			const int closedSize = 50;

			Rect rLabel = r;
			rLabel.width = r.width - checkboxSize - closedSize;
			Rect rCheckbox = r;
			rCheckbox.x = r.xMax - checkboxSize;
			rCheckbox.width = checkboxSize;
			Rect rClosed = r;
			rClosed.x = rLabel.xMax;
			rClosed.width = closedSize;
			EditorGUI.LabelField( rLabel, "Points" );
			EditorGUI.LabelField( rClosed, "Closed" );
			EditorGUI.PropertyField( rCheckbox, propClosed, GUIContent.none );
		}

		// Draws the elements on the list
		void DrawPointElement( Rect r, int i, bool isActive, bool isFocused ) {
			r.yMin += 1;
			r.yMax -= 2;
			SerializedProperty prop = propPolyPoints.GetArrayElementAtIndex( i );
			SerializedProperty pPoint = prop.FindPropertyRelative( nameof(PolylinePoint.point) );
			SerializedProperty pThickness = prop.FindPropertyRelative( nameof(PolylinePoint.thickness) );
			SerializedProperty pColor = prop.FindPropertyRelative( nameof(PolylinePoint.color) );
			ShapesUI.PosThicknessColorField( r, pPoint, pThickness, pColor );
			pThickness.floatValue = Mathf.Max( 0.001f, pThickness.floatValue ); // Make sure it's never 0 or under
		}


	}

}