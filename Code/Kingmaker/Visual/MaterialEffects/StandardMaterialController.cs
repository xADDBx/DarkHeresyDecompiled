using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.Visual.MaterialEffects.AdditionalAlbedo;
using Kingmaker.Visual.MaterialEffects.BloodMask;
using Kingmaker.Visual.MaterialEffects.ColorTint;
using Kingmaker.Visual.MaterialEffects.CustomMaterialProperty;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using Kingmaker.Visual.MaterialEffects.LayeredMaterial;
using Kingmaker.Visual.MaterialEffects.MaterialParametersOverride;
using Kingmaker.Visual.MaterialEffects.RimLighting;
using Owlcat.Runtime.Core.Updatables;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.MaterialEffects;

public class StandardMaterialController : UpdateableBehaviour
{
	private bool m_IsDirty;

	[SerializeField]
	private HashSet<int> m_ActiveRendererIds = new HashSet<int>();

	[SerializeField]
	private ColorTintAnimationController m_ColorTintController = new ColorTintAnimationController();

	[SerializeField]
	private RimLightingAnimationController m_RimController = new RimLightingAnimationController();

	[SerializeField]
	private DissolveAnimationController m_DissolveController = new DissolveAnimationController();

	[SerializeField]
	[FormerlySerializedAs("m_PetrificationController")]
	private AdditionalAlbedoAnimationController m_AdditionalAlbedoController = new AdditionalAlbedoAnimationController();

	[SerializeField]
	private MaterialParametersOverrideController m_MaterialParametersOverrideController = new MaterialParametersOverrideController();

	[SerializeField]
	private BloodMaskController m_BloodMaskController = new BloodMaskController();

	private CustomMaterialPropertyAnimationController m_CustomMaterialPropertyAnimationController = new CustomMaterialPropertyAnimationController();

	private LayeredMaterialController m_OverlayMaterialController;

	private bool m_IsDisabled;

	internal ColorTintAnimationController ColorTintController => m_ColorTintController;

	internal RimLightingAnimationController RimController => m_RimController;

	internal DissolveAnimationController DissolveController => m_DissolveController;

	internal AdditionalAlbedoAnimationController AdditionalAlbedoController => m_AdditionalAlbedoController;

	internal MaterialParametersOverrideController MaterialParametersOverrideController => m_MaterialParametersOverrideController;

	internal BloodMaskController BloodMaskController => m_BloodMaskController;

	internal CustomMaterialPropertyAnimationController CustomMaterialPropertyAnimationController => m_CustomMaterialPropertyAnimationController;

	[UsedImplicitly]
	private void Awake()
	{
		if (m_OverlayMaterialController == null)
		{
			m_OverlayMaterialController = new LayeredMaterialController(base.gameObject, new MaterialPropertyBlock());
		}
		m_OverlayMaterialController.SetMaxActiveLayersCount(ConfigRoot.Instance.FxRoot.MaxMaterialLayersCount);
		Setup();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		m_CustomMaterialPropertyAnimationController.Dispose();
		CleanupRenderers();
	}

	private void Setup()
	{
		m_OverlayMaterialController.RefreshScriptPropertiesSnapshot();
		List<Renderer> value;
		using (CollectionPool<List<Renderer>, Renderer>.Get(out value))
		{
			GetComponentsInChildren(value);
			HashSet<int> hashSet = new HashSet<int>();
			foreach (Renderer item in value)
			{
				if ((item is MeshRenderer || item is SkinnedMeshRenderer || item is ParticleSystemRenderer || item is LineRenderer || item is TrailRenderer) && item.renderingLayerMask != 0)
				{
					int instanceID = item.GetInstanceID();
					hashSet.Add(instanceID);
					if (!m_ActiveRendererIds.Contains(instanceID))
					{
						SetupRendererMaterials(item);
						m_OverlayMaterialController.SetupRenderer(item);
					}
				}
			}
			foreach (int activeRendererId in m_ActiveRendererIds)
			{
				if (!hashSet.Contains(activeRendererId))
				{
					ClearMaterialsByRenderer(activeRendererId);
					m_OverlayMaterialController.RemoveRenderer(activeRendererId);
				}
			}
			m_ActiveRendererIds = hashSet;
		}
		m_OverlayMaterialController.UpdateMaterials();
	}

