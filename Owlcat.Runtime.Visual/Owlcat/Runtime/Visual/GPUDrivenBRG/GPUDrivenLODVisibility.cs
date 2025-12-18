using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations.GPUDriven;
using Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

internal static class GPUDrivenLODVisibility
{
	public const float kInvisible = 0f;

	public const float kFullyVisible = 1f;

	private unsafe static float ApplyLODBias(in GPUDrivenLODGroupData lodGroupData, float bias, float sqrDistance)
	{
		if (bias == 0f)
		{
			return sqrDistance;
		}
		int num = (int)bias;
		float start = ((num == 0) ? 0f : lodGroupData.Distances[num - 1]);
		float end = lodGroupData.Distances[num];
		float num2 = math.lerp(start, end, bias % 1f);
		float num3 = math.sqrt(sqrDistance) + num2;
		return num3 * num3;
	}

	public unsafe static float Compute(in GPUDrivenVisibilityInfo visibilityInfo, in GPUDrivenCullingContext.LODInfo lodInfo, ref NativeArray<GPUDrivenLODGroupData>.ReadOnly lodGroups, NativeArray<GPUDrivenLODViewCollection.ViewDependentLODGroupData>.ReadOnly viewLODGroupData = default(NativeArray<GPUDrivenLODViewCollection.ViewDependentLODGroupData>.ReadOnly))
	{
		if (!GPUDrivenInstanceLODInfo.IsValid(visibilityInfo.PackedLODInstanceInfo))
		{
			return 1f;
		}
		float num = 0f;
		GPUDrivenInstanceLODInfo.Unpack(visibilityInfo.PackedLODInstanceInfo, out var lodGroupIndex, out var lodMask);
		GPUDrivenLODGroupData lodGroupData = lodGroups[lodGroupIndex];
		if (lodInfo.FixedLODIndex >= 0)
		{
			uint num2 = (uint)(1 << math.min(lodGroupData.LODCount - 1, lodInfo.FixedLODIndex));
			if ((lodMask & num2) == 0)
			{
				return 0f;
			}
			return 1f;
		}
		if (lodGroupData.AnimatedCrossFade != 0 && viewLODGroupData.IsCreated)
		{
			float num3 = math.abs(viewLODGroupData[lodGroupIndex].LOD);
			if (!float.IsNaN(num3))
			{
				uint num4 = (uint)((1 << (int)math.floor(num3)) | (1 << (int)math.ceil(num3)));
				if ((lodMask & num4) == 0)
				{
					return 0f;
				}
				return 1f;
			}
		}
		float sqrDistance = (lodInfo.IsOrtho ? lodInfo.SqrScreenRelativeMetric : LODGroupRenderingUtils.CalculateSqrPerspectiveDistance(lodGroupData.WorldSpaceReferencePoint, lodInfo.CameraPosition, lodInfo.SqrScreenRelativeMetric));
		float num5 = math.min(lodGroupData.LODCount - 1, lodInfo.LODBias);
		sqrDistance = ApplyLODBias(in lodGroupData, num5, sqrDistance);
		uint num6 = (uint)(-1 << lodInfo.MaxLOD);
		lodMask &= num6;
		int num7 = math.max(math.tzcnt(lodMask) - 1, (int)((float)lodInfo.MaxLOD + num5));
		for (lodMask >>= num7; lodMask != 0; lodMask >>= 1)
		{
			float num8 = ((num7 == lodInfo.MaxLOD) ? 0f : lodGroupData.Distances[num7 - 1]);
			float num9 = lodGroupData.Distances[num7];
			float num10 = num8 * num8;
			float num11 = num9 * num9;
			if (sqrDistance <= num10)
			{
				break;
			}
			if (sqrDistance < num11)
			{
				GPUDrivenCrossFadeType gPUDrivenCrossFadeType = (GPUDrivenCrossFadeType)((int)lodMask & 3);
				switch (gPUDrivenCrossFadeType)
				{
				case GPUDrivenCrossFadeType.Visible:
					num = 1f;
					break;
				default:
				{
					float num12 = math.sqrt(sqrDistance);
					float num13 = math.sqrt(num11);
					switch (lodGroupData.FadeMode)
					{
					case LODFadeMode.None:
						num = ((gPUDrivenCrossFadeType == GPUDrivenCrossFadeType.CrossFadeOut) ? 1f : 0f);
						break;
					case LODFadeMode.CrossFade:
					{
						float num14 = lodGroupData.TransitionDistances[num7];
						float num15 = num13 - num12;
						if (num15 < num14)
						{
							num = num15 / num14;
							if (gPUDrivenCrossFadeType == GPUDrivenCrossFadeType.CrossFadeIn)
							{
								num = 0f - num;
							}
						}
						else if (gPUDrivenCrossFadeType == GPUDrivenCrossFadeType.CrossFadeOut)
						{
							num = 1f;
						}
						break;
					}
					default:
						num = 0f;
						break;
					}
					break;
				}
				case GPUDrivenCrossFadeType.Disabled:
					break;
				}
				break;
			}
			num7++;
		}
		return num;
	}

	public unsafe static int ComputeDesiredLODIndex(in GPUDrivenCullingContext.LODInfo lodInfo, in GPUDrivenLODGroupData lodGroupData)
	{
		float sqrDistance = (lodInfo.IsOrtho ? lodInfo.SqrScreenRelativeMetric : LODGroupRenderingUtils.CalculateSqrPerspectiveDistance(lodGroupData.WorldSpaceReferencePoint, lodInfo.CameraPosition, lodInfo.SqrScreenRelativeMetric));
		float bias = math.min(lodGroupData.LODCount - 1, lodInfo.LODBias);
		sqrDistance = ApplyLODBias(in lodGroupData, bias, sqrDistance);
		for (int i = lodInfo.MaxLOD; i < lodGroupData.LODCount; i++)
		{
			float num = ((i == lodInfo.MaxLOD) ? 0f : lodGroupData.Distances[i - 1]);
			float num2 = lodGroupData.Distances[i];
			float num3 = num * num;
			float num4 = num2 * num2;
			if (sqrDistance <= num3)
			{
				break;
			}
			if (sqrDistance < num4)
			{
				return i;
			}
		}
		return lodGroupData.LODCount;
	}
}
