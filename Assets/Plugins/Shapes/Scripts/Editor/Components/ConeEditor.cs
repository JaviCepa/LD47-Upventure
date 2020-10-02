using System.Linq;
using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Cone) )]
	[CanEditMultipleObjects]
	public class ConeEditor : ShapeRendererEditor {

		SerializedProperty propRadius;
		SerializedProperty propLength;
		SerializedProperty propSizeSpace;
		SerializedProperty propFillCap;

		public override void OnEnable() {
			base.OnEnable();
			SerializedObject so = serializedObject;
			propRadius = so.FindProperty( "radius" );
			propLength = so.FindProperty( "length" );
			propSizeSpace = so.FindProperty( "sizeSpace" );
			propFillCap = so.FindProperty( "fillCap" );
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			base.DrawProperties();
			ShapesUI.FloatInSpaceField( propRadius, propSizeSpace );
			ShapesUI.FloatInSpaceField( propLength, propSizeSpace, spaceEnabled: false );
			EditorGUILayout.PropertyField( propFillCap );
			if( serializedObject.ApplyModifiedProperties() )
				foreach( Cone cone in targets.Cast<Cone>() )
					cone.UpdateMesh();
		}

	}

}