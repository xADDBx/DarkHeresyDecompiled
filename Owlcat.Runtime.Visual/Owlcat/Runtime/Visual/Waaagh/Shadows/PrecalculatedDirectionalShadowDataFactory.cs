using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

internal static class PrecalculatedDirectionalShadowDataFactory
{
	public static void Populate(WaaaghRenderingData renderingData, WaaaghShadowData shadowData, NativeHashMap<int, PrecalculatedDirectionalShadowData> results)
	{
		NativeArray<VisibleLight> visibleLights = renderingData.VisibleLights;
		int length = visibleLights.Length;
		for (int i = 0; i < length; i++)
		{
			VisibleLight visibleLight = visibleLights[i];
			if (visibleLight.lightType == LightType.Directional)
			{
				Light light = visibleLight.light;
				if (!(light == null) && light.shadows != 0)
				{
					results.Add(light.GetInstanceID(), CreateDirectionalShadowData(i, in visibleLight, renderingData, shadowData));
				}
				continue;
			}
			break;
		}
	}

	private static PrecalculatedDirectionalShadowData CreateDirectionalShadowData(int visibleLightIndex, in VisibleLight visibleLight, WaaaghRenderingData renderingData, WaaaghShadowData shadowData)
	{
		PrecalculatedDirectionalShadowData result = default(PrecalculatedDirectionalShadowData);
		int count = shadowData.DirectionalLightCascades.Count;
		Vector3 ratios = shadowData.DirectionalLightCascades.GetRatios();
		int directionalLightCascadeResolution = (int)shadowData.DirectionalLightCascadeResolution;
		float4x4 float4x = visibleLight.localToWorldMatrix;
		for (int i = 0; i < count; i++)
		{
			renderingData.CullResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(visibleLightIndex, i, count, ratios, directionalLightCascadeResolution, shadowData.ShadowNearPlane, out var viewMatrix, out var projMatrix, out var shadowSplitData);
			Matrix4x4 matrix4x = GL.GetGPUProjectionMatrix(projMatrix, renderIntoTexture: true) * viewMatrix;
			float4 faceDirection = -float4x.c2;
			float value = 2f / projMatrix.m00;
			float4 cullingSphere = shadowSplitData.cullingSphere;
			cullingSphere.w *= cullingSphere.w;
			result.FrustumSizeArray[i] = value;
			result.SplitDataArray[i] = shadowSplitData;
			result.FaceDataArray[i] = new ShadowFaceData
			{
				CullingSphere = cullingSphere,
				FaceDirection = faceDirection,
				WorldToShadow = matrix4x
			};
		}
		return result;
	}
}
