Shader "Custom/Mobile/Alpha Blended Alpha" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Particle Texture", 2D) = "white" {}
	}
    // Category is a logical grouping of commands, mostly used to "inherit" rendering state. For example,
    // your shader might have multiple subshaders, and each of them requires 'fog' to be off, 'blending' 
    // set to additive, etc:
    // Category {
    //      Fog { Mode Off }
    //      Blend One One
    //      SubShader { }
    //      SubShader { }    
    //}
	SubShader {
        // SubShaders use Tags to tell how and when they expect to be rendered by the rendering engine
        // e.g. "Queue"="Transparent" will make the object be rendered after all opaque objects
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
        Blend SrcAlpha OneMinusSrcAlpha
		LOD 100
        
        // Syntax: BindChannels { Bind "source", target }
        // Specifies that vertex data 'source' maps to hardware 'target'
        // Where 'source' can be:
            // Vertex : vertex position
            // Normal: vertex normal
            // Tangent: veretx tangent
            // Texcoord: primary UV coordinate
            // Texcoord1: secondary UV coordinate
            // Color: vertex color
        // Where 'target' can be:
            //Vertex: vertex position
            //Normal: vertex normal
            //Tangent: vertex tangent
            //Texcoord0, Texcoord1, ... texture coordinates for corresponding texture stage
            //Texcoord: texture coordinates for all texture stages
            //Color: vertex color
        // Note: 'source' and 'target' must match for Vertex, Normal, Tangent, and Color
        // There are two typical use cases for BindChannels:
        // 1) Shaders that taken vertex colors into account
        // 2) Shaders that use two UV sets
        
        BindChannels {
            Bind "Color", color
            Bind "Vertex", vertex
            Bind "TexCoord", texcoord
        }
        
		Pass {
            // Passes use Tags to tell how and when they expect to be rendered by the rendering engine
		    //Tags {}
        
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                // Include Unity Helper Functions
                #include "UnityCG.cginc"

                struct appData {
                    float4 vertex : POSITION;
                    float2 texCoord : TEXCOORD0;
                };
                struct v2f {
                    float4 pos : SV_POSITION;
                    float2 texCoord : TEXCOORD0;  
                };
                
                // Re-Declare 'Properties' within CG PROGRAM
                sampler2D _MainTex;
                fixed4 _Color;

                v2f vert(appData IN) {
                    v2f OUT;
                    OUT.pos = mul(UNITY_MATRIX_MVP, IN.vertex);
                    OUT.texCoord = IN.texCoord;
                    return OUT;
                }
                fixed4 frag (v2f IN) : SV_TARGET
                {
                    // 'Albedo' the word, comes from a texture tinted by color
                    fixed4 c = tex2D (_MainTex, IN.texCoord) * _Color;
                           //c.a = c.a * _Color.a;
                    
                    return c;
                }
            ENDCG
        }  // End Pass
	}
	FallBack Off
}
