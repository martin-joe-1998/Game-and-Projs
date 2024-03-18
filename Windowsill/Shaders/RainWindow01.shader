Shader "Unlit/RainWindow01"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Size("Size", float) = 6
        _T("Time", float) = 0
        _Distortion("Distortion", range(-5, 5)) = 1
        _Blur("Blur", range(0, 1)) = 1
        _RainFall("RainFall", range(0, 4)) = 2.1
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
            // S(a, b, x):
            // 1) Returns 0 if x < a < b or x > a > b
            // 2) Returns 1 if x < b < a or x > b > a
            // 3) Returns a value in the range [0,1] for the domain [a,b].
            #define S(a, b, t) smoothstep(a, b, t)
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
            //
            float _Size, _T, _Distortion, _Blur, _RainFall;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // ���㲣���������ڲ����ϵ���ȷ��ʾuvλ�ã�
                o.grabUv = UNITY_PROJ_COORD(ComputeGrabScreenPos(o.vertex));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // ����һ��α���float�������ڿ���ÿ��grid�����ʱ���Ĳ��컯
            // ����һ��[0,1]֮���α�����
            float N21(float2 p) {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float3  Layer(float2 UV, float t){
                // ���ڰ�uv����ĺ��ݱȱ�Ϊ1:2���൱��u��_Size�Ļ����϶����2
                float2 aspect = float2(2, 1);
                // �ָ���ӣ�frac()���ظ�������С���㲿�֣���Ԫ�����򷵻�ÿ��ÿ����Ԫ��С��
                // uv�Ǹ������grid�ı���
                float2 uv = UV * _Size * aspect;

                // ����ʼ�ջ�����ֱ�·��ƶ�
                // ����ʹ���ԭ������Ը���������ٶȱ�ø���
                // ͬʱ��ԭ�������Ļ������������ǻ���ͣ��������
                // �˴���ע�⣬���ʵ�ƽ�����������������������᷽��һ�£�������ӻ����ƶ�
                uv.y += t * 0.35;

                // gv��uv��С���㲿��
                // ��ԭ���ǣ�������ԭ��(1, 1)��uv����ϵ�е����½ǵ���(0, 1)
                // ����x6��ȡС���Ժ󣬸õ�ͱ���˽���(0, 1/6)
                // -0.5��Ϊ�˰�ÿ�����ӵ�ԭ�㶨�ڸ�������
                // gv��grid����������
                float2 gv = frac(uv) - 0.5;

                // ÿ��grid��id
                float2 id = floor(uv);

                // ����һ��[0,1]֮���α�����
                float n = N21(id);
                // ��ʱ�������������ԣ�ʹÿ��grid����ʽ�������
                t += n * 3.1415;
                
                // w����x�����ı���
                float w = UV.y * 10;
                // x�����������켣�ĺ���ƫ��
                // (n - 0.5)ʹ��x��Ӧ[-0.5,0.5]��gv��Χ, 0.8ʹ����β�����ڿ���grid�߽�
                // x = [-0.4, 0.4]
                float x = (n - 0.5) * 0.8;
                // ͨ��(0.4 - abs(x))��Сx�ڱ߽總���İڶ�����
                x += (0.4 - abs(x)) * sin(3 * w) * pow(sin(w), 6) * 0.45;
                // y(v)������"�½��죬������"�ĺ��������µߵ�
                float y = -sin(t + sin(t + sin(t) * 0.6)) * 0.45;

                // ��ˮ�ε���״���������´���
                // �˴�-x��Ϊ�˵���֮ǰ
                y -= (gv.x - x) * (gv.x - x);

                // float2�Ӽ������ƶ�ԭ��(ˮ��)λ��
                // dropPos��ˮ�ε�λ��
                // gv/aspect�������������죬�γ���Բ
                float2 dropPos = (gv - float2(x, y)) / aspect;
                // S(a, b, t):��t<=a,����0.��t>=b,����1.��a<t<b,����һ��[a,b]���ƽ����ֵ
                // length(gv)���ڵ�ǰ���ص����������ڵĿ�����ĵľ���
                float drop = S(0.05, 0.03, length(dropPos));

                // ����һ��t*0.25(���������ƶ����ٶ�)��ʹ����κۼ���Ծ�ֹ
                float2 trailPos = (gv - float2(x, t * 0.35)) / aspect;
                // trail����κۼ���ÿ�����Ӵ���8����/8ʹ����״������ѹ
                trailPos.y = (frac(trailPos.y * 8) - 0.5) / 8;
                float trail = S(0.03, 0.01, length(trailPos));
                
                // ÿ������λ��С��-0.05�򷵻�0
                // ����0.05�򷵻�1
                float fogTrail = S(-0.05, 0.05, dropPos.y);
                // ʵ�־ɵ��������ʧ
                fogTrail *= S(0.5, y, gv.y);
                trail *= fogTrail;
                fogTrail *= S(0.05, 0.04, abs(dropPos.x));

                // offs���ڼ��������ص���д
                float2 offs = drop * dropPos + trail * dropPos;

                // Ϊ���ӻ����ߣ�
                // ����֮ǰ��-0.5��ÿ�����ӵ����ұ��[-0.5, 0.5]
                // ����x�ķ�Χ��y��һЩ��������Ҳ��һЩ
                //if (gv.x > 0.48 || gv.y > 0.49) col = float4(1, 0, 0, 1);
                
                return float3(offs, fogTrail);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // _Time.yÿ������1
                // ��t����ʱ�����������ô�����pattern��ͬʱ���Ӽ���cost
                // fmod����ʹ��tֵÿһСʱ������һ��,����������
                float t = fmod(_Time.y + _T, 3600);
                // black background
                float4 col = 0;

                // ���ص�drops.xy�������ص�offs,drops.z�ǹ켣���
                float3 drops = float3(0, 0, 0);

                float rainFall = floor(_RainFall); 
                if (rainFall > 0) drops += Layer(i.uv, t);
                if (rainFall >= 1) drops += Layer(1.14 * i.uv + 1.59, t);
                if (rainFall >= 2) drops += Layer(2.65 * i.uv + 3.58, t);
                if (rainFall >= 3) drops += Layer(1.23 * i.uv - 9.79, t);

                // ���ڿ��Ʋ�ͬ��������ʾЧ���ı���
                float fade = 1 - saturate(fwidth(i.uv) * 50);

                // _DistortionΪ0ʱ�����ʧ������0ʱ���䳡�����·���С����ʱ�ߵ��ķ��䳡�����Ϸ�
                // _Blur���ڿ��Ʋ����ϵ�������ɵ�ģ������Ҫtexture����trilliniar��
                // (1 - fogTrail)ʹ�ó��˹켣����ĵط���������
                float blur = _Blur * 7 * (1 - drops.z * fade);

                // ǰ�����ڲ���Ч�������
                //col = tex2Dlod(_MainTex, float4(i.uv + drops.xy * _Distortion, 0, blur));

                float2 projUv = i.grabUv.xy / i.grabUv.w;
                projUv += drops.xy * _Distortion * fade;
                blur *= 0.01;

                const float numSamples = 32;
                float a = N21(i.uv) * 6.2831;
                for (float i = 0; i < numSamples; i++){
                    float2 offs = float2(sin(a), cos(a)) * blur;
                    float d = frac(sin((i+1) * 546.) * 5443.);
                    d = sqrt(d);
                    offs *= d;
                    col += tex2D(_GrabTexture, projUv + offs);
                    a++;
                }
                col /= numSamples;

                return col;
            }
            ENDCG
        }
    }
}
