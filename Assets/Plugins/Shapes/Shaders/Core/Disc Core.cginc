// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_DEFINE_INSTANCED_PROP(float4, _ColorOuterStart)
UNITY_DEFINE_INSTANCED_PROP(float4, _ColorInnerEnd)
UNITY_DEFINE_INSTANCED_PROP(float4, _ColorOuterEnd)
UNITY_DEFINE_INSTANCED_PROP(float, _Radius)
UNITY_DEFINE_INSTANCED_PROP(int, _RadiusSpace)
UNITY_DEFINE_INSTANCED_PROP(float, _Thickness)
UNITY_DEFINE_INSTANCED_PROP(int, _ThicknessSpace)
UNITY_DEFINE_INSTANCED_PROP(float, _AngleStart)
UNITY_DEFINE_INSTANCED_PROP(float, _AngleEnd)
UNITY_DEFINE_INSTANCED_PROP(int, _RoundCaps)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 uv0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
    float4 pos : SV_POSITION;
    float2 uv0 : TEXCOORD0;
    float pxCoverage : TEXCOORD1;
    float innerRadiusFraction : TEXCOORD2;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

// I hate C
inline void ApplyRadialMask( inout half mask, VertexOutput i, out half tRadial );
inline void ApplyAngularMask( inout half mask, VertexOutput i, out half tAngular, out half2 coord, out half ang, out half angStart, out half angEnd, out bool useRoundCaps );
inline void ApplyEndCaps( inout half mask, VertexOutput i, half2 coord, half ang, half angStart, half angEnd, bool useRoundCaps );
inline half4 GetColor( half tRadial, half tAngular );

VertexOutput vert (VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	float radius = UNITY_ACCESS_INSTANCED_PROP(Props, _Radius);
	float radiusSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _RadiusSpace);
	float3 wPos = LocalToWorldPos( float3(0,0,0) ); // per vertex makes it real wonky so shrug~
    float3 camRight = CameraToWorldVec( float3(1,0,0) );
    LineWidthData widthDataRadius = GetScreenSpaceWidthDataSimple( wPos, camRight, radius*2, radiusSpace );
    o.pxCoverage = widthDataRadius.thicknessPixelsTarget;
    float radiusOuter = widthDataRadius.thicknessMeters / 2;
    
	#ifdef INNER_RADIUS
	    float thickness = UNITY_ACCESS_INSTANCED_PROP(Props, _Thickness);
	    float thicknessSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _ThicknessSpace);
	    LineWidthData widthDataThickness = GetScreenSpaceWidthDataSimple( wPos, camRight, thickness, thicknessSpace );
	    float thicknessRadius = widthDataThickness.thicknessMeters / 2;
	    o.pxCoverage = widthDataThickness.thicknessPixelsTarget;
	    radiusOuter += thicknessRadius;
	    o.innerRadiusFraction = (radiusOuter - thicknessRadius*2) / radiusOuter;
	#endif

	v.vertex.xy = v.uv0 * radiusOuter; // Fit radius
	o.uv0 = v.uv0;
    o.pos = UnityObjectToClipPos( v.vertex );

    return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i); 

    half tRadial, tAngular; // interpolators for radial & angular gradient
	half ang, angStart, angEnd;
	bool useRoundCaps;
	half2 coord; // coordinates used for end caps, if applicable
	
	half mask = 1;
	ApplyRadialMask( /*inout*/ mask, i, /*out*/ tRadial );
	ApplyAngularMask( /*inout*/ mask, i, /*out*/ tAngular, /*out*/ coord, /*out*/ ang, /*out*/ angStart, /*out*/ angEnd, /*out*/ useRoundCaps );
	ApplyEndCaps(/*inout*/ mask, i, coord, ang, angStart, angEnd, useRoundCaps);
	mask *= saturate(i.pxCoverage); // pixel fade
	
    half4 color = GetColor( tRadial, tAngular );    
	return ShapesOutput( color, mask );
}

