// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_DEFINE_INSTANCED_PROP(float, _Radius)
UNITY_DEFINE_INSTANCED_PROP(float, _RadiusSpace)
UNITY_DEFINE_INSTANCED_PROP(float, _Thickness)
UNITY_DEFINE_INSTANCED_PROP(float, _ThicknessSpace)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 uv0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
	float4 pos : SV_POSITION;
	float pxCoverage : TEXCOORD0;
	float4 debug : TEXCOORD1;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert(VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    float radiusMajorTarget = UNITY_ACCESS_INSTANCED_PROP(Props, _Radius);
    int radiusMajorSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _RadiusSpace);
    int thicknessSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _ThicknessSpace);
    float thicknessTarget = UNITY_ACCESS_INSTANCED_PROP(Props, _Thickness);
    
    
    // calc radius & tube center
    float3 objectOrigin = LocalToWorldPos(float3(0,0,0));
    float3 camRight = CameraToWorldVec(float3(1,0,0));
	LineWidthData widthDataRadius = GetScreenSpaceWidthDataSimple( objectOrigin, camRight, radiusMajorTarget * 2 /*to thickness*/, radiusMajorSpace );
	float radiusMajor = widthDataRadius.thicknessMeters / 2;
    float3 dirFromCenter = normalize( float3( v.vertex.xy, 0 ) );
    float3 tubeCenter = dirFromCenter * radiusMajor;
    float3 tubeCenterWorld = LocalToWorldPos( tubeCenter );
	
	// calc thickness
    float3 tangentWorld = LocalToWorldVec( float3(dirFromCenter.y,-dirFromCenter.x, 0) );
    float3 camForward = GetCameraForwardDirection();
	float3 camLineNormal = normalize(cross(camForward, tangentWorld));
	LineWidthData widthDataThickness = GetScreenSpaceWidthDataSimple( tubeCenterWorld, camLineNormal, thicknessTarget, thicknessSpace );
	o.pxCoverage = widthDataThickness.thicknessPixelsTarget;
	float thicknessRadius = widthDataThickness.thicknessMeters * 0.5;
	
	float3 localPos = tubeCenter + v.normal * thicknessRadius; 
	o.pos = LocalToClipPos( localPos );
	return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	float4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
	return ShapesOutput( color, saturate(i.pxCoverage) );
}