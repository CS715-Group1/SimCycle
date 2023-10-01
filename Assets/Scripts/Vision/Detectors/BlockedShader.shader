Shader "Custom/BlockedShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float _DepthThreshold = 0.01; // Adjust this value as needed.

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Use the camera depth texture to determine visibility.
                half depth = tex2D(_CameraDepthTexture, i.uv).r;
                float worldDepth = length(i.worldPos - _WorldSpaceCameraPos);

                // Check if the depth difference exceeds a threshold.
                if (abs(depth - worldDepth) < _DepthThreshold)
                {
                    discard; // Discard fragments that are not visible.
                }

                return half4(1, 1, 1, 1); // Render the visible parts.
            }
            ENDCG
        }
    }
}
