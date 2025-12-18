using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenRendererParamsExtensions
{
	public struct ExtractedRendererSettings
	{
		public bool Enabled;

		public Vector3 Scale;

		public GPUDrivenBatchRendererGroup.GeneralRendererSettings General;
	}

	public static bool TryGetMesh(this in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc, in GPUDrivenBatchRendererGroup.RendererParams rendererParams, [CanBeNull] out MeshFilter meshFilter, out Mesh mesh)
	{
		if (rendererDesc.MeshFilter != null)
		{
			meshFilter = rendererDesc.MeshFilter;
			mesh = meshFilter.sharedMesh;
			return mesh != null;
		}
		if ((bool)rendererDesc.MeshRenderer)
		{
			if (rendererDesc.MeshRenderer.TryGetComponent<MeshFilter>(out meshFilter))
			{
				mesh = meshFilter.sharedMesh;
				return mesh != null;
			}
			mesh = null;
			return false;
		}
		meshFilter = null;
		mesh = rendererParams.Mesh;
		return mesh != null;
	}

	public static void GetMaterials(this in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc, in GPUDrivenBatchRendererGroup.RendererParams rendererParams, List<Material> result)
	{
		result.Clear();
		if (rendererParams.Materials != null)
		{
			foreach (Material material in rendererParams.Materials)
			{
				result.Add(material);
			}
			return;
		}
		if (rendererDesc.MeshRenderer != null)
		{
			rendererDesc.MeshRenderer.GetSharedMaterials(result);
		}
	}

	public static int GetGameObjectInstanceID(this in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc, in GPUDrivenBatchRendererGroup.RendererParams rendererParams)
	{
		if (!(rendererDesc.MeshRenderer != null))
		{
			return rendererParams.GameObjectInstanceID;
		}
		return rendererDesc.MeshRenderer.gameObject.GetInstanceID();
	}

	public static int GetSortingOrder(this in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc, in GPUDrivenBatchRendererGroup.RendererParams rendererParams)
	{
		if (!(rendererDesc.MeshRenderer != null))
		{
			return rendererParams.General.SortingOrder;
		}
		return rendererDesc.MeshRenderer.sortingOrder;
	}

	public static Scene GetScene(this in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc, in GPUDrivenBatchRendererGroup.RendererParams rendererParams)
	{
		if (!(rendererDesc.MeshRenderer != null))
		{
			return rendererParams.Scene;
		}
		return rendererDesc.MeshRenderer.gameObject.scene;
	}

	public static bool GetEnabled(this in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc, in GPUDrivenBatchRendererGroup.RendererParams rendererParams)
	{
		if (!(rendererDesc.MeshRenderer != null))
		{
			return rendererParams.Enabled;
		}
		return GPUDrivenRenderingUtils.IsRendererEnabled(rendererDesc.MeshRenderer);
	}

	public static ExtractedRendererSettings GetRendererSettings(this in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc, in GPUDrivenBatchRendererGroup.RendererParams rendererParams)
	{
		MeshRenderer meshRenderer = rendererDesc.MeshRenderer;
		ExtractedRendererSettings result = default(ExtractedRendererSettings);
		if (meshRenderer != null)
		{
			GameObject gameObject = meshRenderer.gameObject;
			result.Enabled = GPUDrivenRenderingUtils.IsRendererEnabled(meshRenderer, gameObject);
			result.Scale = gameObject.transform.lossyScale;
			result.General.Layer = gameObject.layer;
			result.General.RenderingLayerMask = meshRenderer.renderingLayerMask;
			result.General.OccluderBounds = meshRenderer.bounds;
			result.General.SortingOrder = meshRenderer.sortingOrder;
			result.General.MotionVectorGenerationMode = meshRenderer.motionVectorGenerationMode;
			result.General.ShadowCastingMode = meshRenderer.shadowCastingMode;
			result.General.StaticShadowCaster = meshRenderer.staticShadowCaster;
			result.General.LightmapScaleOffset = meshRenderer.lightmapScaleOffset;
			result.General.ReceiveShadows = meshRenderer.receiveShadows;
			result.General.LightmapIndex = meshRenderer.lightmapIndex;
			result.General.LightProbeUsage = meshRenderer.lightProbeUsage;
		}
		else
		{
			result.Enabled = rendererParams.Enabled;
			result.Scale = rendererParams.TransformScale;
			result.General = rendererParams.General;
		}
		return result;
	}

	public static bool TryGetPerInstanceData(this in GPUDrivenBatchRendererGroup.RendererParams rendererParams, [CanBeNull] GPUDrivenRenderer gpuDrivenRenderer, int nameId, out GPUDrivenRenderer.PropertyData propertyData)
	{
		if (gpuDrivenRenderer != null)
		{
			return gpuDrivenRenderer.TryGetInstanceData(nameId, out propertyData);
		}
		if (rendererParams.PerInstanceProperties.IsCreated)
		{
			foreach (GPUDrivenRenderer.PropertyData perInstanceProperty in rendererParams.PerInstanceProperties)
			{
				if (perInstanceProperty.NameID == nameId)
				{
					propertyData = perInstanceProperty;
					return true;
				}
			}
			propertyData = default(GPUDrivenRenderer.PropertyData);
			return false;
		}
		propertyData = default(GPUDrivenRenderer.PropertyData);
		return false;
	}
}
