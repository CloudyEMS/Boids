Shader "MetaBall/Toon"
{
	Properties
	{
		// ******************************************************************************************** //
		// VARIABLE NAME		NAME IN PROPERTY WINDOW			EDITOR BOX			DEFAULT VALUE		//
		// ******************************************************************************************** //

		// Diffuse
		_Color 					("Tint", 						Color)			= 	(0.2, 0.3, 0.4, 1)
		_Color2					("Intersection Color",	 		Color)			= 	(0.2, 0.3, 0.4, 1)
		_DiffusePower 			("Intersection Power", 			Range(0, 2)) 	= 	3.0
		_DiffuseLevels	 		("DiffuseLevels", 				int) 			= 	3
		// Specular
		_SpecColor 				("Specular", 					Color) 			=	(0.4, 0.5, 0.6, 1)
		_SpecPower 				("Specular Power", 				Float) 			=	3.0
		_SpecAtten 				("Specular Attenuation", 		Range(0,1)) 	= 	1.0
		_SpecThreshold			("Specular Threshold",			Range(0,1))		=	0.4
		// Edge
		_EdgeColor		 		("Edge Color", 					Color) 			=	(0, 0, 0, 1)
		_EdgeThreshold			("Edge Threshold",				Range(0,1))		= 	0.2
		// Radial Basis Function
		_K 						("RadialBasis Constant", 		Float) 			= 	7.0
		_Threshold 				("Isosurface Threshold",		Range(0,1)) 	= 	0.5
		_Epsilon 				("Normal Epsilon", 				Range(0,1)) 	= 	0.1

		_KAtten					("RadialBasis Attenuation", 	Range(0, 30))	=	2.0
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Back
		ZWrite Off 			// Don't write on the ZBuffer.
		Lighting Off		// Don't let unity do lighting calculations.

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// SIZE here must be the same as k_size in BoidSpawner.cs
			#define  SIZE 20 


			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 worldPosition : TEXCOORD0;
			};

			uniform float3 _ParticlesPos[SIZE];	// The world position of all particles, feeded with c# script.

			float4 _Color;						// Diffuse Color.
			float4 _Color2;						// Diffuse Color.
			float  _DiffusePower;				// Diffuse Power.
			int _DiffuseLevels;

			float4 _SpecColor;					// Specular Color.
			float _SpecPower;					// Specular Power.
			fixed _SpecAtten;					// Specular Scale.
			fixed _SpecThreshold;				// Specular threshold.

			float4 _EdgeColor;					// Edge Color.
			fixed _EdgeThreshold;				// Edge threshold.

			float _K;							// Radial basis function constant.
			fixed _Threshold;					// Isosurface threshold.
			fixed _Epsilon;						// Epsilon to approximate normal on isosurface.
			float _KAtten;						// Size of the blob.

			float scalarField(float3 pos){
				float density = 0;
				for(int i = 0; i < SIZE; i++){
					float dis = distance(pos, _ParticlesPos[i].xyz);
					density += exp(-_K * (dis));
				}
				density *= _KAtten;
				return density;
			}

			float scalarField(float x, float y, float z){
				float3 pos = float3(x,y,z);
				return scalarField(pos);
			}

			float3 calcNormal(float3 p){
				float3 N;
				N.x = scalarField(p.x + _Epsilon, p.y, p.z) - scalarField(p.x - _Epsilon, p.y, p.z);
				N.y = scalarField(p.x, p.y + _Epsilon, p.z) - scalarField(p.x, p.y - _Epsilon, p.z);
				N.z = scalarField(p.x, p.y, p.z + _Epsilon) - scalarField(p.x, p.y, p.z - _Epsilon);
				return N;
			}

			float3 blinnPhongToon(float3 V, float3 N, float3 L, float s){
				float3 grad = N / (2.0 * _Epsilon);
				float de = s / length(grad); 

				N = normalize(-N);
				float3 H = normalize(V + L);
				float NdotH = saturate(dot(N, H)); // for specular.
				float NdotV = saturate(dot(N, V)); // for fresnel.
				float LdotN = saturate(dot(L, N)); // for diffuse.

				float3 diffuseTerm = _Color + _Color * floor(LdotN * _DiffuseLevels) / _DiffuseLevels;
				float spec = pow(NdotH, _SpecPower);
				float3 specularTerm = spec * _SpecAtten * _SpecColor * (spec > _SpecThreshold ? 1 : 0);
				float3 edge = NdotV > _EdgeThreshold ? 1 : _EdgeColor;

				float3 c1 = diffuseTerm + specularTerm;
				float3 c2 = _DiffusePower * _Color2 * LdotN;
				return edge * (lerp(c1, c2, de));
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				// initial color of the fragment.
				float4 col = _Color;
				col.a = 0;

				// prepare to raycast.
				float3 viewDir = normalize(i.worldPosition - _WorldSpaceCameraPos.xyz); // World space direction from cam to fragment.
				float3 start = i.worldPosition - 2.0 * viewDir; // Start sampling two units before the fragment along the viewDir.
				float3 p; // The sampled point.
				float s; // Scalar field value.

				// We now start sampling on the ray. We will be sampling each 0.2 points along the ray for a maximum length of 3 units.
				// And once we find an intersection with the isosurface, we sample again with a smaller step till we can pinpoint the
				// exact point of intersection.
				for(float i = 0; i < 3.0; i+=0.2){
					p = start + i * viewDir;
					s = scalarField(p); 
					if(s > _Threshold){
						// now, sample each 0.05 from (p - 0.2, p + 0.2)
						float3 start2 = p - 0.2 * viewDir;
						for(float i = 0.05; i < 0.4; i+=0.05){
							p = start2 + i * viewDir;
							s = scalarField(p); 
							if(s > _Threshold){
								// finally, sample each 0.01 from (p - 0.05, p + 0.05)
								float3 start3 = p - 0.05 * viewDir;
								for(float i = 0.01; i < 0.1; i+=0.01){
									p = start3 + i * viewDir;
									s = scalarField(p); 
									if(s > _Threshold){
										break;
									}
								}
								break;
							}
						}

						// ... calculate its normal.
						float3 N = calcNormal(p);
						// ... calculate the lightDir.
						float3 L = normalize(UnityWorldSpaceLightDir(p.xyz)); // the same as normalize(_WorldSpaceLightPos0 - p.xyz);
						// ... illuminate with blinnPhong
						col.xyz = blinnPhongToon(-viewDir, N, L, s);
						// ... make it visible.
						col.a = 1;
						break;
					}
				}

				return col;
			}

			ENDCG
		}
	}
}
