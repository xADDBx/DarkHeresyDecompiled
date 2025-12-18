using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Scenes;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;

[ExecuteAlways]
[DefaultExecutionOrder(-100)]
public sealed class GPUDrivenLightmapReference : MonoBehaviour
{
	private static readonly Dictionary<GPUDrivenSceneHandle, GPUDrivenLightmapReference> s_SceneToLightmapReference = new Dictionary<GPUDrivenSceneHandle, GPUDrivenLightmapReference>();

	[SerializeField]
	private LightmapArraysCollection m_LightmapArrays;

	[SerializeField]
	private Texture2D[] m_SourceColorLightmaps = Array.Empty<Texture2D>();

	[SerializeField]
	private Texture2D[] m_SourceDirLightmaps = Array.Empty<Texture2D>();

	[SerializeField]
	private Texture2D[] m_SourceShadowmaskLightmaps = Array.Empty<Texture2D>();

	public ref readonly LightmapArraysCollection LightmapArrays => ref m_LightmapArrays;

	private void OnEnable()
	{
		AddReference(GetSceneHandle(), this);
	}

	private void OnDisable()
	{
		RemoveReference(GetSceneHandle(), this);
	}

	public void SetSourceColorLightmaps(Texture2D[] color, Texture2D[] dir, Texture2D[] shadowmask)
	{
		m_SourceColorLightmaps = color ?? Array.Empty<Texture2D>();
		m_SourceDirLightmaps = dir ?? Array.Empty<Texture2D>();
		m_SourceShadowmaskLightmaps = shadowmask ?? Array.Empty<Texture2D>();
	}

	private GPUDrivenSceneHandle GetSceneHandle()
	{
		Scene scene = base.gameObject.scene;
		return GPUDrivenSceneHandle.FromScene(in scene);
	}

	private void OnAdded()
	{
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		if ((object)asset != null)
		{
			GPUDrivenBRGSettings gPUDrivenBRGSettings = asset.GPUDrivenBRGSettings;
			if (gPUDrivenBRGSettings != null && gPUDrivenBRGSettings.IsEnabledAndSupported)
			{
				return;
			}
		}
		PatchAllSourceLightmaps();
	}

	private static void AddReference(GPUDrivenSceneHandle sceneHandle, GPUDrivenLightmapReference reference)
	{
		if (s_SceneToLightmapReference.TryGetValue(sceneHandle, out var value))
		{
			if (!(value == reference))
			{
				s_SceneToLightmapReference[sceneHandle] = reference;
				reference.OnAdded();
			}
		}
		else
		{
			s_SceneToLightmapReference.Add(sceneHandle, reference);
			reference.OnAdded();
		}
	}

	private void PatchAllSourceLightmaps()
	{
		PatchSourceLightmapsComponent(m_LightmapArrays.Color, m_SourceColorLightmaps);
		PatchSourceLightmapsComponent(m_LightmapArrays.Dir, m_SourceDirLightmaps);
		PatchSourceLightmapsComponent(m_LightmapArrays.Shadowmask, m_SourceShadowmaskLightmaps);
		static void PatchSourceLightmapsComponent(Texture2DArray lightmapArray, Texture2D[] sourceLightmaps)
		{
			if (!(lightmapArray == null))
			{
				for (int i = 0; i < lightmapArray.depth && i < sourceLightmaps.Length; i++)
				{
					PatchSourceLightmap(sourceLightmaps, i, lightmapArray);
				}
			}
		}
	}

	private static void PatchSourceLightmap(Texture2D[] sourceLightmaps, int arrayIndex, Texture2DArray lightmapArray)
	{
		Texture2D texture2D = sourceLightmaps[arrayIndex];
		if (texture2D.isReadable)
		{
			texture2D.Reinitialize(lightmapArray.width, lightmapArray.height, lightmapArray.graphicsFormat, lightmapArray.mipmapCount > 1);
			texture2D.Apply(updateMipmaps: true, makeNoLongerReadable: true);
			for (int i = 0; i < lightmapArray.mipmapCount; i++)
			{
				Graphics.CopyTexture(lightmapArray, arrayIndex, i, texture2D, 0, i);
			}
		}
	}

	private static void RemoveReference(GPUDrivenSceneHandle sceneHandle, GPUDrivenLightmapReference removedReference)
	{
		if (s_SceneToLightmapReference.TryGetValue(sceneHandle, out var value) && value == removedReference)
		{
			s_SceneToLightmapReference.Remove(sceneHandle);
		}
	}

	public static bool TryGet(GPUDrivenSceneHandle sceneHandle, out GPUDrivenLightmapReference lightmapReference)
	{
		return s_SceneToLightmapReference.TryGetValue(sceneHandle, out lightmapReference);
	}

	public void SetLightmaps(in LightmapArraysCollection lightmapArraysCollection)
	{
		m_LightmapArrays = lightmapArraysCollection;
	}

	public static void RequestLightmapUnpack(Scene scene, int globalLightmapIndex)
	{
		if (globalLightmapIndex < 0 || globalLightmapIndex > 65533)
		{
			return;
		}
		LightmapData[] lightmaps = LightmapSettings.lightmaps;
		if (lightmaps.Length != 0)
		{
			LightmapData lightmapData = lightmaps[globalLightmapIndex];
			if (lightmapData != null && TryGet(GPUDrivenSceneHandle.FromScene(in scene), out var lightmapReference))
			{
				FindAndPatch(lightmapReference.LightmapArrays.Color, lightmapData.lightmapColor, lightmapReference.m_SourceColorLightmaps);
				FindAndPatch(lightmapReference.LightmapArrays.Dir, lightmapData.lightmapDir, lightmapReference.m_SourceDirLightmaps);
				FindAndPatch(lightmapReference.LightmapArrays.Shadowmask, lightmapData.shadowMask, lightmapReference.m_SourceShadowmaskLightmaps);
			}
		}
		static void FindAndPatch(Texture2DArray lightmapArray, Texture2D lightmapColor, Texture2D[] sourceLightmaps)
		{
			if (!(lightmapArray == null) && !(lightmapColor == null) && sourceLightmaps != null)
			{
				int num = Array.IndexOf(sourceLightmaps, lightmapColor);
				if (num >= 0 && num < lightmapArray.depth)
				{
					PatchSourceLightmap(sourceLightmaps, num, lightmapArray);
				}
			}
		}
	}
}
