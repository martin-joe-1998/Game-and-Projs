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
                // 计算玻璃后物体在玻璃上的正确显示uv位置？
                o.grabUv = UNITY_PROJ_COORD(ComputeGrabScreenPos(o.vertex));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // 返回一个伪随机float数，用于控制每个grid里雨滴时机的差异化
            // 产生一个[0,1]之间的伪随机数
            float N21(float2 p) {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float3  Layer(float2 UV, float t){
                // 用于把uv坐标的横纵比变为1:2，相当于u在_Size的基础上多乘了2
                float2 aspect = float2(2, 1);
                // 分割格子，frac()返回浮点数的小数点部分，多元数据则返回每个每个次元的小数
                // uv是负责控制grid的变量
                float2 uv = UV * _Size * aspect;

                // 格子始终缓慢向垂直下方移动
                // 这样使雨滴原本就相对更快的下落速度变得更快
                // 同时让原本就慢的回升看起来像是缓慢停留或下落
                // 此处需注意，材质的平面的坐标轴必须与世界坐标轴方向一致，否则格子会乱移动
                uv.y += t * 0.35;

                // gv是uv的小数点部分
                // 其原理是：比如在原本(1, 1)的uv坐标系中的左下角点是(0, 1)
                // 经过x6再取小数以后，该点就变成了近似(0, 1/6)
                // -0.5是为了把每个格子的原点定在格子中心
                // gv是grid的中心坐标
                float2 gv = frac(uv) - 0.5;

                // 每个grid的id
                float2 id = floor(uv);

                // 产生一个[0,1]之间的伪随机数
                float n = N21(id);
                // 给时间变量增加随机性，使每个grid的样式是随机的
                t += n * 3.1415;
                
                // w用于x函数的变量
                float w = UV.y * 10;
                // x控制雨滴下落轨迹的横向偏移
                // (n - 0.5)使得x适应[-0.5,0.5]的gv范围, 0.8使得雨滴不会过于靠近grid边界
                // x = [-0.4, 0.4]
                float x = (n - 0.5) * 0.8;
                // 通过(0.4 - abs(x))减小x在边界附近的摆动幅度
                x += (0.4 - abs(x)) * sin(3 * w) * pow(sin(w), 6) * 0.45;
                // y(v)方向是"下降快，回升慢"的函数的上下颠倒
                float y = -sin(t + sin(t + sin(t) * 0.6)) * 0.45;

                // 让水滴的形状看起来是下垂的
                // 此处-x是为了抵消之前
                y -= (gv.x - x) * (gv.x - x);

                // float2加减可以移动原点(水滴)位置
                // dropPos是水滴的位置
                // gv/aspect来抵消方框拉伸，形成正圆
                float2 dropPos = (gv - float2(x, y)) / aspect;
                // S(a, b, t):当t<=a,返回0.当t>=b,返回1.当a<t<b,返回一个[a,b]间的平滑插值
                // length(gv)等于当前像素到该像素所在的框的中心的距离
                float drop = S(0.05, 0.03, length(dropPos));

                // 减掉一个t*0.25(格子向下移动的速度)，使得雨滴痕迹相对静止
                float2 trailPos = (gv - float2(x, t * 0.35)) / aspect;
                // trail（雨滴痕迹）每个格子存在8个，/8使其形状不被挤压
                trailPos.y = (frac(trailPos.y * 8) - 0.5) / 8;
                float trail = S(0.03, 0.01, length(trailPos));
                
                // 每个纵向位置小于-0.05则返回0
                // 大于0.05则返回1
                float fogTrail = S(-0.05, 0.05, dropPos.y);
                // 实现旧的雨滴逐渐消失
                fogTrail *= S(0.5, y, gv.y);
                trail *= fogTrail;
                fogTrail *= S(0.05, 0.04, abs(dropPos.x));

                // offs用于集合雨滴相关的描写
                float2 offs = drop * dropPos + trail * dropPos;

                // 为格子画红线，
                // 由于之前的-0.5，每个格子的左右变成[-0.5, 0.5]
                // 这里x的范围比y大一些，所以线也粗一些
                //if (gv.x > 0.48 || gv.y > 0.49) col = float4(1, 0, 0, 1);
                
                return float3(offs, fogTrail);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // _Time.y每秒增加1
                // 当t过大时，会产生不那么随机的pattern，同时增加计算cost
                // fmod函数使得t值每一小时就重置一次,解决这个问题
                float t = fmod(_Time.y + _T, 3600);
                // black background
                float4 col = 0;

                // 返回的drops.xy是雨滴相关的offs,drops.z是轨迹相关
                float3 drops = float3(0, 0, 0);

                float rainFall = floor(_RainFall); 
                if (rainFall > 0) drops += Layer(i.uv, t);
                if (rainFall >= 1) drops += Layer(1.14 * i.uv + 1.59, t);
                if (rainFall >= 2) drops += Layer(2.65 * i.uv + 3.58, t);
                if (rainFall >= 3) drops += Layer(1.23 * i.uv - 9.79, t);

                // 用于控制不同距离下显示效果的变量
                float fade = 1 - saturate(fwidth(i.uv) * 50);

                // _Distortion为0时雨滴消失，大于0时反射场景在下方，小于零时颠倒的反射场景在上方
                // _Blur用于控制玻璃上的雾气造成的模糊（需要texture设置trilliniar）
                // (1 - fogTrail)使得除了轨迹以外的地方都有雾气
                float blur = _Blur * 7 * (1 - drops.z * fade);

                // 前期用于测试效果的语句
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
