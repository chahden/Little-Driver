Shader "Little Driver/Sidi Street Surface"
{
    Properties
    {
        [Enum(Stone Paving,0,Red White Curb,1)] _SurfaceStyle("Surface Style", Float) = 0
        _StoneA("Stone Colour A", Color) = (0.58,0.56,0.49,1)
        _StoneB("Stone Colour B", Color) = (0.78,0.75,0.65,1)
        _JointColour("Stone Joint Colour", Color) = (0.29,0.28,0.24,1)
        _StoneSize("Stone Size In Metres (X,Z)", Vector) = (0.28,0.34,0,0)
        _JointWidth("Joint Width In Metres", Range(0.002,0.05)) = 0.018
        _CurbRed("Curb Red", Color) = (0.68,0.08,0.065,1)
        _CurbWhite("Curb White", Color) = (0.90,0.88,0.82,1)
        _StripeLength("Each Curb Band In Metres", Range(0.1,2)) = 0.5
        _StripeOffset("Curb Band Offset", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" "DisableBatching"="True" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _StoneA, _StoneB, _JointColour, _CurbRed, _CurbWhite;
            float4 _StoneSize;
            float _SurfaceStyle, _JointWidth, _StripeLength, _StripeOffset;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            half3 normalWS : TEXCOORD1;
            float3 positionOS : TEXCOORD2;
            float4 shadowCoord : TEXCOORD3;
            half fog : TEXCOORD4;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };
        Varyings Vertex(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            VertexPositionInputs p = GetVertexPositionInputs(input.positionOS.xyz);
            output.positionCS = p.positionCS;
            output.positionWS = p.positionWS;
            output.positionOS = input.positionOS.xyz;
            output.normalWS = TransformObjectToWorldNormal(input.normalOS);
            output.shadowCoord = GetShadowCoord(p);
            output.fog = ComputeFogFactor(p.positionCS.z);
            return output;
        }
        float Hash(float2 p)
        {
            p = frac(p * float2(123.34, 456.21));
            p += dot(p, p + 45.32);
            return frac(p.x * p.y);
        }
        half3 Paving(float3 worldPosition, half3 normal)
        {
            // World-space coordinates keep the stone size independent of block scaling.
            float2 uv = worldPosition.xz / max(_StoneSize.xy, 0.05);
            float row = floor(uv.y);
            uv.x += frac(row * 0.5) + (Hash(float2(row, 7)) - 0.5) * 0.10;
            float2 cell = floor(uv);
            float2 tile = frac(uv);
            float h = Hash(cell);
            float2 gap = _JointWidth / max(_StoneSize.xy, 0.05);
            float radius = 0.065;
            float2 q = abs(tile - 0.5) - (0.5 - gap * 0.5) + radius;
            float distance = length(max(q, 0)) + min(max(q.x, q.y), 0) - radius;
            float aa = max(fwidth(distance), 0.004);
            half mask = 1 - smoothstep(-aa, aa, distance);
            half3 stone = lerp(_StoneA.rgb, _StoneB.rgb, h);
            stone *= lerp(0.82h, 1.0h, saturate(-distance / 0.055));
            half3 top = lerp(_JointColour.rgb, stone, mask);
            half3 side = lerp(_StoneA.rgb, _StoneB.rgb, 0.3h);
            return lerp(side, top, saturate((normal.y - 0.5h) * 3));
        }
        half3 Curb(float3 objectPosition)
        {
            float4x4 m = GetObjectToWorldMatrix();
            float sizeX = length(float3(m._m00, m._m10, m._m20));
            float sizeZ = length(float3(m._m02, m._m12, m._m22));
            // Unit cube curb meshes repeat along their longest horizontal axis.
            float along = sizeX > sizeZ ? (objectPosition.x + 0.5) * sizeX : (objectPosition.z + 0.5) * sizeZ;
            float band = along / max(_StripeLength, 0.05) + _StripeOffset;
            float aa = min(fwidth(band), 0.2);
            half whiteBand = smoothstep(0.5 - aa, 0.5 + aa, frac(band * 0.5));
            half3 colour = lerp(_CurbRed.rgb, _CurbWhite.rgb, whiteBand);
            return colour * lerp(0.97h, 1.0h, Hash(floor(float2(along * 3, 5))));
        }
        half4 Fragment(Varyings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            half3 normal = normalize(input.normalWS);
            half3 albedo = _SurfaceStyle < 0.5 ? Paving(input.positionWS, normal) : Curb(input.positionOS);
            #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                float4 shadowCoord = input.shadowCoord;
            #else
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
            #endif
            Light sun = GetMainLight(shadowCoord);
            half3 illumination = SampleSH(normal) + sun.color * saturate(dot(normal, sun.direction)) * sun.distanceAttenuation * sun.shadowAttenuation;
            #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                for (uint i = 0; i < count; i++)
                {
                    Light light = GetAdditionalLight(i, input.positionWS);
                    illumination += light.color * saturate(dot(normal, light.direction)) * light.distanceAttenuation * light.shadowAttenuation;
                }
            #endif
            return half4(MixFog(albedo * illumination, input.fog), 1);
        }
        ENDHLSL
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ColorMask 0
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            float3 _LightDirection;
            float3 _LightPosition;
            float4 ShadowVertex(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 position = TransformObjectToWorld(input.positionOS.xyz);
                float3 normal = TransformObjectToWorldNormal(input.normalOS);
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                    float3 direction = normalize(_LightPosition - position);
                #else
                    float3 direction = _LightDirection;
                #endif
                float4 clip = TransformWorldToHClip(ApplyShadowBias(position, normal, direction));
                #if UNITY_REVERSED_Z
                    clip.z = min(clip.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    clip.z = max(clip.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return clip;
            }
            half4 ShadowFragment() : SV_Target { return 0; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask R
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vertex
            #pragma fragment DepthFragment
            #pragma multi_compile_instancing
            half4 DepthFragment(Varyings input) : SV_Target { return input.positionCS.z; }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
