Shader "Custom/LowPolyTree"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}

        [Header(Foliage Season Colors)]
        _SpringColor ("Spring Foliage", Color) = (0.4, 0.8, 0.25, 1)
        _SummerColor ("Summer Foliage", Color) = (0.15, 0.65, 0.2, 1)
        _AutumnColor ("Autumn Foliage", Color) = (0.9, 0.45, 0.1, 1)
        _WinterColor ("Winter Foliage", Color) = (0.75, 0.85, 0.9, 1)

        [Header(Trunk Color)]
        _TrunkColor ("Bark Color", Color) = (0.35, 0.22, 0.12, 1)

        [Header(Snow Settings)]
        _SnowColor ("Snow Color", Color) = (0.95, 0.98, 1.0, 1)
        _SnowAmount ("Manual Snow Intensity", Range(0, 1)) = 0.0

        [Header(Wind Settings)]
        _WindSpeed ("Wind Speed", Float) = 2.0
        _WindStrength ("Wind Strength", Float) = 0.12

        [Header(Season State)]
        _SeasonProgress ("Season Progress (0..4)", Range(0, 4)) = 0.0
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
                float4 color : COLOR; // Red channel = Trunk (0) vs Leaves (1)
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float2 uv : TEXCOORD0;
                float vertexIsLeaf : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SpringColor;
                float4 _SummerColor;
                float4 _AutumnColor;
                float4 _WinterColor;
                float4 _TrunkColor;
                float4 _SnowColor;
                float _SnowAmount;
                float _WindSpeed;
                float _WindStrength;
                float _SeasonProgress;
            CBUFFER_END

            float4 GetSeasonFoliageColor(float progress)
            {
                float p = frac(progress / 4.0) * 4.0;
                if (p < 1.0) 
                    return lerp(_SpringColor, _SummerColor, p);
                else if (p < 2.0) 
                    return lerp(_SummerColor, _AutumnColor, p - 1.0);
                else if (p < 3.0) 
                    return lerp(_AutumnColor, _WinterColor, p - 2.0);
                else 
                    return lerp(_WinterColor, _SpringColor, p - 3.0);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);

                // Wind sway: Only sway leaf/branch vertices (Vertex Color Red > 0.1)
                float isLeaf = input.color.r;
                float heightFactor = saturate(input.positionOS.y * 0.3);
                float windWave = sin(_Time.y * _WindSpeed + worldPos.x + worldPos.z) * _WindStrength;
                
                worldPos.x += windWave * heightFactor * isLeaf;
                worldPos.z += windWave * 0.5 * heightFactor * isLeaf;

                output.positionCS = TransformWorldToHClip(worldPos);
                output.worldNormal = TransformObjectToWorldNormal(input.normalOS);
                output.worldPos = worldPos;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.vertexIsLeaf = isLeaf;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float4 seasonFoliage = GetSeasonFoliageColor(_SeasonProgress);

                // Blend between Bark Color and Season Foliage based on vertex color mask
                float3 baseTreeColor = lerp(_TrunkColor.rgb, seasonFoliage.rgb, input.vertexIsLeaf);
                baseTreeColor *= texColor.rgb;

                // Snow calculation (Accumulates on top surfaces facing up)
                float3 norm = normalize(input.worldNormal);
                float upDot = saturate(dot(norm, float3(0, 1, 0)));

                float autoWinterSnow = saturate(1.0 - abs(_SeasonProgress - 3.0));
                float totalSnow = max(_SnowAmount, autoWinterSnow);
                float snowMask = pow(upDot, 1.8) * totalSnow;

                float3 finalAlbedo = lerp(baseTreeColor, _SnowColor.rgb, snowMask);

                // Stylized Lighting calculation
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(norm, mainLight.direction));
                float3 lighting = mainLight.color * (NdotL * 0.65 + 0.35);

                return float4(finalAlbedo * lighting, 1.0);
            }
            ENDHLSL
        }
    }
}