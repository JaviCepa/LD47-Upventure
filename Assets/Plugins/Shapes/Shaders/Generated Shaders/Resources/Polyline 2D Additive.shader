Shader "Shapes/Polyline 2D Additive" {
	SubShader {
		Tags {
			"IgnoreProjector" = "True"
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"DisableBatching" = "True"
		}
		Pass {
			Cull Off
			ZWrite Off
			Blend One One
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
				#pragma multi_compile __ IS_JOIN_MESH
				#pragma multi_compile __ JOIN_MITER JOIN_ROUND JOIN_BEVEL
				#define ADDITIVE
				#include "../../Core/Polyline 2D Core.cginc"
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
				#pragma multi_compile __ IS_JOIN_MESH
				#pragma multi_compile __ JOIN_MITER JOIN_ROUND JOIN_BEVEL
				#define ADDITIVE
				#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
				#define SCENE_VIEW_PICKING
				#include "../../Core/Polyline 2D Core.cginc"
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
				#pragma multi_compile __ IS_JOIN_MESH
				#pragma multi_compile __ JOIN_MITER JOIN_ROUND JOIN_BEVEL
				#define ADDITIVE
				#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
				#define SCENE_VIEW_OUTLINE_MASK
				#include "../../Core/Polyline 2D Core.cginc"
			ENDCG
		}
	}
}
