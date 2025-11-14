Shader "Custom/GridOverlayHighlight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GridColor ("Grid Line Color", Color) = (0,0,0,1)
        _GridThickness ("Line Thickness", Range(0.001, 0.1)) = 0.01
        _GridCount ("Grid Cells (X,Y)", Vector) = (10, 10, 0, 0)

        // Hover properties
        _HoveredCell ("Hovered Cell (X,Y)", Vector) = (0,0,0,0)
        _HoverColor ("Hover Color", Color) = (0, 1, 0, 0.3)
        _BlockedColor ("Blocked Color", Color) = (1, 0, 0, 0.3)
        _CanPlace ("Can Place?", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;

        float4 _GridColor;
        float4 _HoverColor;
        float4 _BlockedColor;

        float _GridThickness;
        float4 _GridCount;
        float4 _HoveredCell;
        float _CanPlace;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;
            float4 baseCol = tex2D(_MainTex, uv);

            float2 gridUV = uv * _GridCount.xy;
            float2 g = frac(gridUV);

            float lineX = step(g.x, _GridThickness) + step(1.0 - g.x, _GridThickness);
            float lineY = step(g.y, _GridThickness) + step(1.0 - g.y, _GridThickness);
            float lineMask = saturate(lineX + lineY);

            float4 color = lerp(baseCol, _GridColor, lineMask);

            float2 cell = floor(gridUV);

            bool isHovered =
                (cell.x == _HoveredCell.x) &&
                (cell.y == _HoveredCell.y);

            if (isHovered)
            {
                float4 highlight = (_CanPlace > 0.5) ? _HoverColor : _BlockedColor;
                color = lerp(color, highlight, highlight.a);
            }

            o.Albedo = color.rgb;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
