﻿Shader "Instanced/Unlit Instanced"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
	}

SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_instancing /// v5.6 will auto add the toggle of "Enable Instancing" Property.
		 
		#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			UNITY_INSTANCE_ID
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			UNITY_INSTANCE_ID
		};

		UNITY_INSTANCING_CBUFFER_START(MyProperties)
		UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
		UNITY_INSTANCING_CBUFFER_END

		v2f vert(appdata v)
		{
			v2f o;

			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_TRANSFER_INSTANCE_ID(v, o);

			o.vertex = UnityObjectToClipPos(v.vertex);
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			UNITY_SETUP_INSTANCE_ID(i);
			return UNITY_ACCESS_INSTANCED_PROP(_Color);
		}
			ENDCG
		}
	}
	/// use Fallback supports cast shadow, 
	/// however other fall back passes will mess up the GPU instancing,
	/// creates a lot of draw calls. So Fallback is off for this testing shader.
	//Fallback "Legacy Shaders/VertexLit"
}
