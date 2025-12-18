using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.GPUDrivenBRG.ShaderMetadata;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Batching;

public static class GPUDrivenMaterialUniquenessKeyExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetBreakingPropertiesCount(this in GPUDrivenMaterialUniquenessKey key)
	{
		if (!key.BatchBreakingProperties.IsCreated)
		{
			return 0;
		}
		return key.BatchBreakingProperties.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	public static bool RequiresCustomSorting(this in GPUDrivenMaterialUniquenessKey key)
	{
		return (key.ShaderTypeMask & GPUDrivenShaderMetadata.TypeFlags.DecalAny) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	public static bool HasMotionVectorsPassEnabled(this in GPUDrivenMaterialUniquenessKey key)
	{
		int motionVectorsPassIndex = key.MotionVectorsPassIndex;
		if (motionVectorsPassIndex != -1)
		{
			return (key.EnabledPassesMask & (1 << motionVectorsPassIndex)) != 0;
		}
		return false;
	}
}
