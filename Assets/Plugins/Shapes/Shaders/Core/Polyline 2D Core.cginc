// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_DEFINE_INSTANCED_PROP(float4, _PointStart)
UNITY_DEFINE_INSTANCED_PROP(float4, _PointEnd)
UNITY_DEFINE_INSTANCED_PROP(float, _Thickness)
UNITY_DEFINE_INSTANCED_PROP(int, _ThicknessSpace)
UNITY_DEFINE_INSTANCED_PROP(float, _DashSize)
UNITY_DEFINE_INSTANCED_PROP(float, _DashOffset)
UNITY_DEFINE_INSTANCED_PROP(int, _Alignment)
UNITY_INSTANCING_BUFFER_END(Props)

#define ALIGNMENT_FLAT 0
#define ALIGNMENT_BILLBOARD 1
 
struct VertexInput {
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float4 vertex : POSITION; // current
    float4 uv0 : TEXCOORD0; // uvs (XY) endpoint (Z) thickness (W)
    float3 uv1 : TEXCOORD1; // prev
    float3 uv2 : TEXCOORD2; // next
    float4 color : COLOR;
};
struct VertexOutput {
    float4 pos : SV_POSITION;
    float4 color : TEXCOORD0;
    float pxCoverage : TEXCOORD1;
    float3 uv0 : TEXCOORD2;
    float radius : TEXCOORD3;
    #ifdef DASHED
        float dashCoord : TEXCOORD4;
    #endif
    #if defined(CAP_ROUND) || defined(CAP_SQUARE)
        float coordLongStart : TEXCOORD4;
        float coordLongEnd : TEXCOORD5;
    #endif
    #if defined(IS_JOIN_MESH) && defined(JOIN_ROUND)
        float3 worldPos : TEXCOORD6;
        float3 origin : TEXCOORD7;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


void GetMiterOffset( out float3 dir, out float len, float3 aNormal, float3 bNormal, float radius ) {
    float dotVal = dot(aNormal, bNormal);
    if (dotVal < -0.99) {
        dir = float3(0, 0, 0);
        len = 0;
    } else {
        dir = normalize( aNormal + bNormal );
        len = radius / max(0.0001,dot( dir, bNormal ));
    }
}
void GetSimpleOffset( out float3 dir, out float len, float3 aNormal, float3 bNormal, float radius ){
    float dotVal = dot(aNormal, bNormal);
    if (dotVal < -0.99) {
        dir = float3(0, 0, 0);
        len = 0;
    } else {
        dir = normalize( aNormal + bNormal );
        len = radius;
    }
}

VertexOutput vert (VertexInput v) {
    VertexOutput o = (VertexOutput)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    // alignment
    int alignment = UNITY_ACCESS_INSTANCED_PROP(Props, _Alignment);

    // points
    float3 ptWorld = 0;
    float3 pt = 0;
    float3 ptPrev = 0;
    float3 ptNext = 0;
    float3 dirToCam = 0;
    switch( alignment ){
        case ALIGNMENT_FLAT:
            // local space
            pt = float3( v.vertex.xy, 0 );
            ptWorld = LocalToWorldPos( pt );
            ptPrev = float3( v.uv1.xy, 0 );
            ptNext = float3( v.uv2.xy, 0 );
        break;
        case ALIGNMENT_BILLBOARD:
            pt = LocalToWorldPos( v.vertex.xyz );
            ptWorld = pt;
            ptPrev = LocalToWorldPos( v.uv1 );
            ptNext = LocalToWorldPos( v.uv2 );
            dirToCam = DirectionToNearPlanePos( pt );
        break;
    }
    

    // tangents & normals
    float3 tangentPrev = normalize( pt - ptPrev );
    float3 tangentNext = normalize( ptNext - pt );
    float3 normalPrev = 0;
    float3 normalNext = 0;
    switch( alignment ){
        case ALIGNMENT_FLAT:
            // local space
            normalPrev = float3( tangentPrev.y, -tangentPrev.x, 0 );
            normalNext = float3( tangentNext.y, -tangentNext.x, 0 );
        break;
        case ALIGNMENT_BILLBOARD:
            // world space
            normalPrev = normalize(cross( dirToCam, tangentPrev ));
            normalNext = normalize(cross( dirToCam, tangentNext ));
        break;
    }
    
    
    float turnDirection = sign(dot( tangentPrev, normalNext ));
    
    
    // thickness stuff
    float thickness = UNITY_ACCESS_INSTANCED_PROP(Props, _Thickness) * v.uv0.w; // global thickness * per-point thickness
    
    // radius calc
    int thicknessSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _ThicknessSpace);
	float3 camRight = CameraToWorldVec( float3( 1, 0, 0 ) );
	LineWidthData widthData = GetScreenSpaceWidthData( ptWorld, camRight, thickness, thicknessSpace );
	o.pxCoverage = widthData.thicknessPixelsTarget;
	float vertexRadius = widthData.thicknessMeters * 0.5;
	#if LOCAL_ANTI_ALIASING_QUALITY > 0
        float paddingWorldSpace = (0.5 * widthData.pxPadding / widthData.pxPerMeter);
        bool isEndpoint = abs(v.uv0.z) > 0;
        float endpointExtrude = isEndpoint ? paddingWorldSpace : 0;
    #else
        float endpointExtrude = 0;
    #endif
    o.radius = vertexRadius / widthData.aaPaddingScale; // actual radius of the visuals

