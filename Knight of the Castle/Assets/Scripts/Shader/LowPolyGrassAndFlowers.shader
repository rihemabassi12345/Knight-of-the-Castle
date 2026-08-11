Shader "Custom/LowPolyGrassAndFlowers"
{
    Properties
    {
        _MainTex ("Base Texture (Alpha for Flowers/Blades)", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff Threshold", Range(0, 1)) = 0.5

        [Header(Season and Base Colors)]
        _GrassBaseColor ("Grass Base/Tip Tint", Color) = (0.4, 0.8, 0.2, 1)
        _FlowerTint ("Flower Accent Tint", Color) = (1.0, 0.3, 0.5, 1)

        [Header(Wind Sway Settings)]
        _WindSpeed ("Wind Speed", Float) = 2.5
        _WindStrength ("Wind Strength", Float) = 0.15
        _WindFrequency ("Wind Turbulence", Float) = 1.0

        [Header(Season Transition)]
        _SeasonProgress ("Season Progress (0..4)", Range(0, 4)) = 0.0
    }
    SubShader
    {
        Tags { 
            "RenderType"="Opaque" 
            "Queue"="AlphaTest" 
            "RenderPipeline"="UniversalPipeline" 
        }

        // Render both sides of grass blades like Blender
        Cull Off 

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _GrassBaseColor;
                float4 _FlowerTint;
                float _Cutoff;
                float _WindSpeed;
                float _WindStrength;
                float _WindFrequency;
                float _SeasonProgress;
            CBUFFER_END

            float Hash(float3 p)
            {
                return frac(sin(dot(p, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);

                // Wind sway calculation (Root stays fixed, tips sway in wind)
                float windMask = saturate(input.positionOS.y); // Height factor from mesh base
                float wave = sin(_Time.y * _WindSpeed + (worldPos.x + worldPos.z) * _WindFrequency);
                float sway = wave * _WindStrength * windMask;

                worldPos.x += sway;
                worldPos.z += sway * 0.5;

                output.positionCS = TransformWorldToHClip(worldPos);
                output.worldNormal = TransformObjectToWorldNormal(input.normalOS);
                output.worldPos = worldPos;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Alpha clipping for blade/flower mesh outlines
                clip(texColor.a - _Cutoff);

                // Check if pixel belongs to a flower (Red channel or dedicated UV mask)
                float isFlower = texColor.r > 0.8 && texColor.g < 0.3;

                // Per-instance random color variations based on world position
                float rnd = Hash(floor(input.worldPos * 1.5));
                float3 grassColor = _GrassBaseColor.rgb + (rnd - 0.5) * 0.1;
                float3 flowerColor = _FlowerTint.rgb + (rnd - 0.5) * 0.2;

                float3 finalAlbedo = lerp(grassColor, flowerColor, isFlower ? 1.0 : 0.0) * texColor.rgb;

                // Lighting calculation (URP Directional Light)
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalize(input.worldNormal), mainLight.direction));
                float3 lighting = mainLight.color * (NdotL * 0.7 + 0.3); // Soft stylized shadow

                return float4(finalAlbedo * lighting, 1.0);
            }
            ENDHLSL
        }
    }
}