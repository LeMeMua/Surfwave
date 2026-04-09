Shader "Custom/Waves"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
/*         _Amplitude ("Amplitude", float) = 1 */        
        //_Steepness ("Steepness",Range(0,1)) = 0.5
        //_Wavelength ("Wavelength", float) = 10
        //_Speed ("Speed", float) = 10
        //_Direction ("Direction (2D)", Vector) = (1,0,0,0)
        _WaveA("Wave A (dir, steepness, wavelength)", Vector) = (1,0,0.5,10)
        _WaveB("Wave B", Vector) = (0.5, 0.75, 0.2, 40)
        _WaveC("Wave C", Vector) = (0.2, 0.5, 0.05, 20)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;

                half _Metallic;
                half _Smoothness;
                //half _Amplitude;
                //half _Steepness;
                //half _Wavelength;
                //half _Speed;
                //float2 _Direction;
                float4 _WaveA;
                float4 _WaveB;
                float4 _WaveC;
            CBUFFER_END

            float3 Gerstner_waves(float4 wave, float3 p, inout float3 tangent, inout float3 binormal)
            {
                float steepness = wave.z;
                float wavelength = wave.w;
                float k = 2* PI/ wavelength; //Velocidad de las olas
                float c = sqrt(9.8/k); //Entre más larga, más rápida
                float2 d = normalize(wave.xy);
                float f = k *(dot(d, p.xz)- c * _Time.y);
                float a = steepness/k; //que tan alta puede ser sin colapsar sobre si misma, genera ciclos

                tangent += float3(
                    -d.x * d.x * (steepness * sin(f)),
                    d.x * (steepness * cos(f)),
                    -d.x * d.y * (steepness * sin(f))
                );
                binormal += float3 (
                    -d.x * d.y * (steepness * sin(f)),
                    d.y * (steepness * cos(f)),
                    -d.x * d.y * (steepness * sin(f))
                );
                return float3(
                    d.x * (a * cos(f)),
                    a * sin(f),
                    d.y * (a * cos(f))
                );
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 gridPoint = IN.positionOS.xyz;
                float3 tangent = float3(1,0,0);
                float3 binormal = float3(0,0,1);
                float3 p = gridPoint;

                p += Gerstner_waves(_WaveA, gridPoint, tangent, binormal);
                p += Gerstner_waves(_WaveB, gridPoint, tangent, binormal);
                float3 normal = normalize(cross(binormal,tangent));
                //OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionHCS = TransformObjectToHClip(p);
                OUT.positionWS = TransformObjectToWorld(p);
                OUT.normal = TransformObjectToWorldNormal(normal); //las normales se usan en world space para calcular para el mundo
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light light = GetMainLight(shadowCoord);
                float3 normal = normalize(IN.normal);
                float NdotL = saturate(dot(normal, light.direction)); //sacamos que tanto se va aplicar la normal en la luz
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                float3 result = color.rgb * light.color * NdotL * light.shadowAttenuation; // se multiplican los valores de los colores del plano, por el color de la luz y la dirección de la luz
                return half4(result, color.a);
            }
            ENDHLSL
        }
    }
}
