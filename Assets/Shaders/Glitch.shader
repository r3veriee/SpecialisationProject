Shader "Hidden/Custom/Glitch"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Digital Glitch"
            ZTest Always ZWrite Off Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float _Intensity;
            float _ScanlineJitter;
            float _ColorDrift;
            float _TimeX;

            struct appdata { uint vertexID : SV_VertexID; };
            struct v2f { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f Vert(appdata input) {
                v2f o;
                float2 uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
                o.uv = uv;
                o.positionHCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                o.uv.y = 1.0 - o.uv.y;
                #endif
                return o;
            }

            float random(float2 uv) {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            half4 Frag(v2f i) : SV_Target {
                float2 uv = i.uv;
                
                // Jitter (Horizontal tearing)
                float jitter = random(float2(0, floor(uv.y * _ScanlineJitter) + _TimeX));
                if(jitter < _Intensity) uv.x += (random(float2(_TimeX, uv.y)) - 0.5) * 0.05 * _Intensity;

                // Color Drift (RGB Split)
                float drift = _ColorDrift * _Intensity * 0.02;
                float r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + float2(drift, 0)).r;
                float g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv).g;
                float b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv - float2(drift, 0)).b;

                return float4(r, g, b, 1.0);
            }
            ENDHLSL
        }
    }
}