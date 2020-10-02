Shader "Shapes/Line 3D Opaque" {
	SubShader {
		Tags {
			"IgnoreProjector" = "True"
			"Queue" = "AlphaTest"
			"RenderType" = "TransparentCutout"
			"DisableBatching" = "True"
		}
		Pass {
			Cull Off
			AlphaToMask On
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
				#pragma multi_compile __ CAP_ROUND CAP_SQUARE
				#define OPAQUE
				#include "../../Core/Line 3D Core.cginc"
			ENDCG
		}
		Pass {
			Name "Picking"
			Tags { "LightMode" = "Picking" }
			Cull Off
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
				#pragma multi_compile __ CAP_ROUND CAP_SQUARE
				#define OPAQUE
				#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
				#define SCENE_VIEW_PICKING
				#include "../../Core/Line 3D Core.cginc"
			ENDCG
		}
		Pass {
			Name "Selection"
			Tags { "LightMode" = "SceneSelectionPass" }
			Cull Off
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
				#pragma multi_compile __ CAP_ROUND CAP_SQUARE
				#define OPAQUE
				#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
				#define SCENE_VIEW_OUTLINE_MASK
				#include "../../Core/Line 3D Core.cginc"
			ENDCG
		}
	}
}
