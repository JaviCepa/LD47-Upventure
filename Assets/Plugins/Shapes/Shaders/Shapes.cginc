// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "Shapes Config.cginc"
#include "Shapes Math.cginc"

// parameters for selection outlines
#ifdef SCENE_VIEW_OUTLINE_MASK
	int _ObjectId;
	int _PassValue;
#endif
#ifdef SCENE_VIEW_PICKING
	uniform float4 _SelectionID;
#endif

// used for the final output. supports branching based on opaque vs transparent and outline functions
inline float4 ShapesOutput( float4 shape_color, float shape_mask ){
    float4 outColor = float4(shape_color.rgb, shape_mask * shape_color.a);
    
    clip(outColor.a - VERY_SMOL);
    
    #ifdef ADDITIVE
        outColor.rgb *= outColor.a; // additive fade base on alpha
    #endif
    #ifdef MULTIPLICATIVE
        outColor.rgb = 1 + outColor.a * ( outColor.rgb - 1 ); // lerp(1,b,t) = 1 + t(b - 1);
    #endif
    
    #if defined(SCENE_VIEW_OUTLINE_MASK) || defined(SCENE_VIEW_PICKING)
        clip(shape_mask - 0.5 + VERY_SMOL); // Don't take color into account
    #endif 
    
    #if defined( SCENE_VIEW_OUTLINE_MASK )
        return float4(_ObjectId, _PassValue, 1, 1);
    #elif defined( SCENE_VIEW_PICKING )
        return _SelectionID;
    #else
        return outColor; // Render mesh
    #endif
}