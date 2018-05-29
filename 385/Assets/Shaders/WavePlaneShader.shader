Shader "Custom/WavePlaneShader"
{
	SubShader
	{
		Pass
		{
			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _CameraDepthTexture;

			struct vertexInput
			{
				float4 vertex : POSITION;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float4 screenPos : TEXCOORD1;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				output.pos = UnityObjectToClipPos(input.vertex);
				output.screenPos = ComputeScreenPos(output.pos);

				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				input.pos.xy = floor(input.pos.xy * 0.15) * 0.5;
				float checker = -frac(input.pos.r + input.pos.g);

				clip(checker);

				fixed4 c = float4(255.0, 0, 255.0, 1) * _CosTime.a;
				return c;
/*
				float depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, input.screenPos);
				float depth = LinearEyeDepth(depthSample).r;
				float4 foamLine = float4(depth, depth, depth, 1);

				return foamLine;*/
			}
			ENDCG
		}
	}
}