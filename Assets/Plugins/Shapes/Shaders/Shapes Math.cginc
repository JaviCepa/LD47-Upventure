// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

// constants
#define TAU 6.28318530718
#define VERY_SMOL 0.000001

#define THICKN_SPACE_WORLD 0
#define THICKN_SPACE_PIXELS 1
#define THICKN_SPACE_NOOTS 2


// remap functions
inline float InverseLerp( float a, float b, float v ) {
	return (v - a) / (b - a);
}
inline float2 InverseLerp( float2 a, float2 b, float2 v ) {
	return (v - a) / (b - a);
}
inline float3 InverseLerp( float3 a, float3 b, float v ) {
	return (v - a) / (b - a);
}
float2 Remap( float2 iMin, float2 iMax, float2 oMin, float2 oMax, float2 v ) {
	float2 t = InverseLerp( iMin, iMax, v );
	return lerp( oMin, oMax, t );
}
inline float Round( float a, float divs ){
	return round(a*divs)/divs;
}

// vector utils
float Determinant( in float2 a, in float2 b ) {
    return a.x * b.y - a.y * b.x;
}
float2 Rotate( float2 v, float ang ){
	float2 a = float2( cos(ang), sin(ang) );
	return float2(
		a.x * v.x - a.y * v.y,
		a.y * v.x + a.x * v.y
	);
}
inline half2 AngToDir( half ang ){
    return half2(cos(ang),sin(ang));
}
inline float2 Rotate90Left( in float2 v ){
	return float2( -v.y, v.x );
}
inline float2 Rotate90Right( in float2 v ){
	return float2( v.y, -v.x );
}
inline float3 Rotate90Left( in float3 v ){
	return float3( -v.y, v.x, 0 );
}
inline float3 Rotate90Right( in float3 v ){
	return float3( v.y, -v.x, 0 );
}
void GetDirMag( in float2 v, out float2 dir, out float mag ){
	mag = length( v );
	dir = v / mag; // Normalize
}
void GetDirMag( in float3 v, out float3 dir, out float mag ){
	mag = length( v );
	dir = v / mag; // Normalize
}

// color/value utils
inline float4 GammaCorrectVertexColorsIfNeeded( in float4 color ){
    #ifdef UNITY_COLORSPACE_GAMMA
        return color;
    #else
        return float4( GammaToLinearSpace(color.rgb), color.a ); 
    #endif
}
float PxNoise( float2 uv ){
    float2 s = uv + 0.2127+uv.x*0.3713*uv.y;
    float2 r = 4.789*sin(489.123*s);
    return frac(r.x*r.y*(1+s.x));
}



// modified version of http://iquilezles.org/www/articles/ibilinear/ibilinear.htm
float2 InvBilinear( in float2 p, in float2 a, in float2 b, in float2 c, in float2 d ) {
    float2 res = float2(-1, -1);

    float2 e = d-a;
    float2 f = b-a;
    float2 g = a-d+c-b;
    float2 h = p-a;
        
    float k2 = Determinant( g, f );
    float k1 = Determinant( e, f ) + Determinant( h, g );
    float k0 = Determinant( h, e );
    
    // if edges are parallel, this is a linear equation. Do not this test here though, do
    // it in the user code
    if( abs(k2)<0.001 ) {
        float v = -k0/k1;
        float u  = (h.x*k1+f.x*k0) / ((e.x*k1-g.x*k0));
        /*if( v>0.0 && v<1.0 && u>0.0 && u<1.0 )*/  res = float2( u, v );
    } else {
        // otherwise, it's a quadratic
        float w = sqrt(max(0, k1*k1 - 4.0*k0*k2));

        float ik2 = 0.5/k2;
        float v = (-k1 - w)*ik2;
        if( v<0.0 || v>1.0 )
            v = (-k1 + w)*ik2;
        float u = (h.x - f.x*v)/(e.x + g.x*v);
        res = saturate(float2( u, v ));
    }
    
    return res;
}

// partial derivative shenanigans
#if LOCAL_ANTI_ALIASING_QUALITY != 0
inline float PD( float value ){
	#if LOCAL_ANTI_ALIASING_QUALITY == 2
		float2 pd = float2(ddx(value), ddy(value));
		return sqrt( dot( pd, pd ) );
	#endif
	#if LOCAL_ANTI_ALIASING_QUALITY == 1
		return fwidth( value );
	#endif
}
#endif

