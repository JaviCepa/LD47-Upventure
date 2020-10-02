using System;
using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class ShapesMaterials {

		const bool USE_INSTANCING = true;
		public const string SHAPES_SHADER_PATH_PREFIX = "Shapes/";



		readonly Material[] materials;
		public Material this[ ShapesBlendMode type ] => materials[(int)type];

		public ShapesMaterials( string shaderName, params string[] keywords ) {
			int count = Enum.GetNames( typeof(ShapesBlendMode) ).Length;
			materials = new Material[count];
			for( int i = 0; i < count; i++ )
				materials[i] = InitMaterial( shaderName, ( (ShapesBlendMode)i ).ToString(), keywords );
		}

		public static string GetMaterialName( string shaderName, string blendModeSuffix, params string[] keywords ) {
			string keywordsSuffix = "";
			if( keywords != null && keywords.Length > 0 ) {
				keywordsSuffix = $" [{string.Join( "][", keywords )}]";
			}
			return $"{shaderName} {blendModeSuffix}{keywordsSuffix}";
		}

		static Material InitMaterial( string shaderName, string blendModeSuffix, params string[] keywords ) {
			
			#if UNITY_EDITOR
			// in editor, we want to use the material *assets*, not create any materials
			string path = $"{ShapesIO.GeneratedMaterialsFolder}/{GetMaterialName( shaderName, blendModeSuffix, keywords )}.mat";
			Material mat = AssetDatabase.LoadAssetAtPath<Material>( path );
			if( mat == null )
				Debug.LogWarning( "Failed to load material " + path );
			return mat;
			#else
				// in builds, we want to create them
				shaderName = SHAPES_SHADER_PATH_PREFIX + shaderName + " " + blendModeSuffix;
				Shader shaderObj = Shader.Find( shaderName );
				if( shaderObj == null ) {
					Debug.LogError( "Could not find shader " + shaderName );
					return null;
				}

				Material mat = new Material( shaderObj ) { hideFlags = HideFlags.HideAndDontSave, enableInstancing = USE_INSTANCING };
				if( keywords != null )
					foreach( string keyword in keywords )
						mat.EnableKeyword( keyword );
				return mat;
			#endif
		}


	}

}