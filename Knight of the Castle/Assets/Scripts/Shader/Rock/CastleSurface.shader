Shader "Custom/CastleSurface"
{
    Properties
    {
        [MainTexture]
        _BaseTexture ("Base Texture", 2D) = "white" {}

        [MainColor]
        _StoneColor ("Stone Color", Color) = (0.65, 0.65, 0.65, 1)

        _RoofColor ("Roof Color", Color) = (0.35, 0.12, 0.06, 1)

        _StoneTiling ("Stone Tiling", Float) = 1.0
        _RoofTiling ("Roof Tiling", Float) = 1.0

        _TextureStrength ("Texture Strength", Range(0, 2)) = 1.0

        _VariationStrength ("Surface Variation", Range(0, 1)) = 0.15
        _VariationScale ("Variation Scale", Float) = 3.0

        _Darkness ("Vertical Darkness", Range(0, 1)) = 0.25
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"

            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                float4 color      : COLOR;
                float fogFactor   : TEXCOORD3;
            };

            TEXTURE2D(_BaseTexture);
            SAMPLER(sampler_BaseTexture);

            CBUFFER_START(UnityPerMaterial)

                float4 _BaseTexture_ST;

                float4 _StoneColor;
                float4 _RoofColor;

                float _StoneTiling;
                float _RoofTiling;

                float _TextureStrength;

                float _VariationStrength;
                float _VariationScale;

                float _Darkness;

            CBUFFER_END


            // ---------------------------------------------------------
            // Simple procedural noise
            // ---------------------------------------------------------

            float Hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float Noise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);

                f = f * f * (3.0 - 2.0 * f);

                float n000 = Hash(i + float3(0,0,0));
                float n100 = Hash(i + float3(1,0,0));
                float n010 = Hash(i + float3(0,1,0));
                float n110 = Hash(i + float3(1,1,0));

                float n001 = Hash(i + float3(0,0,1));
                float n101 = Hash(i + float3(1,0,1));
                float n011 = Hash(i + float3(0,1,1));
                float n111 = Hash(i + float3(1,1,1));

                float nx00 = lerp(n000, n100, f.x);
                float nx10 = lerp(n010, n110, f.x);
                float nx01 = lerp(n001, n101, f.x);
                float nx11 = lerp(n011, n111, f.x);

                float nxy0 = lerp(nx00, nx10, f.y);
                float nxy1 = lerp(nx01, nx11, f.y);

                return lerp(nxy0, nxy1, f.z);
            }


            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;

                output.uv = TRANSFORM_TEX(input.uv, _BaseTexture);

                // Your C# vertex color:
                // R = 0 -> Stone
                // R = 1 -> Roof
                output.color = input.color;

                output.fogFactor =
                    ComputeFogFactor(positionInputs.positionCS.z);

                return output;
            }


            half4 frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);

                // -----------------------------------------------------
                // Determine material type from vertex color
                // -----------------------------------------------------

                float roofMask = saturate(input.color.r);

                // Roof uses its own tiling
                float roofUVScale = _RoofTiling;

                // Stone uses its own tiling
                float stoneUVScale = _StoneTiling;

                float2 stoneUV = input.uv * stoneUVScale;
                float2 roofUV  = input.uv * roofUVScale;

                // Sample the same base texture
                float4 stoneTexture =
                    SAMPLE_TEXTURE2D(
                        _BaseTexture,
                        sampler_BaseTexture,
                        stoneUV
                    );

                float4 roofTexture =
                    SAMPLE_TEXTURE2D(
                        _BaseTexture,
                        sampler_BaseTexture,
                        roofUV
                    );

                // -----------------------------------------------------
                // Apply texture
                // -----------------------------------------------------

                float3 stone =
                    lerp(
                        _StoneColor.rgb,
                        _StoneColor.rgb * stoneTexture.rgb,
                        _TextureStrength
                    );

                float3 roof =
                    lerp(
                        _RoofColor.rgb,
                        _RoofColor.rgb * roofTexture.rgb,
                        _TextureStrength
                    );

                // Select Stone / Roof
                float3 albedo =
                    lerp(stone, roof, roofMask);


                // -----------------------------------------------------
                // Procedural surface variation
                // -----------------------------------------------------

                float noise =
                    Noise(input.positionWS * _VariationScale);

                float variation =
                    lerp(
                        1.0 - _VariationStrength,
                        1.0 + _VariationStrength,
                        noise
                    );

                albedo *= variation;


                // -----------------------------------------------------
                // Vertical darkening
                // Gives castle more depth
                // -----------------------------------------------------

                float heightNoise =
                    saturate(input.positionWS.y * 0.05);

                float verticalDarkness =
                    lerp(
                        1.0 - _Darkness,
                        1.0,
                        heightNoise
                    );

                albedo *= verticalDarkness;


                // -----------------------------------------------------
                // Main directional light
                // -----------------------------------------------------

                Light mainLight =
                    GetMainLight();

                float NdotL =
                    saturate(dot(normal, mainLight.direction));

                float3 lighting =
                    mainLight.color *
                    (NdotL * 0.75 + 0.25);

                albedo *= lighting;


                // -----------------------------------------------------
                // Ambient
                // -----------------------------------------------------

                float3 ambient =
                    SampleSH(normal);

                albedo += ambient * 0.25;


                // -----------------------------------------------------
                // Fog
                // -----------------------------------------------------

                albedo =
                    MixFog(
                        albedo,
                        input.fogFactor
                    );

                return half4(albedo, 1);
            }

            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}