using System;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Unity.Burst;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.ShaderMetadata;

[Serializable]
public struct GPUDrivenShaderMetadata : IEquatable<GPUDrivenShaderMetadata>
{
	[Serializable]
	public struct PassMetadata : IEquatable<PassMetadata>
	{
		public string LightMode;

		public BitMask256 LocalKeywordsMask;

		public BitMask256 IgnoredBatchBreakingProperties;

		public BitMask256 IgnoredBreakingPropertiesNoAlphaTest;

		public bool Equals(PassMetadata other)
		{
			if (LightMode == other.LightMode && LocalKeywordsMask.Equals(other.LocalKeywordsMask) && IgnoredBatchBreakingProperties.Equals(other.IgnoredBatchBreakingProperties))
			{
				return IgnoredBreakingPropertiesNoAlphaTest.Equals(other.IgnoredBreakingPropertiesNoAlphaTest);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is PassMetadata other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(LightMode, LocalKeywordsMask, IgnoredBatchBreakingProperties, IgnoredBreakingPropertiesNoAlphaTest);
		}
	}

	[Flags]
	public enum SupportFlags
	{
		None = 0,
		SRPBatcher = 1,
		DOTSInstancing = 2,
		Everything = 3
	}

	[Flags]
	public enum TypeFlags
	{
		None = 0,
		DecalDeferred = 1,
		DecalGUI = 2,
		DecalAny = 3
	}

	[Serializable]
	public struct BatchBreakingPropertiesMetadata : IEquatable<BatchBreakingPropertiesMetadata>
	{
		public BitMask256 Mask;

		public int Count;

		public bool Equals(BatchBreakingPropertiesMetadata other)
		{
			if (Mask.Equals(other.Mask))
			{
				return Count == other.Count;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is BatchBreakingPropertiesMetadata other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Mask, Count);
		}
	}

	public string ShaderName;

	public string ShaderGraphGuid;

	public SupportFlags Support;

	[NonSerialized]
	public int ShadowCasterPassIndex;

	[NonSerialized]
	public int DepthOnlyPassIndex;

	[NonSerialized]
	public int MotionVectorsPassIndex;

	public string[] LocalKeywordNames;

	[NonSerialized]
	public LocalKeyword[] LocalKeywords;

	public PassMetadata[] Passes;

	public TypeFlags TypeMask;

	public BatchBreakingPropertiesMetadata BatchBreakingProperties;

	public BitMask256 VirtualTexturePropertyMask;

	[BurstDiscard]
	public bool Equals(GPUDrivenShaderMetadata other)
	{
		if (ShaderName == other.ShaderName && Support == other.Support && ShadowCasterPassIndex == other.ShadowCasterPassIndex && DepthOnlyPassIndex == other.DepthOnlyPassIndex && MotionVectorsPassIndex == other.MotionVectorsPassIndex && GPUDrivenEqualityUtils.AllStringsAreEqual(LocalKeywordNames, other.LocalKeywordNames) && GPUDrivenEqualityUtils.AllItemsAreEqual(LocalKeywords, other.LocalKeywords) && GPUDrivenEqualityUtils.AllItemsAreEqual(Passes, other.Passes) && BatchBreakingProperties.Equals(other.BatchBreakingProperties))
		{
			return VirtualTexturePropertyMask.Equals(other.VirtualTexturePropertyMask);
		}
		return false;
	}

	[BurstDiscard]
	public override bool Equals(object obj)
	{
		if (obj is GPUDrivenShaderMetadata other)
		{
			return Equals(other);
		}
		return false;
	}

	[BurstDiscard]
	public override int GetHashCode()
	{
		return HashCode.Combine(ShaderName, (int)Support, HashCode.Combine(ShadowCasterPassIndex, DepthOnlyPassIndex, MotionVectorsPassIndex), GPUDrivenEqualityUtils.GetHashCode(LocalKeywordNames), GPUDrivenEqualityUtils.GetHashCode(LocalKeywords), GPUDrivenEqualityUtils.GetHashCode(Passes), HashCode.Combine(BatchBreakingProperties, VirtualTexturePropertyMask));
	}

	public void ResetUnserializedFields()
	{
		ShadowCasterPassIndex = (DepthOnlyPassIndex = (MotionVectorsPassIndex = -1));
		LocalKeywords = Array.Empty<LocalKeyword>();
	}
}
