using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class ShaderBuilder {

		public enum ShaderPassType {
			Render,
			Picking,
			Selection
		}

		public string shader;

		// all multi_compiles are defined here
		public static Dictionary<string, MultiCompile[]> shaderKeywords = new Dictionary<string, MultiCompile[]> {
			{ "Disc", new[] { new MultiCompile( "INNER_RADIUS" ), new MultiCompile( "SECTOR" ) } },
			{ "Line 2D", new[] { new MultiCompile( "CAP_ROUND", "CAP_SQUARE" ) } },
			{ "Line 3D", new[] { new MultiCompile( "CAP_ROUND", "CAP_SQUARE" ) } },
			{ "Polyline 2D", new[] { new MultiCompile( "IS_JOIN_MESH" ), new MultiCompile( "JOIN_MITER", "JOIN_ROUND", "JOIN_BEVEL" ) /*, new MultiCompile( "CAP_ROUND", "CAP_SQUARE" )*/ } },
			{ "Rect", new[] { new MultiCompile( "CORNER_RADIUS" ), new MultiCompile( "BORDERED" ) } }
		};

		const char INDENT_STR = '\t';
		int indentLevel = 0;
		ShapesBlendMode blendMode;
		string shaderName;

		public struct BracketScope : IDisposable {
			ShaderBuilder builder;

			public BracketScope( ShaderBuilder builder, string line ) {
				this.builder = builder;
				builder.AppendLine( line + " {" );
				builder.indentLevel++;
			}

			public void Dispose() {
				builder.indentLevel--;
				builder.AppendLine( "}" );
			}
		}

		public BracketScope Scope( string line ) => new BracketScope( this, line );

		public ShaderBuilder( string name, ShapesBlendMode blendMode ) {
			this.blendMode = blendMode;
			this.shaderName = name;

			using( Scope( $"Shader \"Shapes/{name} {blendMode.ToString()}\"" ) ) {
				using( Scope( "SubShader" ) ) {
					using( Scope( "Tags" ) ) // sub shader tags
						AppendLines( blendMode.GetSubshaderTags() );

					using( Scope( "Pass" ) ) { // render pass
						AppendLines( blendMode.GetPassRenderStates() );
						AppendCGPROGRAM( ShaderPassType.Render );
					}

					using( Scope( "Pass" ) ) { // picking pass
						AppendLine( "Name \"Picking\"" );
						AppendLine( "Tags { \"LightMode\" = \"Picking\" }" );
						AppendLine( "Cull Off" );
						AppendCGPROGRAM( ShaderPassType.Picking );
					}

					using( Scope( "Pass" ) ) { // selection pass
						AppendLine( "Name \"Selection\"" );
						AppendLine( "Tags { \"LightMode\" = \"SceneSelectionPass\" }" );
						AppendLine( "Cull Off" );
						AppendCGPROGRAM( ShaderPassType.Selection );
					}
				}
			}
		}

		public void AppendCGPROGRAM( ShaderPassType passType ) {
			AppendLine( "CGPROGRAM" );
			indentLevel++;
			AppendLine( "#pragma vertex vert" );
			AppendLine( "#pragma fragment frag" );
			AppendLine( "#pragma multi_compile_instancing" );
			if( shaderKeywords.ContainsKey( shaderName ) )
				AppendLines( shaderKeywords[shaderName].Select( x => x.ToString() ) );
			AppendLine( $"#define {blendMode.BlendShaderDefine()}" );
			if( passType == ShaderPassType.Picking || passType == ShaderPassType.Selection ) {
				AppendLine( "#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap" );
				if( passType == ShaderPassType.Picking )
					AppendLine( "#define SCENE_VIEW_PICKING" );
				else if( passType == ShaderPassType.Selection )
					AppendLine( "#define SCENE_VIEW_OUTLINE_MASK" );
			}

			AppendLine( $"#include \"../../Core/{shaderName} Core.cginc\"" );
			indentLevel--;
			AppendLine( "ENDCG" );
		}


		string GetIndentation() => new string( INDENT_STR, indentLevel );
		void AppendLine( string s ) => shader += $"{GetIndentation()}{s}\n";

		void AppendLines( IEnumerable<string> strings ) {
			foreach( string s in strings )
				AppendLine( s );
		}

		void BeginScope( string line ) {
			AppendLine( line );
			indentLevel++;
		}

		void EndScope() {
			indentLevel--;
			AppendLine( "}" );
		}


	}

}