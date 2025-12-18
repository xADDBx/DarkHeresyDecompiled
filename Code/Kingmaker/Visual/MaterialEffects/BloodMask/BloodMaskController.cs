using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.BloodMask;

public class BloodMaskController
{
	private struct MaterialEntry
	{
		public BloodMaskMaterial Material;

		public BloodMaskMaterial.Snapshot Snapshot;
	}

	private bool m_Resetted;

	private readonly List<MaterialEntry> m_MaterialEntries = new List<MaterialEntry>();

	public List<BloodMaskSettings> SettingsEntries = new List<BloodMaskSettings>();

	public void AddMaterial(BloodMaskMaterial material)
	{
		m_MaterialEntries.Add(new MaterialEntry
		{
			Material = material,
			Snapshot = material.TakeSnapshot()
		});
	}

	public void ClearMaterials()
	{
		m_MaterialEntries.Clear();
	}

	public void UpdateMaterialProperties()
	{
		for (int i = 0; i < m_MaterialEntries.Count; i++)
		{
			MaterialEntry value = m_MaterialEntries[i];
			value.Snapshot.BloodMask = value.Material.BloodMask;
			value.Snapshot.BloodMask_ST = value.Material.BloodMask_ST;
			value.Snapshot.BloodMaskColor = value.Material.BloodMaskColor;
			value.Snapshot.BloodMaskEdge = value.Material.BloodMaskEdge;
			m_MaterialEntries[i] = value;
		}
	}

	internal void Update()
	{
		if (SettingsEntries.Count > 0)
		{
			int num = SettingsEntries.Count - 1;
			while (num >= 0)
			{
				BloodMaskSettings bloodMaskSettings = SettingsEntries[num];
				if (bloodMaskSettings.IsDisabled)
				{
					SettingsEntries.RemoveAt(num);
					num--;
					continue;
				}
				if (!bloodMaskSettings.IsActivated)
				{
					ApplyMaterialSettings(bloodMaskSettings);
				}
				if (bloodMaskSettings.IsNeedUpdate)
				{
					UpdateMaterialSettings(bloodMaskSettings);
				}
				break;
			}
			m_Resetted = false;
		}
		else if (!m_Resetted)
		{
			m_Resetted = true;
			RevertToDefaults();
		}
	}

	private void ApplyMaterialSettings(BloodMaskSettings settings)
	{
		foreach (MaterialEntry materialEntry in m_MaterialEntries)
		{
			MaterialEntry current = materialEntry;
			current.Material.BloodMask = settings.BloodTexture;
			current.Material.BloodMaskColor = settings.BloodColor;
			Vector4 bloodMask_ST = current.Snapshot.BloodMask_ST;
			bloodMask_ST.x = settings.DefaultTileSize.x * settings.UnitSizeMultiplier;
			bloodMask_ST.y = settings.DefaultTileSize.y * settings.UnitSizeMultiplier;
			current.Material.BloodMask_ST = bloodMask_ST;
			current.Material.BloodMaskEdge = settings.FadeOut;
		}
		settings.IsActivated = true;
	}

	private void UpdateMaterialSettings(BloodMaskSettings settings)
	{
		foreach (MaterialEntry materialEntry in m_MaterialEntries)
		{
			MaterialEntry current = materialEntry;
			current.Material.BloodMaskColor = settings.BloodColor;
			current.Material.BloodMaskEdge = settings.FadeOut;
		}
		settings.IsNeedUpdate = false;
	}

	public void RevertToDefaults()
	{
		foreach (MaterialEntry materialEntry in m_MaterialEntries)
		{
			MaterialEntry current = materialEntry;
			current.Material.ApplySnapshot(in current.Snapshot);
		}
		foreach (BloodMaskSettings settingsEntry in SettingsEntries)
		{
			settingsEntry.IsActivated = false;
		}
	}
}
