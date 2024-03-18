Shader "Unlit/MeanShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius", Range(1, 5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            // size of pixel, which structure is (1/width, 1/height, width, height)
            float4 _MainTex_TexelSize;
            // range of sampling
            float _Radius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half2 uv = i.uv;
                int r = _Radius;
                float2 filter[9] = {{0, 0}, {r, 0}, {0, r}, {r, r}, {-r, 0}, {0, -r}, {-r, -r}, {r, -r}, {-r, r}};
                float2 pos;
                float3 col = 0;

                // calculate average rgb value of every pixel in a 3x3 filter window
                int count = 0;
                for(int i = 0; i < 9; i++)
                {
                    pos =  filter[i];
                    float2 tar_uv = uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y);
                    // continue if uv is out of bound, to make boundary of tex being processed correctly
                    if (tar_uv.x <= 0 || tar_uv.x >= 1 || tar_uv.y <= 0 || tar_uv.y >= 1){
                        continue;
                    }

                    col += tex2Dlod(_MainTex, float4(tar_uv, 0., 0.)).rgb;
                    count++;
                }
                col /= count;

                float4 fin_col = tex2D(_MainTex, uv);
                fin_col.rgb = col;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, fin_col);

                return fin_col;
            }
            ENDCG
        }
    }
}