    // miter point calc
    #if defined(IS_JOIN_MESH) // only draw joins
        float tSide = (-turnDirection*v.uv0.y + 1)/2;
        float tExtVsMiter = (v.uv0.x + 1)/2;
        float3 normalPrevFlip = turnDirection*normalPrev;
        float3 normalNextFlip = turnDirection*normalNext;
        float3 sideExtrudeDir = lerp( normalPrevFlip, normalNextFlip, tSide );
        float3 centerNormalDir = normalize( normalPrevFlip + normalNextFlip );
        float3 miterDir;
        float miterLength;
        GetMiterOffset( /*out*/ miterDir, /*out*/ miterLength, sideExtrudeDir, centerNormalDir, vertexRadius );
        float3 miterOffset = miterDir * miterLength;
        float3 vertOffset = lerp( miterOffset, sideExtrudeDir * vertexRadius, tExtVsMiter ) * abs(v.uv0.x);
        float3 vertPos = pt + vertOffset;
    #else // only draw line segments
    
        float3 miterDir;
        float miterLength;
        
        // all these boys use proper miters
        #if defined(JOIN_MITER) || defined(JOIN_ROUND) || defined(JOIN_BEVEL) 
            GetMiterOffset( /*out*/ miterDir, /*out*/ miterLength, normalPrev, normalNext, vertexRadius );
            // make sure miter length doesn't overshoot line length
            // or not you know because it turns out~ this is more complicated than this
            // (because it changes thickness) so, uh, nevermind for now
            // miterLength = lerp(miterLength, min(miterLength, min(length(pt - ptPrev),length(ptNext - pt)) ),(-v.uv0.x*turnDirection + 1)/2 );
            //float3 midpt = (ptNext + ptPrev)/2;
            //miterLength = min(miterLength, min( distance( pt, ptNext ), distance( pt, ptPrev ) ) );
        #else
            // simple joins
            GetSimpleOffset( /*out*/ miterDir, /*out*/ miterLength, normalPrev, normalNext, vertexRadius );
        #endif
        
        
        float3 scaledMiterNormal = miterDir * miterLength;
        #if defined(JOIN_ROUND) || defined(JOIN_BEVEL)
            float3 extrude = lerp(normalPrev, normalNext, (v.uv0.y + 1)/2 ) * vertexRadius;
            extrude = lerp( extrude, scaledMiterNormal, (-v.uv0.x*turnDirection + 1)/2 );
            float3 vertPos = pt + extrude * v.uv0.x;
        #else
            float3 vertPos = pt + scaledMiterNormal * v.uv0.x; // float3(v.tangent.xy * v.uv0.x * vertexRadius,0);
        #endif
        #if LOCAL_ANTI_ALIASING_QUALITY > 0
            vertPos += tangentPrev * endpointExtrude * v.uv0.z; // is 0 if not an endpoint
            float distToPrev = distance(pt, ptPrev);
            v.uv0.z *= (distToPrev + endpointExtrude)/(distToPrev); // ratio for extrude dist compensation 
        #endif
    #endif

    #if defined(IS_JOIN_MESH) && defined(JOIN_ROUND)
        o.worldPos = vertPos;
        o.origin = pt;
    #endif

    o.color = GammaCorrectVertexColorsIfNeeded(v.color) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

    o.uv0 = v.uv0;
    o.uv0.x *= widthData.aaPaddingScale; // scale compensate for fading

    //float depth = unity_ObjectToWorld[2][3];
    switch( alignment ){
        case ALIGNMENT_FLAT:
            // world space
            o.pos = LocalToClipPos( float4( vertPos, 1 ) );
        break;
        case ALIGNMENT_BILLBOARD:
            o.pos = WorldToClipPos( float4( vertPos, 1 ) );
        
            /*// view space
            vertPos.z = originalClipSpacePos.z;
            o.pos = float4( vertPos, originalClipSpacePos.w );*/
        break;
    }
    
    return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(i);
    
    // debug padding
    // float2 paddingDebug = abs(float2( i.uv0.x, i.uv0.z )) >= 1 ? 1 : 0;
    //return ShapesOutput( float4(uv,0,1), shape_mask );
    
    float shape_mask = 1;
    
    // Round joins
    #if defined(IS_JOIN_MESH) && defined(JOIN_ROUND)
        shape_mask = SdfToMask( distance( i.worldPos, i.origin ) / i.radius, 1 );
    #endif
        
	// used for line segments and bevel joins
	#if LOCAL_ANTI_ALIASING_QUALITY > 0 && ( defined(IS_JOIN_MESH) == false || (defined(IS_JOIN_MESH) && defined(JOIN_BEVEL)) )
        float maskEdges = GetLineLocalAA( i.uv0.x, i.pxCoverage );
        float maskEdgesCap = GetLineLocalAA( i.uv0.z, i.pxCoverage );
        shape_mask = min( shape_mask, min( maskEdges, maskEdgesCap ) );
    #endif
    
    
    
    shape_mask *= saturate( i.pxCoverage );
    
    return ShapesOutput( i.color, shape_mask );
    
}