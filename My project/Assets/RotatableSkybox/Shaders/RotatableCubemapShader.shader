Shader "Skybox/Rotatable"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _QuaternionRotation("Rotation", Vector) = (0, 0, 0, 1) // Quaternion rotation
        [NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}
    }
    
    SubShader {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
    
        Pass {
    
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
    
            #include "UnityCG.cginc"
    
            samplerCUBE _Tex;
            half4 _Tex_HDR;
            half4 _Tint;
            half _Exposure;
            float4 _QuaternionRotation;
                    
            struct appdata_t {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
    
            struct v2f {
                float4 vertex : SV_POSITION;
                float3 cubemapDirection : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
    
            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Transform vertex to clip space
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Get the world direction of the vertex (skybox is centered on the camera)
                float3 worldDir = normalize(v.vertex.xyz);

                // Convert quaternion to a rotation matrix
                float3x3 rotationMatrix = float3x3(
                    1 - 2 * (_QuaternionRotation.y * _QuaternionRotation.y + _QuaternionRotation.z * _QuaternionRotation.z),
                    2 * (_QuaternionRotation.x * _QuaternionRotation.y - _QuaternionRotation.z * _QuaternionRotation.w),
                    2 * (_QuaternionRotation.x * _QuaternionRotation.z + _QuaternionRotation.y * _QuaternionRotation.w),

                    2 * (_QuaternionRotation.x * _QuaternionRotation.y + _QuaternionRotation.z * _QuaternionRotation.w),
                    1 - 2 * (_QuaternionRotation.x * _QuaternionRotation.x + _QuaternionRotation.z * _QuaternionRotation.z),
                    2 * (_QuaternionRotation.y * _QuaternionRotation.z - _QuaternionRotation.x * _QuaternionRotation.w),

                    2 * (_QuaternionRotation.x * _QuaternionRotation.z - _QuaternionRotation.y * _QuaternionRotation.w),
                    2 * (_QuaternionRotation.y * _QuaternionRotation.z + _QuaternionRotation.x * _QuaternionRotation.w),
                    1 - 2 * (_QuaternionRotation.x * _QuaternionRotation.x + _QuaternionRotation.y * _QuaternionRotation.y)
                );

                // Apply the rotation to the direction
                o.cubemapDirection = mul(rotationMatrix, worldDir);

                return o;
            }
    
            fixed4 frag (v2f i) : SV_Target
            {
                half4 tex = texCUBE (_Tex, i.cubemapDirection);
                half3 c = DecodeHDR (tex, _Tex_HDR);
                c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                c *= _Exposure;
                return half4(c, 1);
            }
            ENDCG
        }
    }

    Fallback "Skybox/Cubemap"
}