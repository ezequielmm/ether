// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/PathShader"
{
    Properties
    {
        [HideInInspector]
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _Offset("Upper Edge Offset", Float) = 0
        _EdgeColor("Upper Edge Color", Color) = (0.8, 0.8, 0.8, 1)
        _UpperBlend("Upper Blend", Float) = 1

        _UnderOffset("Lower Edge Offset", Float) = 0
        _UnderEdgeColor("Lower Edge Color", Color) = (0.8, 0.8, 0.8, 1)
        _UnderBlend("Lower Blend", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        ZWrite Off

        CGPROGRAM
        #pragma surface surf NoLighting alpha:auto

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
            float4 color : COLOR;
        };

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        half4 _Color;
        half4 _RendererColor;
        half4 _EdgeColor;
        float _Offset;
        float _UnderOffset;
        half4 _UnderEdgeColor;
        float _UpperBlend;
        float _UnderBlend;

        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            fixed4 c;
            c.rgb = s.Albedo;
            c.a = s.Alpha;
            return c;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {


            fixed4 normalTex = tex2D(_MainTex, IN.uv_MainTex);
            float2 uv = IN.uv_MainTex;
            fixed4 offsetTex = tex2D(_MainTex, float2(uv.x, uv.y + _Offset));
            fixed4 lowerOffsetTex = tex2D(_MainTex, float2(uv.x, uv.y - _UnderOffset));

            
            fixed upperA = clamp(normalTex.a - offsetTex.a, 0, 1);
            fixed4 upperEdge = _EdgeColor * (1, 1, 1, upperA) * clamp(normalTex * _UpperBlend, 0, 1);

            fixed lowerA = clamp(normalTex.a - lowerOffsetTex.a, 0, 1);
            fixed4 lowerEdge = _UnderEdgeColor * (1, 1, 1, lowerA) * clamp(normalTex * _UnderBlend, 0, 1);

            fixed normalA = 1 - (upperA + lowerA);

            normalTex = (normalTex * _Color * _RendererColor) * (1,1,1, normalA);

            fixed4 c = normalTex + upperEdge + lowerEdge;

            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }

    Fallback "Sprite/Default"
}