inline float StepThresholdPD( float value, float pd ) {
    return saturate( value / max( 0.00001, pd ) + 0.5 ); // sooooooooooooo this is complicated, whether it should be +0 or +.5
}
inline float StepThresholdPDAAOffset( float value, float pd, float aaOffset ) {
    return saturate( value / max( 0.00001, pd ) + aaOffset );
}
inline float StepAA( float value ) {
    #if LOCAL_ANTI_ALIASING_QUALITY == 0
        return step(0,value);
    #else
        return StepThresholdPD( value, PD( value ) );
	#endif
}



inline float StepAA( float thresh, float value ){
	return StepAA( value - thresh );
}

inline float SdfToMask( float value, float thresh ){
    float sdf = value - thresh;
    return 1-StepAA( sdf );
}

#if LOCAL_ANTI_ALIASING_QUALITY != 0
float StepAAPdThresh( float thresh, float value ){
    float pd = PD(thresh);
    value = value * pd;
    return StepAA( thresh, value );
}
float StepAAManualPD( float2 coords, float sdf, float thresh, float2 pdCoordSpace ){
	float2 pdScreenSpace = pdCoordSpace * PD( coords ); // Transform uv to screen space (does not support rotations, I think)
	float pdMag = length( pdScreenSpace ); // Get the magnitude of change
	float sub = sdf - thresh;
	return 1.0-saturate( sub / pdMag );
}
inline float GetLineLocalAA( float coord, float pxCoverage ){
    float ddxy = PD(coord);
    float sdf = abs(coord)-1;
    float aaOffset = saturate(InverseLerp( 0, 1.1, pxCoverage )) * 0.5; // the 1.1 here is very much a hack but it looks good okay THIN LINES ARE HARD
    return 1.0-StepThresholdPDAAOffset( sdf, ddxy, aaOffset );
}
#endif

inline float2 GetDir( float angleRad ) {
	return float2(cos(angleRad), sin(angleRad));
}

// sdfs
inline float SdfBox( float2 coord, float2 size ) {
    float2 q = abs(coord) - size;
    return length(max(0,q)) + min(0,max(q.x,q.y));
}

// smoothing and tweening
inline float Smooth( float x ) { // Cubic
	return x * x * (3.0 - 2.0 * x);
}
inline float Smooth2( float x ) { // Quartic
	return x * x * x * (x * (x * 6.0 - 15.0) + 10.0);
}
inline float EaseOutBack( float x, float p ){
	return lerp( x, pow( x, p )+p*(1-x), x );
}
inline float EaseOutBack2( float x ){
	return x * (3 + x*(x-3) );
}
inline float EaseOutBack3( float x ){
	return x * (4 + x*(x*x-4) );
}
inline float EaseOutBack4( float x ){
	return x * (5 + x*( x*x*x-5 ) );
}
inline float EaseOutBack5( float x ){
	return x * (6 + x*( x*x*x*x-6 ) );
}

#ifdef SCENE_VIEW_PICKING
    // long story short - when picking in the scene view, it does so by rendering a tiny 4x4 RT
    // but it does not write to _ScreenParams, so any pixel-sized objects get very incorrect values
    #define SCREEN_PARAMS float4( 4, 4, 1.25, 1.25 )
#else
    #define SCREEN_PARAMS _ScreenParams
#endif 

// space utils
inline float4 WorldToClipPos( in float3 worldPos ) {
    return mul( UNITY_MATRIX_VP, float4( worldPos, 1 ) );
}
inline float4 ViewToClipPos( in float3 viewPos ) {
    return mul( UNITY_MATRIX_P, float4( viewPos, 1 ) );
}
inline float4 LocalToClipPos( in float3 localPos ) {
    return UnityObjectToClipPos( float4( localPos, 1 ) );
}
inline float3 LocalToWorldPos( in float3 localPos ){
    return mul( UNITY_MATRIX_M, float4( localPos, 1 )).xyz; 
}
/*inline float3 LocalToViewPos( in float3 localPos ){
    return UnityObjectToViewPos( localPos ); // mul( UNITY_MATRIX_MV, float4( localPos, 1 )).xyz; 
}
inline float3 LocalToViewVec( in float3 localVec ){
    return mul( (float3x3)UNITY_MATRIX_MV, localVec ).xyz; // Unity stop warning about this pls this is valid :c
}*/ 
inline float3 LocalToWorldVec( in float3 localVec ){
    return mul( (float3x3)UNITY_MATRIX_M, localVec ); 
}
inline float3 CameraToWorldVec( float3 camVec ){
    return mul( (float3x3)unity_CameraToWorld, camVec );
}
float2 WorldToScreenSpace( float3 worldPos ){
    float4 clipSpace = UnityObjectToClipPos( float4( worldPos, 1 ) );
    float2 normalizedScreenspace = clipSpace.xy / clipSpace.w;
    return 0.5*(normalizedScreenspace+1.0) * SCREEN_PARAMS.xy;
}
float2 WorldToScreenSpaceNormalized( float3 worldPos ){
    float4 clipSpace = WorldToClipPos( worldPos );
    return clipSpace.xy / clipSpace.w;
}

