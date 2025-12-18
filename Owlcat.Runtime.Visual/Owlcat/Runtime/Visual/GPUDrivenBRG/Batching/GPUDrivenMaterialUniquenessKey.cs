using System;
using Owlcat.Runtime.Visual.GPUDrivenBRG.ShaderMetadata;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Batching;

[BurstCompile]
public struct GPUDrivenMaterialUniquenessKey : IEquatable<GPUDrivenMaterialUniquenessKey>
{
	public GPUDrivenShaderMetadata.TypeFlags ShaderTypeMask;

	public BitMask256 EnabledKeywordsMask;

	public int ShaderInstanceID;

	public int EnabledPassesMask;

	public int ShadowCasterPassIndex;

	public int DepthOnlyPassIndex;

	public int MotionVectorsPassIndex;

	public int RenderQueue;

	public GPUDrivenRenderingUtils.RenderType RenderType;

	public UnsafeList<GPUDrivenBatchBreakingProperty> BatchBreakingProperties;

	public BitMask256 IgnoredBatchBreakingProperties;

	public int BatchBreakingPropertiesHashCode;

	public BitMask256 EnabledShadowCasterKeywordsMask;

	public BitMask256 EnabledDepthOnlyKeywordsMask;

	public BitMask256 IgnoredShadowCasterBatchBreakingProperties;

	public BitMask256 IgnoredDepthOnlyBreakingProperties;

	public int ShadowCasterBreakingPropertiesHashCode;

	public int DepthOnlyBreakingPropertiesHashCode;

	public bool Equals(GPUDrivenMaterialUniquenessKey other)
	{
		if (ShaderTypeMask == other.ShaderTypeMask && ShaderInstanceID == other.ShaderInstanceID && EnabledKeywordsMask.Equals(other.EnabledKeywordsMask) && EnabledPassesMask == other.EnabledPassesMask && ShadowCasterPassIndex == other.ShadowCasterPassIndex && DepthOnlyPassIndex == other.DepthOnlyPassIndex && MotionVectorsPassIndex == other.MotionVectorsPassIndex && RenderQueue == other.RenderQueue && RenderType == other.RenderType && IgnoredBatchBreakingProperties.Equals(other.IgnoredBatchBreakingProperties) && BatchBreakingPropertiesHashCode == other.BatchBreakingPropertiesHashCode)
		{
			return BreakingPropertiesAreTheSame(in this, in other);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is GPUDrivenMaterialUniquenessKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(HashCode.Combine(ShaderTypeMask, ShaderInstanceID, EnabledKeywordsMask, EnabledPassesMask, ShadowCasterPassIndex, DepthOnlyPassIndex, MotionVectorsPassIndex), RenderQueue, RenderType, IgnoredBatchBreakingProperties, BatchBreakingPropertiesHashCode);
	}

	private static bool BreakingPropertiesAreTheSame(in GPUDrivenMaterialUniquenessKey key1, in GPUDrivenMaterialUniquenessKey key2)
	{
		if (!key1.BatchBreakingProperties.IsCreated || !key2.BatchBreakingProperties.IsCreated)
		{
			return key1.BatchBreakingProperties.IsCreated == key2.BatchBreakingProperties.IsCreated;
		}
		if (key1.BatchBreakingProperties.Length != key2.BatchBreakingProperties.Length)
		{
			return false;
		}
		BitMask256 ignoredBatchBreakingProperties = key1.IgnoredBatchBreakingProperties;
		BitMask256 ignoredBatchBreakingProperties2 = key2.IgnoredBatchBreakingProperties;
		for (int i = 0; i < key1.BatchBreakingProperties.Length; i++)
		{
			if ((!ignoredBatchBreakingProperties.GetBit(i) || !ignoredBatchBreakingProperties2.GetBit(i)) && !key1.BatchBreakingProperties[i].Equals(key2.BatchBreakingProperties[i]))
			{
				return false;
			}
		}
		return true;
	}

	public int CompareTo(GPUDrivenMaterialUniquenessKey other, bool approximate)
	{
		int num = RenderQueue.CompareTo(other.RenderQueue);
		if (num != 0)
		{
			return num;
		}
		int renderType = (int)RenderType;
		int num2 = renderType.CompareTo((int)other.RenderType);
		if (num2 != 0)
		{
			return num2;
		}
		renderType = (int)ShaderTypeMask;
		int num3 = renderType.CompareTo((int)other.ShaderTypeMask);
		if (num3 != 0)
		{
			return num3;
		}
		int num4 = ShaderInstanceID.CompareTo(other.ShaderInstanceID);
		if (num4 != 0)
		{
			return num4;
		}
		int num5 = EnabledKeywordsMask.CompareTo(other.EnabledKeywordsMask);
		if (num5 != 0)
		{
			return num5;
		}
		int num6 = EnabledPassesMask.CompareTo(other.EnabledPassesMask);
		if (num6 != 0)
		{
			return num6;
		}
		int num7 = ShadowCasterPassIndex.CompareTo(other.ShadowCasterPassIndex);
		if (num7 != 0)
		{
			return num7;
		}
		int num8 = DepthOnlyPassIndex.CompareTo(other.DepthOnlyPassIndex);
		if (num8 != 0)
		{
			return num8;
		}
		int num9 = MotionVectorsPassIndex.CompareTo(other.MotionVectorsPassIndex);
		if (num9 != 0)
		{
			return num9;
		}
		int num10 = BatchBreakingPropertiesHashCode.CompareTo(other.BatchBreakingPropertiesHashCode);
		if (num10 != 0)
		{
			return num10;
		}
		int num11 = IgnoredBatchBreakingProperties.CompareTo(other.IgnoredBatchBreakingProperties);
		if (num11 != 0)
		{
			return num11;
		}
		int breakingPropertiesCount = this.GetBreakingPropertiesCount();
		int breakingPropertiesCount2 = other.GetBreakingPropertiesCount();
		int num12 = breakingPropertiesCount.CompareTo(breakingPropertiesCount2);
		if (num12 != 0)
		{
			return num12;
		}
		if (!approximate && breakingPropertiesCount > 0)
		{
			for (int i = 0; i < breakingPropertiesCount; i++)
			{
				GPUDrivenBatchBreakingProperty gPUDrivenBatchBreakingProperty = BatchBreakingProperties[i];
				GPUDrivenBatchBreakingProperty other2 = other.BatchBreakingProperties[i];
				if (!IgnoredBatchBreakingProperties.GetBit(gPUDrivenBatchBreakingProperty.PropertyIndex) || !other.IgnoredBatchBreakingProperties.GetBit(other2.PropertyIndex))
				{
					int num13 = gPUDrivenBatchBreakingProperty.CompareTo(other2);
					if (num13 != 0)
					{
						return num13;
					}
				}
			}
		}
		return 0;
	}
}
