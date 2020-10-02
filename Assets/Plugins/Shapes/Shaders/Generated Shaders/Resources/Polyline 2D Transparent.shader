Shader "Shapes/Polyline 2D Transparent" {
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
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
				#pragma multi_compile __ IS_JOIN_MESH
				#pragma multi_compile __ JOIN_MITER JOIN_ROUND JOIN_BEVEL
				#define TRANSPARENT
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
				#define TRANSPARENT
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
				#define TRANSPARENT
				#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
				#define SCENE_VIEW_OUTLINE_MASK
				#include "../../Core/Polyline 2D Core.cginc"
			ENDCG
		}
	}
}
