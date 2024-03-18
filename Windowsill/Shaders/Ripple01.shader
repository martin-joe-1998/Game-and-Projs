Shader "Unlit/Ripple01"
{
    Properties
    {
        _MainTex ("Fross Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Tags
            {
                // Specify LightMode correctly.
                "LightMode" = "UseColorTexture"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #define pi 3.1415926

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 grabUv : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex, _GrabTexture;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.grabUv = UNITY_PROJ_COORD(ComputeGrabScreenPos(o.vertex));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = 0;//tex2D(_MainTex, i.uv);

                // newUV = -1 -> 1
                float2 newUV = i.uv * 2 - 1;

                float t = fmod(_Time.y, 4);
                float ring_pos = 0.3 * t;
                float ring_r = 0.05;

                float newUV_len = length(newUV);
                // 1st ripple
                // fade center -> outside 
                if (newUV_len <= ring_pos + ring_r && newUV_len >= ring_pos)
                {
                    // x = 0 -> 1
                    float x = (newUV_len - ring_pos) * 20;
                    float fadeOut = lerp(1.0, 0.0, sin(x * pi/2));
                    col += float4(fadeOut, fadeOut, fadeOut, fadeOut);
                } // fade inside -> center
                else if (newUV_len >= ring_pos - ring_r && newUV_len <= ring_pos){
                    // x = 0 -> 1
                    float x = (newUV_len - ring_pos + ring_r) * 20;
                    float fadeIn = lerp(1.0, 0.0, sqrt(1 - x*x));
                    col += float4(fadeIn, fadeIn, fadeIn, fadeIn);
                }

                // 2nd ripple
                if (newUV_len - 0.2 > 0)
                {
                    newUV_len = max(newUV_len - 0.2, 0);
                    if (newUV_len <= ring_pos + ring_r && newUV_len >= ring_pos)
                    {
                        // x = 0 -> 1
                        float x = (newUV_len - ring_pos) * 20;
                        float fadeOut = lerp(1.0, 0.0, sin(x * pi/2));
                        col += float4(fadeOut, fadeOut, fadeOut, fadeOut);
                    } 
                    else if (newUV_len >= ring_pos - ring_r && newUV_len <= ring_pos)
                    {
                        // x = 0 -> 1
                        float x = (newUV_len - ring_pos + ring_r) * 20;
                        float fadeIn = lerp(1.0, 0.0, sqrt(1 - x*x));
                        col += float4(fadeIn, fadeIn, fadeIn, fadeIn);
                    }
                }
                
                // 3rd ripple
                if (newUV_len - 0.2 > 0)
                {
                    newUV_len = max(newUV_len - 0.2, 0);
                    if (newUV_len <= ring_pos + ring_r && newUV_len >= ring_pos)
                    {
                        // x = 0 -> 1
                        float x = (newUV_len - ring_pos) * 20;
                        float fadeOut = lerp(1.0, 0.0, sin(x * pi/2));
                        col += float4(fadeOut, fadeOut, fadeOut, fadeOut);
                    } 
                    else if (newUV_len >= ring_pos - ring_r && newUV_len <= ring_pos)
                    {
                        // x = 0 -> 1
                        float x = (newUV_len - ring_pos + ring_r) * 20;
                        float fadeIn = lerp(1.0, 0.0, sqrt(1 - x*x));
                        col += float4(fadeIn, fadeIn, fadeIn, fadeIn);
                    }
                }
                
                //process about distortion and transparence
                float2 projUv = i.grabUv.xy / i.grabUv.w;
                float distortion = -0.1;
                projUv += col * distortion;
                col = tex2D(_GrabTexture, projUv);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
