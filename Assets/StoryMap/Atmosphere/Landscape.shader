Shader "Tuman/LivingLandscape"
{
    Properties { _MainTex("Earth atlas",2D)="white"{} _TileRect("Earth rectangle",Vector)=(0,0,1,1) }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;float4 _TileRect;
            struct In {float4 vertex:POSITION;float4 color:COLOR;};
            struct Out {float4 pos:SV_POSITION;float4 color:COLOR;float2 world:TEXCOORD0;};
            Out vert(In v){Out o;o.pos=UnityObjectToClipPos(v.vertex);o.color=v.color;o.world=mul(unity_ObjectToWorld,v.vertex).xy;return o;}
            float4 frag(Out i):SV_Target
            {
                float3 c;
                if(i.color.b>i.color.r*1.15)
                {
                    float wave=sin(i.world.y*5+sin(i.world.x*3)+_Time.y*.8)*.5+.5;
                    float sheen=pow(saturate(sin(i.world.y*21+i.world.x*3+_Time.y*1.8)),20)*.025;
                    c=GammaToLinearSpace(i.color.rgb*(.9+wave*.13)+sheen);
                }
                else
                {
                    float2 uv=_TileRect.xy+(.01+frac(i.world*.65)*.98)*_TileRect.zw;
                    float3 earth=tex2D(_MainTex,uv).rgb;
                    c=earth*(.65+i.color.rgb*1.3);
                }
                return float4(c,i.color.a);
            }
            ENDHLSL
        }
    }
}
