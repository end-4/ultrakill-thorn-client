Shader "Custom/WireframeShader"
{
    Properties
    {
        _WireframeColor ("Wireframe Color", Color) = (1, 1, 1, 1)
        _FillColor ("Fill Color", Color) = (0, 0, 0, 0.5)
        _WireframeThickness ("Wireframe Thickness", Range(0.001, 10.0)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        // -------------------------------------------------------------
        // Pass 1: Depth Pre-pass
        // Writes depth to prevent back-faces from showing through 
        // and avoids z-sorting glitches across multiple transparent objects.
        // -------------------------------------------------------------
        Pass
        {
            Cull Back
            ZWrite On
            ColorMask 0

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }

        // -------------------------------------------------------------
        // Pass 2: Color Pass
        // Handles alpha blending over the depth written in Pass 1.
        // -------------------------------------------------------------
        Pass
        {
            Cull Back
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 pos : SV_POSITION;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 barycentric : TEXCOORD0;
            };

            fixed4 _WireframeColor;
            fixed4 _FillColor;
            float _WireframeThickness;

            v2g vert (appdata v)
            {
                v2g o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> stream)
            {
                g2f o;

                o.pos = input[0].pos;
                o.barycentric = float3(1, 0, 0);
                stream.Append(o);

                o.pos = input[1].pos;
                o.barycentric = float3(0, 1, 0);
                stream.Append(o);

                o.pos = input[2].pos;
                o.barycentric = float3(0, 0, 1);
                stream.Append(o);
            }

            fixed4 frag (g2f i) : SV_Target
            {
                float3 unitWidth = fwidth(i.barycentric);
                float3 edge = smoothstep(float3(0, 0, 0), unitWidth * _WireframeThickness, i.barycentric);
                float minEdge = min(min(edge.x, edge.y), edge.z);

                return lerp(_WireframeColor, _FillColor, minEdge);
            }
            ENDCG
        }
    }
    FallBack "Transparent/Cutout/VertexLit"
}