// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_DEFINE_INSTANCED_PROP(float4, _ColorEnd)
UNITY_DEFINE_INSTANCED_PROP(float3, _PointStart)
UNITY_DEFINE_INSTANCED_PROP(float3, _PointEnd)
UNITY_DEFINE_INSTANCED_PROP(float, _Thickness)
UNITY_DEFINE_INSTANCED_PROP(int, _ThicknessSpace)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
	float4 vertex : POSITION;
	float2 uv0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
	float4 pos : SV_POSITION;
	#if defined(CAP_SQUARE)
	    float colorBlend : TEXCOORD0; // needed since we need unclamped color blend value to the frag shader
	#else
	    float4 color : TEXCOORD0;
	#endif
	float3 wPos : TEXCOORD1;
	float pxCoverage : TEXCOORD2;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert(VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	float3 a = LocalToWorldPos( UNITY_ACCESS_INSTANCED_PROP(Props, _PointStart) );
	float3 b = LocalToWorldPos( UNITY_ACCESS_INSTANCED_PROP(Props, _PointEnd) );
	float thickness = UNITY_ACCESS_INSTANCED_PROP(Props, _Thickness);
	float thicknessSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _ThicknessSpace);

	float lineLength;
	float3 right;
	float3 normal;
	float3 forward;
	GetDirMag(b - a, /*out*/ forward, /*out*/ lineLength);

    if( lineLength < 0.0001 ){ // degenerate case (start == end)
        right   = float3(1,0,0);
        normal  = float3(0,1,0);
        forward = float3(0,0,1);
    } else {
        float prettyVertical = abs(forward.y) >= 0.99;
        float3 upRef = prettyVertical ? float3(1,0,0) : float3(0,1,0);
        normal = normalize(cross(upRef,forward));
        right = cross( normal, forward );
    }

	float side = saturate( v.uv0.y );
	float3 vertOrigin = side > 0.5 ? b : a;
	float3 camForward = DirectionToNearPlanePos(vertOrigin);
	float3 camLineNormal = normalize(cross(camForward, forward));
    LineWidthData widthData = GetScreenSpaceWidthDataSimple( vertOrigin, camLineNormal, thickness, thicknessSpace );
    o.pxCoverage = widthData.thicknessPixelsTarget;
    float radius = widthData.thicknessMeters * 0.5;
	
	float3 localOffset = v.vertex - float3( 0, 0, saturate( v.uv0.y ) ); //  if uv >= 1 then subtract height (z) by 1 to make it a spherical offset
	localOffset *= radius;
	float3 vertPos = vertOrigin + localOffset.x * right + localOffset.y * normal;
	
	#ifdef CAP_ROUND
	    vertPos += localOffset.z * forward;
	#elif defined(CAP_SQUARE)
	    vertPos += (side*2-1) * forward * radius;
	#endif
	
	#if defined(CAP_SQUARE)
	    float k = 2 * radius / lineLength + 1;
        float m = -radius / lineLength;
        o.colorBlend = k * side + m;
	#else
        float4 colorStart = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
        float4 colorEnd = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorEnd); // todo: make the gradient thing be a thing
	    o.color = lerp( colorStart, colorEnd, side );
	#endif
	o.wPos = vertPos.xyz;
	o.pos = WorldToClipPos( vertPos.xyz );
	return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	
    #if defined(CAP_SQUARE)
        // interpolation of colors is done here because we need to clamp the color blend value in the frag shader
        // due to that being calculated in the vert shader, but the 0 and 1 crossings are offset from the vert
        // todo: use a proper cylinder mesh with extra verts for 0 and 1 crossings
        float4 colorStart = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
        float4 colorEnd = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorEnd); // todo: make the gradient thing be a thing
	    float4 shape_color = lerp( colorStart, colorEnd, saturate(i.colorBlend) );
    #else
	    float4 shape_color = i.color;
    #endif
    
    float shape_mask = saturate(i.pxCoverage);
	
	return ShapesOutput( shape_color, shape_mask );
}