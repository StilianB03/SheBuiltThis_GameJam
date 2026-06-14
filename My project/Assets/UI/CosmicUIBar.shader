Shader "Custom/CosmicUIBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ColorLeft ("Left Tint (Neon)", Color) = (0.2, 0.8, 1, 1)
        _ColorRight ("Right Tint (Dark)", Color) = (0.05, 0.1, 0.3, 1)
        _PulseSpeed ("Cosmic Pulse Speed", Float) = 3.0
        _PulseIntensity ("Pulse Intensity", Range(0, 0.5)) = 0.2
        
        // Required Unity UI Boilerplate
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma shader_feature __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _ColorLeft;
            fixed4 _ColorRight;
            float _PulseSpeed;
            float _PulseIntensity;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. Procedural Left-to-Right Gradient Map
                fixed4 gradient = lerp(_ColorLeft, _ColorRight, IN.texcoord.x);
                
                // 2. High-frequency cosmic energy wave waving across the X axis
                float wave = sin((IN.texcoord.x * 6.0) - (_Time.y * _PulseSpeed)) * _PulseIntensity;
                gradient.rgb += wave;

                // 3. Keep standard Image component tinting & native Filled Bar capabilities intact
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                fixed4 finalColor = gradient * IN.color;
                finalColor.a *= texColor.a;

                #ifdef UNITY_UI_ALPHACLIP
                clip (finalColor.a - 0.001);
                #endif

                return finalColor;
            }
        ENDCG
        }
    }
}