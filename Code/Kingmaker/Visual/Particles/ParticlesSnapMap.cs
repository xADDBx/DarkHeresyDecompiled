using System.Collections.Generic;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Particles.Blueprints;
using Owlcat.Fmw.Blueprints;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class ParticlesSnapMap : SnapMapBase
{
	private struct LocatorAndParent
	{
		public Transform Locator;

		public Transform Parent;
	}

	[Tooltip("Required only for EquipmentEntity based characters")]
	public CharacterFxBonesMap CharacterFxBonesMap;

	protected override void OnInitialize()
	{
		if (!(CharacterFxBonesMap == null) && !base.Initialized)
		{
			base.SizeScale = CharacterFxBonesMap.SizeScale;
			base.ParticleSizeScale = CharacterFxBonesMap.ParticleSizeScale;
			base.LifetimeScale = CharacterFxBonesMap.LifetimeScale;
			base.RateOverTimeScale = CharacterFxBonesMap.RateOverTimeScale;
			base.BurstScale = CharacterFxBonesMap.BurstScale;
		}
	}

	public List<FxLocator> GetLocatorsWithGroups(List<BpRef<BlueprintFxLocatorGroup>> groups)
	{
		List<FxLocator> list = new List<FxLocator>();
		foreach (FxLocator fxLocator in FxLocators)
		{
			foreach (BpRef<BlueprintFxLocatorGroup> group in groups)
			{
				if (fxLocator.Group == group.Blueprint)
				{
					list.Add(fxLocator);
				}
			}
		}
		return list;
	}
}
