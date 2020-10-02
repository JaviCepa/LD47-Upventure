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
UNITY_DEFINE_INSTANCED_PROP(float, _DashSize)
UNITY_DEFINE_INSTANCED_PROP(float, _DashOffset)
UNITY_DEFINE_INSTANCED_PROP(int, _Alignment)
UNITY_INSTANCING_BUFFER_END(Props)

#define ALIGNMENT_FLAT 0
#define ALIGNMENT_BILLBOARD 1

struct VertexInput {
	float4 vertex : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
	float4 pos : SV_POSITION;
	float tColor : TEXCOORD0;
	float dashCoord : TEXCOORD1;
	//#if defined(CAP_ROUND) || defined(CAP_SQUARE)
		float2 uv0 : TEXCOORD2;
		float capLengthRatio : TEXCOORD4;
	//#endif
	float pxCoverage : TEXCOORD5;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert(VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    float3 aLocal = UNITY_ACCESS_INSTANCED_PROP(Props, _PointStart);
    float3 bLocal = UNITY_ACCESS_INSTANCED_PROP(Props, _PointEnd);
    int alignment = UNITY_ACCESS_INSTANCED_PROP(Props, _Alignment);
    aLocal.z *= saturate(alignment); // flatten Z if _Alignment == ALIGNMENT_FLAT
    bLocal.z *= saturate(alignment);
	float3 a = LocalToWorldPos( aLocal );
	float3 b = LocalToWorldPos( bLocal );
	float3 vertOrigin = v.vertex.y < 0 ? a : b;

	float lineLengthVisual;
	float3 tangent;
	GetDirMag(b - a, /*out*/ tangent, /*out*/ lineLengthVisual);

    float3 normal = 0;
    switch( alignment ){
        case ALIGNMENT_FLAT:
            // todo: this feels kinda wonky why don't we do all of this in local space?
            float3 tangentLocal = mul( (float3x3)unity_WorldToObject, tangent );
            normal = mul( (float3x3)UNITY_MATRIX_M, Rotate90Right( tangentLocal ) );
        break;
        case ALIGNMENT_BILLBOARD:
            float3 camForward = -DirectionToNearPlanePos( vertOrigin );
            normal = normalize(cross(camForward, tangent));
        break;
    }

	// 2d only
	// 
	float thickness = UNITY_ACCESS_INSTANCED_PROP(Props, _Thickness);
	int thicknessSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _ThicknessSpace);
	
	LineWidthData widthData = GetScreenSpaceWidthData( vertOrigin, normal, thickness, thicknessSpace );
	o.uv0 = v.vertex;
	float verticalPaddingTotal = widthData.pxPadding / widthData.pxPerMeter;
	
	#if LOCAL_ANTI_ALIASING_QUALITY > 0
	    o.uv0.x *= widthData.aaPaddingScale; // scale compensate width
	    o.uv0.y *= (lineLengthVisual + verticalPaddingTotal ) / lineLengthVisual; // scale compensate height
	#endif
	
	    
	o.pxCoverage = widthData.thicknessPixelsTarget;
	float radiusVtx = widthData.thicknessMeters / 2;
	float radiusVisuals = 0.5*widthData.thicknessPixelsTarget / widthData.pxPerMeter;
	
	
	#if defined(CAP_ROUND) || defined(CAP_SQUARE)
	    float endToEndLength = lineLengthVisual + radiusVisuals * 2;
	#else
	    float endToEndLength = lineLengthVisual;
	#endif
	o.capLengthRatio = (2*radiusVisuals)/endToEndLength;

	float dashSize = UNITY_ACCESS_INSTANCED_PROP(Props, _DashSize);
	if( dashSize > 0 ){
	    float dashOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _DashOffset)*0.5;
	    float dashSpan = lineLengthVisual;
	    float extraCapOffset = 0;
        #if defined(CAP_ROUND) || defined(CAP_SQUARE)
            extraCapOffset += 1;
        #endif
        
        o.dashCoord = ((o.uv0.y+1)/o.capLengthRatio-extraCapOffset);
        o.dashCoord = o.dashCoord / (4*dashSize);
        o.dashCoord -= 0.25 + dashOffset;
	}

	

	
	#if defined(CAP_ROUND) || defined(CAP_SQUARE)
	    float3 vertPos = vertOrigin + (normal * v.vertex.x + tangent * v.vertex.y) * radiusVtx;
	#else
	    #if LOCAL_ANTI_ALIASING_QUALITY > 0
		    float3 vertPos = vertOrigin + normal * (v.vertex.x * radiusVtx) + tangent * (v.vertex.y * verticalPaddingTotal * 0.5);
		#else
		    float3 vertPos = vertOrigin + normal * v.vertex.x * radiusVtx;
		#endif
	#endif
	
	#if defined(CAP_ROUND) || defined(CAP_SQUARE)
	    float k = 2 * radiusVtx / lineLengthVisual + 1;
        float m = -radiusVtx / lineLengthVisual;
        o.tColor = k * (v.vertex.y/2+0.5) + m;
    #else
	    o.tColor = v.vertex.y/2+0.5;
	#endif

	o.pos = WorldToClipPos( vertPos.xyz );
	return o;
} 

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
 
	UNITY_SETUP_INSTANCE_ID(i);
	
	float4 colorStart = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
    float4 colorEnd = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorEnd);
	float4 shape_color = lerp(colorStart, colorEnd, saturate( i.tColor ) ); // lerp(i.coord);

	float shape_mask = 1;
	
	// edge masking
	#if LOCAL_ANTI_ALIASING_QUALITY > 0
	    float maskEdges = GetLineLocalAA( i.uv0.x, i.pxCoverage );
		shape_mask = min( shape_mask, maskEdges );
	#endif
	
    // cap masking
	#ifdef CAP_ROUND
		float2 uv = i.uv0.xy;
		uv = abs(uv);
		uv.y = (uv.y-1)/i.capLengthRatio + 1;
		float maskRound = StepAA(length(max(0,uv)),1);
		float useMaskRound = saturate(i.pxCoverage/2); // only use LineLocalAA when very thin
		shape_mask = min( shape_mask, lerp( 1, maskRound, useMaskRound)); 
	#else
	    // if cap == square or no caps, also do uv.y masking for caps
	    #if LOCAL_ANTI_ALIASING_QUALITY > 0
	        shape_mask = min( shape_mask, GetLineLocalAA( i.uv0.y, i.pxCoverage ) );
	    #endif
	#endif

	float dashSize = UNITY_ACCESS_INSTANCED_PROP(Props, _DashSize);
	if( dashSize > 0 ){
		float dashDdxy = fwidth(i.dashCoord * 4);
		float dashMask = saturate((abs(frac(i.dashCoord) * 2 - 1) * 2 - 1) / dashDdxy);
		shape_mask = min(shape_mask, dashMask);
	}
    
	shape_mask *= saturate( i.pxCoverage );
    
	return ShapesOutput( shape_color, shape_mask );
}