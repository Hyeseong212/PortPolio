Shader "Draw/OutlineShaderClicked" {
    Properties{
        _OutlineColor("Outline Color", Color) = (1,0,0,1)
        _Outline("Outline width", Range(0, 1)) = .025
    }

        CGINCLUDE
#include "UnityCG.cginc"

        struct appdata {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
    };

    struct v2f {
        float4 pos : POSITION;
        float4 color : COLOR;
    };

    uniform float _Outline;
    uniform float4 _OutlineColor;

    v2f vert(appdata v) {
        v2f o;

        // 노멀 벡터를 따라 정점을 확장하여 아웃라인 효과를 줍니다.
        float3 norm = normalize(v.normal);
        v.vertex.xyz += norm * _Outline;

        o.pos = UnityObjectToClipPos(v.vertex);
        o.color = _OutlineColor;
        return o;
    }
    ENDCG

        SubShader{
            Tags { "DisableBatching" = "True" }
            Pass {
                Name "OUTLINE"
                Tags {"LightMode" = "Always" }
                Cull Front
                ZWrite On
                ColorMask RGB
                Blend SrcAlpha OneMinusSrcAlpha

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                half4 frag(v2f i) :COLOR { return i.color; }
                ENDCG
            }
    }

        Fallback "Diffuse"
}
