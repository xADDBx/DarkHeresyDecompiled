using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Framework.VO;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Sound;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Animation.CustomIdleComponents;
using Owlcat.Fmw.Blueprints;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("944f703c9407ac94aa1cc87bf43a3312")]
public class UnitSpawner : UnitSpawnerBase, IUnitSpawnerConfig, IAbstractUnitSpawnerConfig, IEntityConfig
{
	public delegate void CallbackType(UnitSpawner spawner);

	public static CallbackType OnTransformParentChangedCallback;

	[SerializeField]
	[InspectorReadOnly]
	private BpRef<BlueprintEncounter> m_Encounter = new BpRef<BlueprintEncounter>();

	public VoIdIndex VoIdIndex = new VoIdIndex();

	[SerializeField]
	[Space(40f)]
	private bool m_BossMusicEnable;

	[SerializeField]
	[ShowIf("m_BossMusicEnable")]
	private AkStateReference m_MusicBossFightType;

	public BlueprintEncounter Encounter
	{
		get
		{
			return m_Encounter;
		}
		set
		{
			m_Encounter = value.Reference();
		}
	}

	VoIdIndex IUnitSpawnerConfig.VoIdIndex => VoIdIndex;

	bool IUnitSpawnerConfig.IsLightweight => IsLightweight;

	bool IUnitSpawnerConfig.BossMusicEnable => m_BossMusicEnable;

	AkStateReference IUnitSpawnerConfig.MusicBossFightType => m_MusicBossFightType;

	CustomIdleAnimationMonoComponent IUnitSpawnerConfig.CustomIdleAnimation => GetComponent<CustomIdleAnimationMonoComponent>();

	private bool IsLightweight
	{
		get
		{
			if (Encounter == null && TryGetComponent<SpawnerOptimizedUnit>(out var component))
			{
				return component.IsLightweight;
			}
			return false;
		}
	}

	public string GetVoGuid()
	{
		return VoIdIndex.GetVoGuid(base.Blueprint);
	}

	[ExecuteAlways]
	private void OnTransformParentChanged()
	{
		OnTransformParentChangedCallback?.Invoke(this);
	}

	protected override AbstractUnitSpawnerEntity CreateSpawnerEntity(bool load)
	{
		return Entity.Initialize(new UnitSpawnerEntity(this));
	}

	string IAbstractUnitSpawnerConfig.get_name()
	{
		return base.name;
	}
}
