Shader "Custom/Imposter"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (1, 1, 1, 1)
        [IntRange]_TileX ("Tile X Size", Range(1, 20)) = 1.0
        [IntRange]_TileY ("Tile Y Size", Range(1, 20)) = 1.0
        _AngleXRange ("Angle X Range", Range(0, 180)) = 0
        _AngleYRange ("Angle Y Range", Range(0, 90)) = 0
        
        [Space][Space]
        [Toggle(_ENABLE_ANIMATION)]_Animation ("Animation", Int) = 0
        _SubTex ("Sub Texture", 2D) = "black" {}
        _AnimSpeed ("Animation Speed", Float) = 1.0
        _Ratio ("Main <--> Sub : Ratio", Range(-0.99, 0.99)) = 0.0
        _Delay ("Start DelayTime", Float) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ENABLE_ANIMATION
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 rowcol : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;      // 1/width, 1/height, width, height
            fixed4 _MainColor;
            float _TileX;
            float _TileY;
            float _AngleXRange;
            float _AngleYRange;
            sampler2D _SubTex;
            half _AnimSpeed;
            half _Ratio;
            half _Delay;
            
            // Billboard
            float3 GetRotatedPosByVector(float center, float3 vertex)
            {
                float3 viewer = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1)).xyz;
                float3 normalDir = viewer - center;
                normalDir.y = 0;
                normalDir = normalize(normalDir);
                float3 upDir = abs(normalDir.y) > 0.999 ? float3(0, 0, 1) : float3(0, 1, 0);
                float3 rightDir = normalize(cross(upDir, normalDir));
                upDir = normalize(cross(normalDir, rightDir));
                float3 centerOffs = vertex.xyz - center;
                float3 localPos = center + rightDir * centerOffs.x * -1 + upDir * centerOffs.y + normalDir * centerOffs.z;
                return localPos;
            }
            
            float2 RecTangularToSpherical(float3 p)
            {
                float r = sqrt(dot(p, p));
                return float2(atan2(p.z, p.x) * 180 / UNITY_PI, acos(p.y / r) * 180 / UNITY_PI); // phi[-pi, pi], theta[0, pi]
            }
            
            VertexOutput vert (VertexInput v)
            {
                VertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v); 
                float3 centerOS = float3(0, 0, 0);
                float3 positionOS = GetRotatedPosByVector(centerOS, v.positionOS.xyz);
                float3 centerWS = mul(unity_ObjectToWorld, float4(centerOS, 1));
                float2 sphWS = RecTangularToSpherical(_WorldSpaceCameraPos - centerWS);
                
                float widAngleUnit = _AngleXRange / _TileX;
                float heiAngleUnit = _AngleYRange / _TileY;
                float row = floor(clamp(90 - sphWS.y, 0, _AngleYRange) / heiAngleUnit); // png从下往上第几行
                float widOffset = (180 - _AngleXRange) * 0.5;
                float col = floor(clamp(sphWS.x - widOffset, 0, 180 - widOffset) / widAngleUnit); // png从左往右第几列
                o.rowcol = float2(clamp(row, 0, _TileY-1), clamp(col, 0, _TileX - 1));
                o.positionCS = UnityObjectToClipPos(positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (VertexOutput i) : SV_Target
            {
                float xScale = 1 - ((_MainTex_TexelSize.z % _TileX) / _MainTex_TexelSize.z); // 分辨率无法整除行列数时，进行微小的缩放
                float yScale = 1 - ((_MainTex_TexelSize.w % _TileY) / _MainTex_TexelSize.w);
                float row = i.rowcol.x;
                float col = i.rowcol.y;
                float2 uv = float2(
                    ((col + i.uv.x) * xScale) / _TileX,
                    ((row + i.uv.y) * yScale) / _TileY
                );
                fixed4 color = tex2D(_MainTex, uv) * _MainColor;
            #ifdef _ENABLE_ANIMATION
                fixed4 subColor = tex2D(_SubTex, uv) * _MainColor;
                float time = max(_Time.y - _Delay, 0) * _AnimSpeed;
                color = (time) % 2 > (1 + _Ratio) ? color : subColor;
            #endif
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
