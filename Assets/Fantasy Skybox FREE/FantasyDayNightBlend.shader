Shader "Skybox/Fantasy Day Night Blend"
{
    Properties
    {
        _DayTex ("Day Panorama", 2D) = "white" {}
        _NightTex ("Night Panorama", 2D) = "black" {}
        _NightBlend ("Night Blend", Range(0, 1)) = 0
        _Rotation ("Rotation", Range(0, 360)) = 0
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Exposure ("Exposure", Range(0, 8)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "PreviewType" = "Skybox"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _DayTex;
            sampler2D _NightTex;

            float _NightBlend;
            float _Rotation;
            float4 _Tint;
            float _Exposure;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 direction : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.direction = v.vertex.xyz;
                return o;
            }

            float2 DirectionToUV(float3 direction)
            {
                direction = normalize(direction);

                float longitude = atan2(
                    direction.x, direction.z
                );

                float latitude = asin(
                    clamp(direction.y, -1.0, 1.0)
                );

                float2 uv = float2(
                    longitude / (2.0 * UNITY_PI) + 0.5,
                    latitude / UNITY_PI + 0.5
                );

                uv.x = frac(uv.x + _Rotation / 360.0);

                return uv;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = DirectionToUV(i.direction);

                fixed4 day = tex2D(_DayTex, uv);
                fixed4 night = tex2D(_NightTex, uv);

                fixed4 result = lerp(
                    day,
                    night,
                    saturate(_NightBlend)
                );

                result.rgb *= _Tint.rgb * _Exposure;
                result.a = 1;

                return result;
            }
            ENDCG
        }
    }

    Fallback Off
}