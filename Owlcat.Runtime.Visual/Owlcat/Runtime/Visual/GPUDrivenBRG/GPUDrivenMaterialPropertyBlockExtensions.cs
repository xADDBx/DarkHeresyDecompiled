using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenMaterialPropertyBlockExtensions
{
	public static void GetPropertyBlock([NotNull] this Renderer renderer, [NotNull] GPUDrivenMaterialPropertyBlock propertyBlock, GPUDrivenMaterialPropertyBlock.Mode mode = GPUDrivenMaterialPropertyBlock.Mode.Auto)
	{
		if (propertyBlock == null)
		{
			throw new ArgumentNullException("propertyBlock");
		}
		GPUDrivenRenderer gpuDrivenRenderer = null;
		propertyBlock.Get(renderer, ref gpuDrivenRenderer, mode);
	}

	public static void GetPropertyBlock([NotNull] this Renderer renderer, [NotNull] GPUDrivenMaterialPropertyBlock propertyBlock, [CanBeNull] ref GPUDrivenRenderer gpuDrivenRenderer, GPUDrivenMaterialPropertyBlock.Mode mode = GPUDrivenMaterialPropertyBlock.Mode.Auto)
	{
		if (propertyBlock == null)
		{
			throw new ArgumentNullException("propertyBlock");
		}
		propertyBlock.Get(renderer, ref gpuDrivenRenderer, mode);
	}

	public static void SetPropertyBlock([NotNull] this Renderer renderer, [NotNull] GPUDrivenMaterialPropertyBlock propertyBlock, GPUDrivenMaterialPropertyBlock.Mode mode = GPUDrivenMaterialPropertyBlock.Mode.Auto)
	{
		if (propertyBlock == null)
		{
			throw new ArgumentNullException("propertyBlock");
		}
		GPUDrivenRenderer gpuDrivenRenderer = null;
		propertyBlock.Set(renderer, ref gpuDrivenRenderer, mode);
	}

	public static void SetPropertyBlock([NotNull] this Renderer renderer, [NotNull] GPUDrivenMaterialPropertyBlock propertyBlock, [CanBeNull] ref GPUDrivenRenderer gpuDrivenRenderer, GPUDrivenMaterialPropertyBlock.Mode mode = GPUDrivenMaterialPropertyBlock.Mode.Auto)
	{
		if (propertyBlock == null)
		{
			throw new ArgumentNullException("propertyBlock");
		}
		propertyBlock.Set(renderer, ref gpuDrivenRenderer, mode);
	}
}
