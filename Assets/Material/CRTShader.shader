Shader "Custom/CRTShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _ScanlineIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                
                o.uv = v.uv;

                // 곡률 효과 적용
                // o.uv.x += (o.pos.x - 0.5) * 0.05;
                // o.uv.y += (o.pos.y - 0.5) * 0.05;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // 기본 텍스처
                float4 color = tex2D(_MainTex, i.uv);

                // 스캔 라인 효과
                float scanline = sin(i.uv.y * _ScreenParams.y * 2.0) * _ScanlineIntensity;
                color.rgb -= scanline;

                // 색상 분리
                float2 redOffset = i.uv + float2(0.002, 0);
                float2 blueOffset = i.uv + float2(-0.002, 0);
                float4 redChannel = tex2D(_MainTex, redOffset);
                float4 blueChannel = tex2D(_MainTex, blueOffset);
                float4 greenChannel = tex2D(_MainTex, i.uv);

                color.r = redChannel.r;
                color.g = greenChannel.g;
                color.b = blueChannel.b;

                // 가장자리 블러 (Vignette)
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                float vignette = smoothstep(0.7, 1.0, dist);
                color.rgb *= (1.0 - vignette * 0.5);

                // 노이즈 효과
                float noise = frac(sin(dot(i.uv.xy, float2(12.9898,78.233))) * 43758.5453);
                color.rgb += noise * 0.05;

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}