float WorldToPixelDistance( float3 worldPosA, float3 worldPosB ){
    float2 pxNrmA = WorldToScreenSpaceNormalized( worldPosA );
	float2 pxNrmB = WorldToScreenSpaceNormalized( worldPosB );
	float2 diff = (pxNrmA - pxNrmB) * SCREEN_PARAMS.xy;
	return length( diff )*0.5;
}
inline float NootsToPixels( in float noots ){
    return min( _ScreenParams.x, _ScreenParams.y ) * ( noots / NOOTS_ACROSS_SCREEN );
}
inline float PixelsToNoots( in float pixels ){
    return (NOOTS_ACROSS_SCREEN * pixels) / min( _ScreenParams.x, _ScreenParams.y );
}
inline float3 GetCameraForwardDirection(){
    return CameraToWorldVec( float3(0,0,1) );
}

inline bool IsOrthographic(){
    return unity_OrthoParams.w == 1;
}

inline float3 DirectionToNearPlanePos( float3 pt ){
    if( IsOrthographic() ){
        return -GetCameraForwardDirection();
    } else {
        return normalize( _WorldSpaceCameraPos - pt );
    }
}

// line utils
inline void ConvertToPixelThickness( float3 vertOrigin, float3 normal, float thickness, int thicknessSpace, out float pxPerMeter, out float pxWidth ){

    // calculate pixels per meter
	pxPerMeter = WorldToPixelDistance( vertOrigin, vertOrigin + normal ); // 1 unit in world space
	
	// figure out target width in pixels
	switch( thicknessSpace ){
	    case THICKN_SPACE_WORLD:
	        pxWidth = thickness*pxPerMeter; // this specifically should not have the + extraWidth
	        break;
	    case THICKN_SPACE_PIXELS:
	        pxWidth = thickness;
	        break;
        case THICKN_SPACE_NOOTS:
            pxWidth = NootsToPixels( thickness );
            break;
        default:
            pxWidth = 0;
            break;
    }
}



struct LineWidthData{
    half thicknessPixelsTarget; // 1 when thicker than 1 px, px thickness when smaller
    half thicknessMeters; // vertex position thickness. includes LAA padding
    half aaPaddingScale; // multiplier used to correct UVs for LAA padding
    half pxPerMeter; // might be useful idk
    half pxPadding;
};

inline void GetPaddingData( float thicknessPixelsTarget, out half pxPadding, out float aaPaddingScale, out float pxWidthVert ){
	#if LOCAL_ANTI_ALIASING_QUALITY == 0
        pxPadding = 0;
    #else
        pxPadding = 2;
    #endif
    // for vertex width, we need to clamp at 1px wide to prevent wandering ants and we don't want ants now do we
    pxWidthVert = max( 1, thicknessPixelsTarget+pxPadding );
    aaPaddingScale = pxWidthVert / max( VERY_SMOL, thicknessPixelsTarget ); // how much extra we got from the padding, as a multiplier
}


inline LineWidthData GetScreenSpaceWidthData( float3 vertOrigin, float3 normal, float thickness, int thicknessSpace ){
    LineWidthData data;
    ConvertToPixelThickness( vertOrigin, normal, thickness, thicknessSpace, /*out*/ data.pxPerMeter, /*out*/ data.thicknessPixelsTarget );
	
	float pxWidthVert;
	GetPaddingData( data.thicknessPixelsTarget, /*out*/ data.pxPadding, /*out*/ data.aaPaddingScale, /*out*/ pxWidthVert );
	
	// when using pixel size, scale to match pixels
	data.thicknessMeters = pxWidthVert / data.pxPerMeter; // clamps at 1px wide, then converts to meters
    
    return data;
}

inline LineWidthData GetScreenSpaceWidthDataSimple( float3 vertOrigin, float3 normal, float thickness, int thicknessSpace ){
    LineWidthData data;
    ConvertToPixelThickness( vertOrigin, normal, thickness, thicknessSpace, /*out*/ data.pxPerMeter, /*out*/ data.thicknessPixelsTarget );
    float pxWidthVert = max( 1, data.thicknessPixelsTarget );
    data.aaPaddingScale = 1; 
	data.thicknessMeters = pxWidthVert / data.pxPerMeter; // clamps at 1px wide, then converts to meters
    return data;
}



