Shader "Shapes/Rect Opaque" {
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
				#pragma multi_compile __ CORNER_RADIUS
				#pragma multi_compile __ BORDERED
				#define OPAQUE
				#include "../../Core/Rect Core.cginc"
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
				#pragma multi_compile __ CORNER_RADIUS
				#pragma multi_compile __ BORDERED
				#define OPAQUE
				#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
				#define SCENE_VIEW_PICKING
				#include "../../Core/Rect Core.cginc"
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
				#pragma multi_compile __ CORNER_RADIUS
				#pragma multi_compile __ BORDERED
				#define OPAQUE
				#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
				#define SCENE_VIEW_OUTLINE_MASK
				#include "../../Core/Rect Core.cginc"
			ENDCG
		}
	}
}
