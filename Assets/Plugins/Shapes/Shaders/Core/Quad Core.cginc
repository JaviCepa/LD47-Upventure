// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP( float4, _Color)
UNITY_DEFINE_INSTANCED_PROP( float4, _ColorB)
UNITY_DEFINE_INSTANCED_PROP( float4, _ColorC)
UNITY_DEFINE_INSTANCED_PROP( float4, _ColorD)
UNITY_DEFINE_INSTANCED_PROP( float3, _A)
UNITY_DEFINE_INSTANCED_PROP( float3, _B)
UNITY_DEFINE_INSTANCED_PROP( float3, _C)
UNITY_DEFINE_INSTANCED_PROP( float3, _D)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
    float4 vertex : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
    float4 pos : SV_POSITION;
    #if QUAD_INTERPOLATION_QUALITY == 0
        float4 color : TEXCOORD0;
    #elif QUAD_INTERPOLATION_QUALITY == 1
        float2 uv : TEXCOORD2;
    #elif QUAD_INTERPOLATION_QUALITY == 2
        float2 localPos : TEXCOORD1;
    #elif QUAD_INTERPOLATION_QUALITY == 3
        float2 localPos : TEXCOORD1;
        float2 posA : TEXCOORD2;
        float2 posB : TEXCOORD3;
        float2 posC : TEXCOORD4;
        float2 posD : TEXCOORD5;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

float2 PlanarProject( float3 pos3D, float3 dirX, float3 dirY ){
    return float2( dot( pos3D, dirX ), dot( pos3D, dirY ) );
}

VertexOutput vert (VertexInput v) {
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    float3 A = UNITY_ACCESS_INSTANCED_PROP(Props, _A);
    float3 B = UNITY_ACCESS_INSTANCED_PROP(Props, _B);
    float3 C = UNITY_ACCESS_INSTANCED_PROP(Props, _C);
    float3 D = UNITY_ACCESS_INSTANCED_PROP(Props, _D);
    v.vertex.xyz = A * v.color.r + B * v.color.g + C * v.color.b + D * v.color.a;
    
    #if QUAD_INTERPOLATION_QUALITY == 0
        // per-vertex version, which doesn't do a real bilinear version
        float4 colorA = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
        float4 colorB = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorB);
        float4 colorC = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorC);
        float4 colorD = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorD);
        o.color = colorA * v.color.r + colorB * v.color.g + colorC * v.color.b + colorD * v.color.a;
    #elif QUAD_INTERPOLATION_QUALITY == 1
        o.uv = v.uv * 0.5 + 0.5;
    #elif QUAD_INTERPOLATION_QUALITY == 2
        o.localPos = v.vertex.xy; // assume 2D coordinates only
    #elif QUAD_INTERPOLATION_QUALITY == 3
        // construct best fit plane from average face normals
        float3 ab = B - A;
        float3 ac = C - A;
        float3 ad = D - A;
        float3 normalA = normalize(cross( ab, ac ));
        float3 normalB = normalize(cross( ab, ac ));
        float3 normal = normalize(normalA + normalB);
        float3 tangent = normalize(ab);
        float3 bitangent = cross( normal, tangent );
        
        // project all coordinates
        o.posA = PlanarProject( A, tangent, bitangent );
        o.posB = PlanarProject( B, tangent, bitangent );
        o.posC = PlanarProject( C, tangent, bitangent );
        o.posD = PlanarProject( D, tangent, bitangent );
        o.localPos = PlanarProject( v.vertex, tangent, bitangent );
    #endif
    
    o.pos = UnityObjectToClipPos( v.vertex );
    return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(i);
    #if QUAD_INTERPOLATION_QUALITY == 0
        return ShapesOutput( i.color, 1 );
    #else
        #if QUAD_INTERPOLATION_QUALITY == 1
            float2 uv = i.uv;
        #elif QUAD_INTERPOLATION_QUALITY == 2
            float3 A = UNITY_ACCESS_INSTANCED_PROP(Props, _A);
            float3 B = UNITY_ACCESS_INSTANCED_PROP(Props, _B);
            float3 C = UNITY_ACCESS_INSTANCED_PROP(Props, _C);
            float3 D = UNITY_ACCESS_INSTANCED_PROP(Props, _D);
            float2 uv = InvBilinear( i.localPos.xy, A.xy, B.xy, C.xy, D.xy );
        #elif QUAD_INTERPOLATION_QUALITY == 3
            float2 uv = InvBilinear( i.localPos.xy, i.posA, i.posB, i.posC, i.posD );
        #endif
        float4 colorA = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
        float4 colorB = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorB);
        float4 colorC = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorC);
        float4 colorD = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorD);
        float4 left = lerp(colorA, colorB, uv.y );
        float4 right = lerp(colorD, colorC, uv.y );
        float4 color = lerp( left, right, uv.x );
        return ShapesOutput( color, 1 );
    #endif
}



