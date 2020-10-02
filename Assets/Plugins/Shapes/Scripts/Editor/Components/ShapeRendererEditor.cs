using UnityEngine;
using UnityEditor;
using System.Linq;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

    //[CustomEditor( typeof( ShapeRenderer ) )]
    [CanEditMultipleObjects]
    public class ShapeRendererEditor : Editor {

        protected SerializedProperty propColor;
        SerializedProperty propBlendMode;
        static GUIContent blendModeGuiContent = new GUIContent(
            "Blend Mode",
            "Opaque does not support partial transparency, " +
            "but will write to the depth buffer and sort correctly. " +
            "For best results, use MSAA in your project to avoid aliasing " +
            "(note that it may still be aliased in the scene view)\n" +
            "\n" +
            "Transparent supports partial transparency, " +
            "but may not sort properly in some cases.\n" + 
            "\n" +
            "Additive is good for glowing/brightening effects against dark backgrounds\n" +
            "\n" +
            "Multiplicative is good for tinting/darkening effects against bright backgrounds"
        );


        public virtual void OnEnable() {
            SerializedObject so = serializedObject;
            propColor = so.FindProperty( "color" );
            propBlendMode = so.FindProperty( "blendMode" );
            // hide mesh filter/renderer components
            foreach( ShapeRenderer shape in targets.Cast<ShapeRenderer>() ) 
                shape.HideMeshFilterRenderer();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            //DrawProperties();
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawProperties() {
            PropertyFieldBlendMode();
            PropertyFieldColor();
        }

        protected void PropertyFieldBlendMode() {
            EditorGUILayout.PropertyField( propBlendMode, blendModeGuiContent );
        }
        protected void PropertyFieldColor() => EditorGUILayout.PropertyField( propColor );
        protected void PropertyFieldColor(string s ) => EditorGUILayout.PropertyField( propColor, new GUIContent(s) );
        protected void PropertyFieldColor( GUIContent content ) => EditorGUILayout.PropertyField( propColor, content );
        
        public bool HasFrameBounds() => true;
        public Bounds OnGetFrameBounds() {
            if( serializedObject.isEditingMultipleObjects) {
                // this only works for multiselecting shapes of the same type
                // todo: might be able to make a solution using Editor.CreateEditor shenanigans
                Bounds bounds = ( (ShapeRenderer)serializedObject.targetObjects[0] ).GetWorldBounds();
                for( int i = 1; i < serializedObject.targetObjects.Length; i++ )
                    bounds.Encapsulate( ( (ShapeRenderer)serializedObject.targetObjects[i] ).GetWorldBounds() );
                return bounds;
            } else {
                return ( (ShapeRenderer)target ).GetWorldBounds();
            }
        }

    }
}


