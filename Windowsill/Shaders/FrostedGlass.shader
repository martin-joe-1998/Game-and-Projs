Shader "Unlit/FrostedGlass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Frost ("Frost", Range(0.001, 0.1)) = 0.01
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
            float _Frost;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // Calculate the uv coord of the scene behind the glass
                o.grabUv = UNITY_PROJ_COORD(ComputeGrabScreenPos(o.vertex));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // Hash function
            float hash( float n )
            {
                return frac(sin(n)*43758.5453);
            }

            // The noise function returns a value in the range -1.0f -> 1.0f
            float SimplexNoise(float3 x)
            {
                
                float3 p = floor(x);
                float3 f = frac(x);
            
                f = f * f * (3.0 - 2.0 * f);
                float n = p.x + p.y * 57.0 + 113.0 * p.z;
            
                return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
                                 lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
                            lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
                                 lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // noise t = -1.0f -> 1.0f
                float t = SimplexNoise(i.vertex.xyz); 
                t = (t - 0.5f) * 2.0f;

                // output
                fixed4 col = 0;

                // normalization of projector uv
                float2 projUv = i.grabUv.xy / i.grabUv.w;
                // plus noise on projUv
                col = tex2D(_GrabTexture, projUv + t * _Frost);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
