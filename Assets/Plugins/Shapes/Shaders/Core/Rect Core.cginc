// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_DEFINE_INSTANCED_PROP(float4, _Rect)
#ifdef CORNER_RADIUS
    UNITY_DEFINE_INSTANCED_PROP(float4, _CornerRadii)
#endif
#ifdef BORDERED
    UNITY_DEFINE_INSTANCED_PROP(float, _Thickness)
#endif
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
    float4 vertex : POSITION;
    float2 uv0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
}; 
struct VertexOutput {
    float4 pos : SV_POSITION;
#if defined(BORDERED) || defined(CORNER_RADIUS)
    float2 uv0 : TEXCOORD0;
    fixed2 nrmCoord : TEXCOORD1;
    float2 size : TEXCOORD2;
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert (VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
	float4 rect = UNITY_ACCESS_INSTANCED_PROP(Props, _Rect);
	
	#if defined(BORDERED) || defined(CORNER_RADIUS)
	    o.nrmCoord = v.vertex.xy;
	#endif
	
	v.vertex.xy = Remap( float2(-1, -1), float2(1, 1), rect.xy, rect.xy + rect.zw, v.vertex );
	
    #if defined(BORDERED) || defined(CORNER_RADIUS)
        o.uv0 = v.vertex.xy;
        o.size = rect.zw;
    #endif
    
    o.pos = UnityObjectToClipPos( v.vertex );
    return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	
	float4 rect = UNITY_ACCESS_INSTANCED_PROP(Props, _Rect);
	float2 rectCenter = rect.xy + rect.zw/2;
	
	#ifdef CORNER_RADIUS
	    float4 cornerRadii = UNITY_ACCESS_INSTANCED_PROP(Props, _CornerRadii);
	    fixed2 sgn = sign(i.nrmCoord);
        int rComp = sgn.x-0.5*sgn.x*sgn.y+1.5; // thanks @khyperia <3
	    float cornerRadius = cornerRadii[rComp];
	    float maxRadius = min(i.size.x, i.size.y) / 2;
	    cornerRadius = min( cornerRadius, maxRadius );
    #endif
	
	
	// base sdf
	#ifdef CORNER_RADIUS
        float2 indentBoxSize = (rect.zw - cornerRadius.xx*2);
        float boxSdf = SdfBox( i.uv0.xy - rectCenter, indentBoxSize/2 ) - cornerRadius;
    #elif defined(BORDERED)
        float boxSdf = SdfBox( i.uv0.xy - rectCenter, rect.zw/2 );
    #endif
    
    // apply border to sdf
    #ifdef BORDERED
	    float thickness = UNITY_ACCESS_INSTANCED_PROP(Props, _Thickness);
        float halfthick = thickness / 2;
	    #if LOCAL_ANTI_ALIASING_QUALITY > 0
            float boxSdfPd = PD( boxSdf ); // todo: this has minor artifacts on inner corners, might want to separate masks by axis
            boxSdf = abs(boxSdf + halfthick) - halfthick;
            float shape_mask = 1.0-StepThresholdPD( boxSdf, boxSdfPd );
        #else
            boxSdf = abs(boxSdf + halfthick) - halfthick;
            float shape_mask = 1-StepAA(boxSdf);
        #endif
    #elif defined(CORNER_RADIUS)
        float shape_mask = 1.0-StepAA( boxSdf );
    #else
        float shape_mask = 1;
    #endif
    	
	float4 shape_color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
	
	return ShapesOutput( shape_color, shape_mask );
}