	private void SetupRendererMaterials(Renderer targetRenderer)
	{
		List<Material> value;
		using (CollectionPool<List<Material>, Material>.Get(out value))
		{
			bool flag = false;
			targetRenderer.GetSharedMaterials(value);
			int instanceID = targetRenderer.GetInstanceID();
			int i = 0;
			for (int count = value.Count; i < count; i++)
			{
				Material material = value[i];
				if (material == null || material.shader == null)
				{
					continue;
				}
				bool flag2 = ColorTintMaterial.IsMaterialCompatible(material);
				bool flag3 = RimLightingMaterial.IsMaterialCompatible(material);
				bool flag4 = DissolveMaterial.IsMaterialCompatible(material);
				bool flag5 = AdditionalAlbedoMaterial.IsMaterialCompatible(material);
				bool flag6 = ParametersOverrideMaterial.IsMaterialCompatible(material);
				bool flag7 = BloodMaskMaterial.IsMaterialCompatible(material);
				if (flag2 || flag3 || flag4 || flag5 || flag7 || flag6)
				{
					Material material3 = (value[i] = new Material(material));
					if (flag2)
					{
						ColorTintAnimationController colorTintController = m_ColorTintController;
						ColorTintMaterial material4 = new ColorTintMaterial(material3);
						colorTintController.AddMaterial(in material4, instanceID);
					}
					if (flag3)
					{
						m_RimController.AddMaterial(new RimLightingMaterial(material3), instanceID);
					}
					if (flag4)
					{
						m_DissolveController.AddMaterial(new DissolveMaterial(material3), instanceID);
					}
					if (flag5)
					{
						AdditionalAlbedoAnimationController additionalAlbedoController = m_AdditionalAlbedoController;
						AdditionalAlbedoMaterial material5 = new AdditionalAlbedoMaterial(material3);
						additionalAlbedoController.AddMaterial(in material5, instanceID);
					}
					if (flag6)
					{
						m_MaterialParametersOverrideController.AddMaterial(new ParametersOverrideMaterial(material3), instanceID);
					}
					if (flag7)
					{
						m_BloodMaskController.AddMaterial(new BloodMaskMaterial(material3), instanceID);
					}
					m_CustomMaterialPropertyAnimationController.AddMaterial(material3, instanceID);
					flag = true;
				}
			}
			if (flag)
			{
				targetRenderer.SetSharedMaterialsExt(value);
			}
		}
	}

	private void ClearMaterialsByRenderer(int rendererId)
	{
		m_ColorTintController.ClearMaterial(rendererId);
		m_RimController.ClearMaterial(rendererId);
		m_DissolveController.ClearMaterial(rendererId);
		m_AdditionalAlbedoController.ClearMaterial(rendererId);
		m_MaterialParametersOverrideController.ClearMaterial(rendererId);
		m_BloodMaskController.ClearMaterial(rendererId);
	}

	protected override void OnEnabled()
	{
		DoUpdate();
	}

	public void InvalidateRenderer(Renderer invalidRenderer)
	{
		int instanceID = invalidRenderer.GetInstanceID();
		if (m_ActiveRendererIds.Contains(instanceID))
		{
			m_ActiveRendererIds.Remove(instanceID);
			ClearMaterialsByRenderer(instanceID);
		}
		UpdateRenderers();
	}

	public void CleanupRenderers()
	{
		m_ActiveRendererIds.Clear();
		m_ColorTintController.RevertToDefaults();
		m_RimController.RevertToDefaults();
		m_DissolveController.RevertToDefaults();
		m_AdditionalAlbedoController.RevertToDefaults();
		m_BloodMaskController.RevertToDefaults();
		m_MaterialParametersOverrideController.RevertToDefaults();
		m_ColorTintController.ClearMaterials();
		m_RimController.ClearMaterials();
		m_DissolveController.ClearMaterials();
		m_AdditionalAlbedoController.ClearMaterials();
		m_MaterialParametersOverrideController.ClearMaterials();
		m_BloodMaskController.ClearMaterials();
		m_IsDirty = true;
	}

	public void UpdateRenderers()
	{
		m_IsDirty = true;
	}

	public void DisableUpdate()
	{
		m_IsDisabled = true;
	}

	public override void DoUpdate()
	{
		if (m_IsDisabled)
		{
			return;
		}
		if (m_IsDirty)
		{
			m_IsDirty = false;
			Setup();
		}
		using (Counters.StandardMaterialController?.Measure())
		{
			m_ColorTintController.Update();
			m_RimController.Update();
			m_DissolveController.Update();
			m_AdditionalAlbedoController.Update();
			m_BloodMaskController.Update();
			m_MaterialParametersOverrideController.Update();
			m_OverlayMaterialController.Update();
			m_CustomMaterialPropertyAnimationController.Update();
		}
	}

	internal bool TryAddOverlayAnimation(LayeredMaterialAnimationSetup setup, out int token)
	{
		return m_OverlayMaterialController.TryAddAnimation(setup, out token);
	}

	internal void RemoveOverlayAnimation(int token)
	{
		m_OverlayMaterialController.RemoveAnimation(token);
	}
}