inline void ApplyRadialMask( inout half mask, VertexOutput i, out half tRadial ){
    half len = length( i.uv0 );
    mask = min( mask, StepAA( len, 1 ) ); // outer radius
	#ifdef INNER_RADIUS
		mask = min( mask, 1.0-StepAA( len, i.innerRadiusFraction ) ); // inner radius
		tRadial = saturate(InverseLerp( i.innerRadiusFraction, 1, len ) );
	#else
	    tRadial = saturate(len);
	#endif
}

inline void ApplyAngularMask( inout half mask, VertexOutput i, out half tAngular, out half2 coord, out half ang, out half angStart, out half angEnd, out bool useRoundCaps ){

    #ifdef SECTOR
		angStart = UNITY_ACCESS_INSTANCED_PROP(Props, _AngleStart);
	    angEnd = UNITY_ACCESS_INSTANCED_PROP(Props, _AngleEnd);
	    // Rotate so that the -pi/pi seam is opposite of the visible segment
		// 0 is the center of the segment post-rotate
		half angOffset = -(angEnd + angStart) * 0.5;
	    coord = Rotate( i.uv0, angOffset );
	    angStart += angOffset;
	    angEnd += angOffset;
	#else
	    // required for angular gradients on rings and discs
		angStart = 0;
        angEnd = TAU;
        coord = -i.uv0;
	#endif
	
	ang = atan2( coord.y, coord.x ); // -pi to pi
	float sectorSize = abs(angEnd - angStart);
	tAngular = saturate(ang/sectorSize + 0.5); // angular interpolator for color
	#ifdef SECTOR
	
	    useRoundCaps = UNITY_ACCESS_INSTANCED_PROP(Props, _RoundCaps);
	        
	    float segmentMask;
		#if LOCAL_ANTI_ALIASING_QUALITY == 0
		    segmentMask = StepAA( abs(ang), sectorSize*0.5 );
		#else
		    // if arc
		    #ifdef INNER_RADIUS
		        if( useRoundCaps ){ // arcs with round caps hide the border anyway, so use cheap version
                    segmentMask = step( abs(ang), sectorSize*0.5 );
                } else {
		    #endif
		    
            float2 pdCoordSpace = float2( -coord.y, coord.x ) / dot( coord, coord );
            segmentMask = StepAAManualPD( coord, abs(ang), sectorSize*0.5, pdCoordSpace );
            
            // if arc
            #ifdef INNER_RADIUS
                } // this is a little cursed I know I'm sorry~
		    #endif
		    
		#endif
		
		// Adjust if close to 0 or TAU radians, fade in or out completely
		float THRESH_INVIS = 0.001;
		float THRESH_VIS = 0.002;
		float fadeInMask = saturate( InverseLerp( TAU - THRESH_VIS, TAU - THRESH_INVIS, sectorSize ) );
		float fadeOutMask = saturate( InverseLerp( THRESH_INVIS, THRESH_VIS, sectorSize ) );
		mask *= lerp( segmentMask * fadeOutMask, 1, fadeInMask );
	#else 
	    // SECTOR not defined
	    useRoundCaps = false;
	#endif

}

inline void ApplyEndCaps( inout half mask, VertexOutput i, half2 coord, half ang, half angStart, half angEnd, bool useRoundCaps ){
    #if defined(INNER_RADIUS) && defined(SECTOR)
        if( useRoundCaps ){
            half halfThickness = (1-i.innerRadiusFraction)/2;
            half distToCenterOfRing = 1-halfThickness;
            half angToA = abs( ang - angStart );
            half angToB = abs( ang - angEnd );
            half capAng = (angToA < angToB) ? angStart : angEnd;
            half2 capCenter = AngToDir(capAng) * distToCenterOfRing;
            half endCapMask = StepAA( length(coord - capCenter), halfThickness );
            mask = max(mask, endCapMask);
        }   
    #endif
}

inline half4 GetColor( half tRadial, half tAngular ){
    half4 colInnerStart = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
    half4 colOuterStart = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorOuterStart);
    half4 colInnerEnd = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorInnerEnd);
    half4 colOuterEnd = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorOuterEnd);
	half4 colorStart = lerp( colInnerStart, colOuterStart, tRadial );
	half4 colorEnd = lerp( colInnerEnd, colOuterEnd, tRadial );
	return lerp( colorStart, colorEnd, tAngular );
} 