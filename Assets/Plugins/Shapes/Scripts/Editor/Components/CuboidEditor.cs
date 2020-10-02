using UnityEngine;
using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Cuboid) )]
	[CanEditMultipleObjects]
	public class CuboidEditor : ShapeRendererEditor {

		SerializedProperty propSize;
		SerializedProperty propSizeSpace;

		public override void OnEnable() {
			base.OnEnable();
			SerializedObject so = serializedObject;
			propSize = so.FindProperty( "size" );
			propSizeSpace = so.FindProperty( "sizeSpace" );
		}
		
		public override void OnInspectorGUI() {
			serializedObject.Update();
			base.DrawProperties();
			ShapesUI.FloatInSpaceField( propSize, propSizeSpace );
			serializedObject.ApplyModifiedProperties();
		}

	}

}