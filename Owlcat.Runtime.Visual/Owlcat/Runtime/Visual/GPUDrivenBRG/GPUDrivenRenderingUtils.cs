using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations.GPUDriven;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenRenderingUtils
{
	[Flags]
	public enum MaterialLayoutChangeFlags
	{
		Nothing = 0,
		DefaultValues = 1,
		Properties = 2,
		TotalSizeIncreased = 8,
		BatchBreakingChanges = 0x10
	}

	public enum RenderType
	{
		Various,
		Opaque,
		TransparentCutout,
		Transparent
	}

	public struct MaterialLayoutChanges
	{
		public MaterialLayoutChangeFlags PerMaterialData;

		public BitMask256 DirtyPerMaterialDataMask;

		public MaterialLayoutChangeFlags PerInstanceData;

		public BitMask256 DirtyPerInstanceDataMask;
	}

	public struct PropertyLayout : IDisposable
	{
		public NativeList<GPUDrivenRenderer.PropertyData> PerMaterialData;

		public int PerMaterialDataSizeInBytes;

		public NativeList<GPUDrivenRenderer.PropertyData> PerInstanceData;

		public int PerInstanceDataSizeInBytes;

		public void Dispose()
		{
			PerMaterialData.Dispose();
			PerMaterialDataSizeInBytes = 0;
			PerInstanceData.Dispose();
			PerInstanceDataSizeInBytes = 0;
		}

		public void FillMemoryCounter(Counters.CounterCollection counters, ProfilerCounterValue<int> counter)
		{
			counters.CollectBufferSize(counter, PerMaterialData);
			counters.CollectBufferSize(counter, PerInstanceData);
		}
	}

	public const string kPerInstancePropertyPrefix = "_PerInstance_";

	public const string kPerMaterialPropertyPrefix = "_PerMaterial_";

	public const bool kAssumeAllPropertiesArePerMaterial = true;

	private static int AlignMaterialPropertySize(int size)
	{
		return Alignment.AlignUp(size, 4);
	}

	public static int GetPropertyDataSize(this GPUDrivenRenderer.PropertyDataType propertyDataType)
	{
		return AlignMaterialPropertySize(propertyDataType switch
		{
			GPUDrivenRenderer.PropertyDataType.Float => 4, 
			GPUDrivenRenderer.PropertyDataType.Int => 4, 
			GPUDrivenRenderer.PropertyDataType.Vector => UnsafeUtility.SizeOf<Vector4>(), 
			GPUDrivenRenderer.PropertyDataType.Color => UnsafeUtility.SizeOf<Color>(), 
			GPUDrivenRenderer.PropertyDataType.Matrix => UnsafeUtility.SizeOf<Matrix4x4>(), 
			_ => throw new ArgumentOutOfRangeException("propertyDataType", propertyDataType, null), 
		});
	}

	public static GPUDrivenBatchRendererGroup.ChangeContext CreateChangeContext()
	{
		GPUDrivenBatchRendererGroup.ChangeContext result = default(GPUDrivenBatchRendererGroup.ChangeContext);
		result.FrameIndex = Time.frameCount;
		return result;
	}

	public static bool IsRendererSupported(in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc)
	{
		if (IsPartOfPreviewScene(in rendererDesc))
		{
			return false;
		}
		if (rendererDesc.MeshRenderer == null)
		{
			return rendererDesc.InstanceID.Type == GPUDrivenInstanceID.InstanceIDType.Custom;
		}
		if (rendererDesc.MeshRenderer.probeAnchor != null)
		{
			return false;
		}
		if (rendererDesc.MeshRenderer.HasPropertyBlock())
		{
			return false;
		}
		if (RendererUtils.AllowGPUDrivenRendering(rendererDesc.MeshRenderer) && !rendererDesc.MeshRenderer.isPartOfStaticBatch)
		{
			return !rendererDesc.MeshRenderer.forceRenderingOff;
		}
		return false;
	}

	public static RenderType GetRenderTypeTagValue(Material material)
	{
		return material.GetTag("RenderType", searchFallbacks: false, "Opaque") switch
		{
			"Opaque" => RenderType.Opaque, 
			"TransparentCutout" => RenderType.TransparentCutout, 
			"Transparent" => RenderType.Transparent, 
			_ => RenderType.Various, 
		};
	}

	public static Vector4 GetTilingOffset(Material material, int textureNameID)
	{
		Vector2 textureScale = material.GetTextureScale(textureNameID);
		Vector2 textureOffset = material.GetTextureOffset(textureNameID);
		return new Vector4(textureScale.x, textureScale.y, textureOffset.x, textureOffset.y);
	}

	public unsafe static void CreateOrResizeNativeArray<T>(ref NativeArray<T> nativeArray, int size, Allocator allocator, NativeArrayOptions nativeArrayOptions = NativeArrayOptions.ClearMemory) where T : unmanaged
	{
		if (!nativeArray.IsCreated)
		{
			nativeArray = new NativeArray<T>(size, allocator, nativeArrayOptions);
			return;
		}
		int length = nativeArray.Length;
		ArrayExtensions.ResizeArray(ref nativeArray, size);
		if (length != size && nativeArrayOptions == NativeArrayOptions.ClearMemory)
		{
			int num = (size - length) * UnsafeUtility.SizeOf<T>();
			UnsafeUtility.MemSet((byte*)nativeArray.GetUnsafePtr() + (nint)length * (nint)sizeof(T), 0, num);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRendererEnabled(MeshRenderer meshRenderer)
	{
		return IsRendererEnabled(meshRenderer, meshRenderer.gameObject);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRendererEnabled(MeshRenderer meshRenderer, GameObject gameObject)
	{
		if (meshRenderer.enabled)
		{
			return gameObject.activeInHierarchy;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsPartOfPreviewScene(in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc)
	{
		return false;
	}
}
