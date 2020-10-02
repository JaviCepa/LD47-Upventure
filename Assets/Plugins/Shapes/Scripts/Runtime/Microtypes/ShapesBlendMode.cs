using System.Collections.Generic;
using System;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public enum ShapesBlendMode {
		//					ZWrite	RenderType	Blending		AlphaToCoverage
		Opaque, //	        on		opaque		none			on
		Transparent, //	    off		transparent	alpha blend		off
		Additive, //        off		transparent additive		off
		Multiplicative, //  off		transparent multiply		off
	}

	public static class ShapeBlendModeExtensions {

		static bool ZWrite( this ShapesBlendMode blendMode ) => blendMode == ShapesBlendMode.Opaque;
		static bool AlphaToMask( this ShapesBlendMode blendMode ) => blendMode == ShapesBlendMode.Opaque;
		static string RenderType( this ShapesBlendMode blendMode ) => blendMode == ShapesBlendMode.Opaque ? "TransparentCutout" : "Transparent";
		static string Queue( this ShapesBlendMode blendMode ) => blendMode == ShapesBlendMode.Opaque ? "AlphaTest" : "Transparent";
		static bool HasSpecialBlendMode( this ShapesBlendMode blendMode ) => blendMode != ShapesBlendMode.Opaque;
		public static string BlendShaderDefine( this ShapesBlendMode blendMode ) => blendMode.ToString().ToUpper();

		static string GetShaderBlendMode( this ShapesBlendMode blendMode ) {
			switch( blendMode ) {
				case ShapesBlendMode.Opaque:         return "One Zero";
				case ShapesBlendMode.Transparent:    return "SrcAlpha OneMinusSrcAlpha";
				case ShapesBlendMode.Additive:       return "One One";
				case ShapesBlendMode.Multiplicative: return "DstColor Zero";
				default:                             throw new ArgumentOutOfRangeException( nameof(blendMode), blendMode, null );
			}
		}

		public static IEnumerable<string> GetSubshaderTags( this ShapesBlendMode blendMode ) {
			string Tag( string key, string value ) => $"\"{key}\" = \"{value}\"";
			yield return Tag( "IgnoreProjector", "True" );
			yield return Tag( "Queue", blendMode.Queue() );
			yield return Tag( "RenderType", blendMode.RenderType() );
			yield return Tag( "DisableBatching", "True" );
		}

		public static IEnumerable<string> GetPassRenderStates( this ShapesBlendMode blendMode ) {
			yield return "Cull Off";
			if( blendMode.ZWrite() == false )
				yield return "ZWrite Off";
			if( blendMode.AlphaToMask() )
				yield return "AlphaToMask On";
			if( blendMode.HasSpecialBlendMode() )
				yield return $"Blend {blendMode.GetShaderBlendMode()}";
		}


	}

}