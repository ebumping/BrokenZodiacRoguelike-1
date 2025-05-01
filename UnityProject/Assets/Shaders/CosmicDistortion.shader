Shader "Custom/CosmicDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Intensity ("Distortion Intensity", Range(0, 1)) = 0.1
        _Speed ("Animation Speed", Range(0, 5)) = 0.5
        _CosmicInfluence ("Cosmic Influence", Range(0, 1)) = 0
        _TentacleAmount ("Tentacle Amount", Range(0, 10)) = 3
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
            sampler2D _NoiseTex;
            float _Intensity;
            float _Speed;
            float _CosmicInfluence;
            float _TentacleAmount;
            float4 _VoidColor;
            float4 _MainTex_ST;
            
            // Simple noise function
            float noise(float2 uv)
            {
                return tex2D(_NoiseTex, uv).r;
            }
            
            // Fractal Brownian Motion noise
            float fbm(float2 uv)
            {
                float f = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for(int i = 0; i < 5; i++)
                {
                    f += amplitude * noise(uv * frequency);
                    amplitude *= 0.5;
                    frequency *= 2.0;
                    
                    // Rotate the coordinates to reduce directional bias
                    float2x2 rot = float2x2(cos(0.5), sin(0.5), -sin(0.5), cos(0.5));
                    uv = mul(rot, uv);
                }
                
                return f;
            }
            
            // Tentacle-like distortion
            float2 tentacles(float2 uv, float time)
            {
                // Calculate center distance
                float2 center = float2(0.5, 0.5);
                float2 delta = uv - center;
                float dist = length(delta);
                float angle = atan2(delta.y, delta.x);
                
                // Distort based on angle, creating tentacle-like patterns
                float tentacleEffect = sin(angle * _TentacleAmount + time * _Speed) * 0.5 + 0.5;
                
                // Combine with noise for organic movement
                float2 noiseCoord = uv + time * 0.1;
                float noiseVal = fbm(noiseCoord) * 2.0 - 1.0;
                
                // Apply distortion, stronger near center
                float distFactor = smoothstep(1.0, 0.0, dist * 2.0);
                float2 offset = float2(
                    noiseVal * cos(angle) * _Intensity,
                    noiseVal * sin(angle) * _Intensity
                );
                
                // Add tentacle influence
                offset += normalize(delta) * tentacleEffect * _Intensity * 0.5 * distFactor;
                
                // Increase effect based on cosmic influence
                offset *= (1.0 + _CosmicInfluence * 2.0);
                
                return uv + offset;
            }
            
            // Void patches distortion
            float voidEffect(float2 uv, float time)
            {
                // Create void patches that slowly move
                float2 voidUV = uv * 3.0 + float2(sin(time * 0.1), cos(time * 0.13)) * 0.2;
                float voidNoise = fbm(voidUV) * fbm(voidUV * 0.7 + float2(0, time * 0.05));
                
                // Threshold and smooth the effect
                float voidStrength = smoothstep(0.6, 0.9, voidNoise);
                
                // Scale by cosmic influence
                voidStrength *= _CosmicInfluence * 0.7;
                
                return voidStrength;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y;
                
                // Apply cosmic tentacle distortion
                float2 distortedUV = tentacles(i.uv, time);
                
                // Sample the texture with distorted UVs
                fixed4 col = tex2D(_MainTex, distortedUV);
                
                // Apply the void effect
                float void = voidEffect(i.uv, time);
                col = lerp(col, _VoidColor, void);
                
                // Add subtle color aberration for an otherworldly effect
                float2 redOffset = distortedUV + float2(0.01, 0.01) * _Intensity * _CosmicInfluence;
                float2 blueOffset = distortedUV - float2(0.01, 0.01) * _Intensity * _CosmicInfluence;
                
                float redChannel = tex2D(_MainTex, redOffset).r;
                float blueChannel = tex2D(_MainTex, blueOffset).b;
                
                col.r = lerp(col.r, redChannel, _CosmicInfluence * 0.5);
                col.b = lerp(col.b, blueChannel, _CosmicInfluence * 0.5);
                
                // Add subtle pulsing glow effect when cosmic influence is high
                if (_CosmicInfluence > 0.5)
                {
                    float glow = sin(time * 2.0) * 0.5 + 0.5;
                    col += _VoidColor * glow * (_CosmicInfluence - 0.5) * 0.3;
                }
                
                return col;
            }
            ENDCG
        }
    }
}