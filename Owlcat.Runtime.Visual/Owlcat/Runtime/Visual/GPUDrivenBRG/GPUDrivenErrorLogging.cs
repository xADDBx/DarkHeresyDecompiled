using System.Runtime.CompilerServices;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

internal static class GPUDrivenErrorLogging
{
	private const int kMaxHeavyLoggingCount = 30;

	private const string kPrefix = "GPUDrivenBRG:";

	private static int s_HeavyLoggingFrameIndex = -1;

	private static int s_HeavyLoggingCount;

	public static void FailedToAllocateInstanceIndex(in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc, int submeshIndex)
	{
		Debug.LogError(string.Format("{0} failed to allocate an instance index for {1} (SubmeshIndex={2}). Out of memory.", "GPUDrivenBRG:", rendererDesc.ToString(), submeshIndex), rendererDesc.MeshRenderer);
	}

	public static void FailedToAllocatePersistentData(in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc, int submeshIndex)
	{
		if (TryOutputHeavyLog())
		{
			Debug.LogError(string.Format("{0} failed to allocate persistent data for {1} (SubmeshIndex={2}). Out of memory.", "GPUDrivenBRG:", rendererDesc.ToString(), submeshIndex), rendererDesc.MeshRenderer);
		}
	}

	public static void FailedToAllocateMaterial(Material material)
	{
		if (TryOutputHeavyLog())
		{
			Debug.LogError(string.Format("{0} failed to allocate persistent data for {1}. Out of memory.", "GPUDrivenBRG:", material), material);
		}
	}

	private static bool TryOutputHeavyLog()
	{
		int frameCount = Time.frameCount;
		if (frameCount != s_HeavyLoggingFrameIndex)
		{
			s_HeavyLoggingFrameIndex = frameCount;
			s_HeavyLoggingCount = 0;
		}
		s_HeavyLoggingCount++;
		if (s_HeavyLoggingCount == 31)
		{
			Debug.LogError("GPUDrivenBRG: too many errors. The rest for the current frame will be truncated.");
		}
		return s_HeavyLoggingCount <= 30;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GPUDrivenIndexAllocator.IndexAllocation EnsureAllocationValidity(this GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return indexAllocation;
	}
}
