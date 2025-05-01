Shader "Custom/CosmicBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 0.1)) = 0.01
        _CosmicInfluence ("Cosmic Influence", Range(0, 1)) = 0
        _VoidColor ("Void Color", Color) = (0.1, 0, 0.2, 1)
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
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float _BlurSize;
            float _CosmicInfluence;
            float4 _VoidColor;
            float4 _MainTex_ST;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            // Radial blur function
            fixed4 radialBlur(float2 uv, float time)
            {
                // Calculate direction from center
                float2 center = float2(0.5, 0.5);
                float2 toCenter = center - uv;
                float dist = length(toCenter);
                
                // Normalize direction
                float2 dir = normalize(toCenter);
                
                // Apply radial blur
                float blurAmount = _BlurSize * (1.0 + _CosmicInfluence);
                
                // Pulse the blur over time for a more dynamic effect
                blurAmount *= (sin(time * 0.5) * 0.3 + 0.7);
                
                // Increase blur at the edges
                blurAmount *= smoothstep(0.0, 0.7, dist);
                
                // Sample multiple times along the direction to center
                fixed4 color = fixed4(0, 0, 0, 0);
                float totalWeight = 0;
                
                for (int i = 0; i < 10; i++)
                {
                    float weight = 1.0 - (i / 10.0);
                    float2 offset = dir * blurAmount * (i / 10.0);
                    color += tex2D(_MainTex, uv + offset) * weight;
                    totalWeight += weight;
                }
                
                return color / totalWeight;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y;
                
                // Apply radial blur
                fixed4 col = radialBlur(i.uv, time);
                
                // Add cosmic glow effect proportional to influence
                float2 center = float2(0.5, 0.5);
                float dist = length(center - i.uv);
                float edgeGlow = smoothstep(0.7, 1.0, dist) * _CosmicInfluence;
                
                // Add the edge glow with cosmic color
                col = lerp(col, _VoidColor, edgeGlow * 0.5);
                
                // Add chromatic aberration
                if (_CosmicInfluence > 0.3)
                {
                    float aberrationAmount = _BlurSize * _CosmicInfluence * 3.0;
                    float2 dir = normalize(center - i.uv);
                    
                    // Sample red and blue channels with offset
                    float2 redOffset = i.uv + dir * aberrationAmount;
                    float2 blueOffset = i.uv - dir * aberrationAmount;
                    
                    col.r = lerp(col.r, tex2D(_MainTex, redOffset).r, _CosmicInfluence * 0.7);
                    col.b = lerp(col.b, tex2D(_MainTex, blueOffset).b, _CosmicInfluence * 0.7);
                }
                
                return col;
            }
            ENDCG
        }
    }
}