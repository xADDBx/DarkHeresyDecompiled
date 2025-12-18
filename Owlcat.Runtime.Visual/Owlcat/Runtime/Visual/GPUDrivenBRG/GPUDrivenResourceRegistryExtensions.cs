using System.Runtime.CompilerServices;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenResourceRegistryExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref GPUDrivenResourceRegistry.UnmanagedMeshInfo GetUnmanagedMeshInfo(this GPUDrivenResourceRegistry resourceRegistry, in GPUDrivenRendererGroupPool.RendererGroup rendererGroup)
	{
		return ref resourceRegistry.GetUnmanagedMeshInfo(in rendererGroup.Key);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref GPUDrivenResourceRegistry.UnmanagedMeshInfo GetUnmanagedMeshInfo(this GPUDrivenResourceRegistry resourceRegistry, in GPUDrivenRendererGroupPool.RendererGroupKey rendererGroupKey)
	{
		return ref resourceRegistry.GetUnmanagedMeshInfo(rendererGroupKey.MeshAllocation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref GPUDrivenResourceRegistry.UnmanagedMaterialInfo GetUnmanagedMaterialInfo(this GPUDrivenResourceRegistry resourceRegistry, in GPUDrivenRendererGroupPool.RendererGroup rendererGroup)
	{
		return ref resourceRegistry.GetUnmanagedMaterialInfo(in rendererGroup.Key);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref GPUDrivenResourceRegistry.UnmanagedMaterialInfo GetUnmanagedMaterialInfo(this GPUDrivenResourceRegistry resourceRegistry, in GPUDrivenRendererGroupPool.RendererGroupKey rendererGroupKey)
	{
		return ref resourceRegistry.GetUnmanagedMaterialInfo(rendererGroupKey.MaterialAllocation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref GPUDrivenResourceRegistry.ManagedMeshInfo GetManagedMeshInfo(this GPUDrivenResourceRegistry resourceRegistry, in GPUDrivenRendererGroupPool.RendererGroup rendererGroup)
	{
		return ref resourceRegistry.GetManagedMeshInfo(in rendererGroup.Key);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref GPUDrivenResourceRegistry.ManagedMeshInfo GetManagedMeshInfo(this GPUDrivenResourceRegistry resourceRegistry, in GPUDrivenRendererGroupPool.RendererGroupKey rendererGroupKey)
	{
		return ref resourceRegistry.GetManagedMeshInfo(rendererGroupKey.MeshAllocation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref GPUDrivenResourceRegistry.ManagedMaterialInfo GetManagedMaterialInfo(this GPUDrivenResourceRegistry resourceRegistry, in GPUDrivenRendererGroupPool.RendererGroup rendererGroup)
	{
		return ref resourceRegistry.GetManagedMaterialInfo(in rendererGroup.Key);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref GPUDrivenResourceRegistry.ManagedMaterialInfo GetManagedMaterialInfo(this GPUDrivenResourceRegistry resourceRegistry, in GPUDrivenRendererGroupPool.RendererGroupKey rendererGroupKey)
	{
		return ref resourceRegistry.GetManagedMaterialInfo(rendererGroupKey.MaterialAllocation);
	}
}
