Shader "Custom/StylizedProceduralTower_Textured"
{
    Properties
    {
        [Header(Wall Textures)]
        [MainTexture] _BaseWallTex ("Rock/Wall Base Texture", 2D) = "white" {}
        _WallColorTint ("Wall Color Tint", Color) = (1, 1, 1, 1)
        _WallTiling ("Wall Texture Tiling (X, Y)", Vector) = (4.0, 8.0, 0, 0)

        [Header(Roof Textures)]
        _BaseRoofTex ("Roof Tile Texture", 2D) = "white" {}
        _RoofColorTint ("Roof Color Tint", Color) = (1, 1, 1, 1)
        _RoofTiling ("Roof Texture Tiling (X, Y)", Vector) = (6.0, 6.0, 0, 0)

        [Header(Stylized Lighting)]
        _RampThreshold ("Shadow Threshold", Range(0, 1)) = 0.45
        _RampSmoothness ("Shadow Smoothness", Range(0.001, 0.2)) = 0.05
        _ShadowColor ("Shadow Tint", Color) = (0.2, 0.2, 0.22, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Textures and Samplers Declaration
            Texture2D _BaseWallTex;
            SamplerState sampler_BaseWallTex;

            Texture2D _BaseRoofTex;
            SamplerState sampler_BaseRoofTex;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR; // Tag: Red channel = Roof mask (1 = Roof, 0 = Wall)
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float4 color      : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _WallColorTint;
                float4 _WallTiling;
                float4 _RoofColorTint;
                float4 _RoofTiling;
                float _RampThreshold;
                float _RampSmoothness;
                float4 _ShadowColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS, float4(0,0,0,0));

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normInputs.normalWS;
                output.color = input.color;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // 1. Calculate World-Space Polar UVs (Wraps seamlessly around cylinder)
                float angle = atan2(input.positionWS.z, input.positionWS.x) / (2.0 * 3.14159265);
                float2 polarUV = float2(angle, input.positionWS.y);

                // 2. Sample Rock/Wall Texture
                float2 wallUV = polarUV * _WallTiling.xy;
                float4 wallSample = _BaseWallTex.Sample(sampler_BaseWallTex, wallUV) * _WallColorTint;

                // 3. Sample Roof Texture
                float2 roofUV = polarUV * _RoofTiling.xy;
                float4 roofSample = _BaseRoofTex.Sample(sampler_BaseRoofTex, roofUV) * _RoofColorTint;

                // 4. Mask and Blend (using C# Red Vertex Color)
                float isRoof = input.color.r;
                float3 albedo = lerp(wallSample.rgb, roofSample.rgb, isRoof);

                // 5. Stylized Toon Lighting (Fixed against Blue Glow / HDR Bloom blowout)
                Light mainLight = GetMainLight();
                float3 N = normalize(input.normalWS);
                float3 L = normalize(mainLight.direction);

                float NdotL = max(0.0, dot(N, L));
                float toonLight = smoothstep(_RampThreshold - _RampSmoothness, 
                                            _RampThreshold + _RampSmoothness, 
                                            NdotL);

                // Normalize light color to prevent HDR over-exposure on full White
                float3 normalizedLightColor = saturate(mainLight.color);

                // Blend lighting using neutral shadow tint
                float3 lighting = lerp(_ShadowColor.rgb, normalizedLightColor, toonLight);

                // 6. Height Ambient Occlusion
                float heightAO = saturate(input.positionWS.y * 0.25 + 0.15);

                // 7. Clamp final RGB to prevent Bloom glow trigger on White tint
                float3 finalColor = min(albedo * lighting * heightAO, 1.0);

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}