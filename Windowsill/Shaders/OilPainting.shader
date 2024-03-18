Shader "Unlit/OilPainting"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius", Range(0, 10)) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Transparent" }
        LOD 100
        // Interpolation to control alpha
        Blend SrcAlpha OneMinusSrcAlpha

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
             #pragma target 3.0
            // make fog work
            #pragma multi_compile_fog

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
                return o;
            }

            int _Radius;
            float4 _MainTex_TexelSize;

            fixed4 frag (v2f i) : SV_Target
            {
                half2 uv = i.uv;
                float2 projUv = i.grabUv.xy / i.grabUv.w;

                // Divided differences array
                float3 mean[4] = {
                    {0, 0, 0},
                    {0, 0, 0},
                    {0, 0, 0},
                    {0, 0, 0}
                };
                
                // Variance array
                float3 sigma[4] = {
                    {0, 0, 0},
                    {0, 0, 0},
                    {0, 0, 0},
                    {0, 0, 0}
                };
                
                // Offset array
                float2 start[4] = {{-_Radius, -_Radius}, {-_Radius, 0}, {0, -_Radius}, {0, 0}};
 
                float2 pos;
                float3 col;
                // Kawahara filter
                // Traverse every pixel
                for (int k = 0; k < 4; k++) {
                    for(int i = 0; i <= _Radius; i++) {
                        for(int j = 0; j <= _Radius; j++) {
                            pos = float2(i, j) + start[k];
                            col = tex2Dlod(_GrabTexture, float4(projUv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0., 0.)).rgb;
                            mean[k] += col;
                            sigma[k] += col * col;
                        }
                    }
                }
                
                // Total variance
                float sigma2;

                // num of pixel in filter window
                float n = pow(_Radius + 1, 2);
                float4 _color = tex2D(_GrabTexture, projUv);
                float min = 1;
                
                // Calc total var and color
                for (int l = 0; l < 4; l++) {
                    mean[l] /= n;
                    sigma[l] = abs(sigma[l] / n - mean[l] * mean[l]);
                    sigma2 = sigma[l].r + sigma[l].g + sigma[l].b;
                    
                    // Update min variance and color
                    if (sigma2 < min) {
                        min = sigma2;
                        _color.rgb = mean[l].rgb;
                    }
                }

                return _color;
            }
            ENDCG
        }
    }
}
