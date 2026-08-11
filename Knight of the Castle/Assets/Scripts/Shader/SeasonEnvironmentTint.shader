Shader "Custom/LowPolyGround"
{
    Properties
    {
        _MainTex ("Base Ground Texture", 2D) = "white" {}
        
        [Header(Season Tints)]
        _SpringGround ("Spring Ground Tint", Color) = (0.55, 0.85, 0.35, 1)
        _SummerGround ("Summer Ground Tint", Color) = (0.2, 0.7, 0.2, 1)
        _AutumnGround ("Autumn Ground Tint", Color) = (0.7, 0.45, 0.15, 1)
        _WinterGround ("Winter Ground Tint", Color) = (0.8, 0.85, 0.9, 1)
        
        [Header(Snow Settings)]
        _SnowColor ("Snow Color", Color) = (0.95, 0.98, 1.0, 1)
        _SnowAmount ("Snow Intensity", Range(0, 1)) = 0.0

        [Header(Season State)]
        _SeasonProgress ("Season Progress (0 to 4)", Range(0, 4)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
                float4 _SpringGround;
                float4 _SummerGround;
                float4 _AutumnGround;
                float4 _WinterGround;
                float4 _SnowColor;
                float _SnowAmount;
                float _SeasonProgress;
            CBUFFER_END

            float4 GetGroundSeasonColor(float progress)
            {
                float p = frac(progress / 4.0) * 4.0;
                if (p < 1.0) 
                    return lerp(_SpringGround, _SummerGround, p);
                else if (p < 2.0) 
                    return lerp(_SummerGround, _AutumnGround, p - 1.0);
                else if (p < 3.0) 
                    return lerp(_AutumnGround, _WinterGround, p - 2.0);
                else 
                    return lerp(_WinterGround, _SpringGround, p - 3.0);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(worldPos);
                output.worldNormal = TransformObjectToWorldNormal(input.normalOS);
                output.worldPos = worldPos;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // 1. Sample base texture and calculate seasonal ground tint
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float4 seasonTint = GetGroundSeasonColor(_SeasonProgress);
                
                // Combine texture with season color
                float3 baseGroundColor = texColor.rgb * seasonTint.rgb;

                // 2. Snow Mask Calculation (facing UP toward the sky)
                float3 norm = normalize(input.worldNormal);
                float upDot = saturate(dot(norm, float3(0, 1, 0))); // 1.0 on flat ground, 0.0 on vertical walls
                
                // Auto calculate snow amount during winter phase (around _SeasonProgress = 3.0)
                float autoWinterSnow = saturate(1.0 - abs(_SeasonProgress - 3.0));
                float totalSnowFactor = max(_SnowAmount, autoWinterSnow);

                float snowMask = pow(upDot, 1.5) * totalSnowFactor;

                // Blend ground color to pure bright snow color (prevents black texture bug)
                float3 finalColor = lerp(baseGroundColor, _SnowColor.rgb, snowMask);

                // 3. Lighting calculation with ambient fallback
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(norm, mainLight.direction));
                
                // Ambient lighting fallback (0.35) prevents pitch-black surfaces if light is weak/missing
                float3 lighting = mainLight.color * (NdotL * 0.65 + 0.35);

                return float4(finalColor * lighting, 1.0);
            }
            ENDHLSL
        }
    }
}