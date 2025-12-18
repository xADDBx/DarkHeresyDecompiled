using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Framework.VO;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.CustomIdleComponents;
using Owlcat.Fmw.Blueprints;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("944f703c9407ac94aa1cc87bf43a3312")]
public class UnitSpawner : UnitSpawnerBase, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
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

	[CanBeNull]
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

	protected override AbstractUnitEntity SpawnUnit(Vector3 position, Quaternion rotation)
	{
		if (Game.Instance.Player.AllCharacters.FirstItem((BaseUnitEntity u) => u.Blueprint.CheckEqualsWithPrototype(base.Blueprint))?.GetCompanionOptional() != null)
		{
			throw new InvalidOperationException(string.Format("Can't spawn {0} because it is companion. Use {1} instead.", base.Blueprint, "CompanionSpawner"));
		}
		using (SimpleContextData<int, BaseUnitEntity.OverrideUnitCR>.Set(Encounter?.OverrideCombatCR ?? 0))
		{
			AbstractUnitEntity abstractUnitEntity = (IsLightweight ? Game.Instance.Controllers.EntitySpawner.SpawnLightweightUnit(base.Blueprint, position, rotation, base.Data.HoldingState, base.SelectedCustomizationVariation) : Game.Instance.Controllers.EntitySpawner.SpawnUnit(base.Blueprint, position, rotation, base.Data.HoldingState, base.SelectedCustomizationVariation));
			abstractUnitEntity.SelectVoGuid(VoIdIndex.GetVoGuid(base.Blueprint));
			if (!(abstractUnitEntity is BaseUnitEntity baseUnitEntity))
			{
				return abstractUnitEntity;
			}
			if (Encounter != null)
			{
				abstractUnitEntity.GetOrCreate<PartEncounter>().SetupOnSpawn(Encounter, UniqueId);
			}
			else
			{
				baseUnitEntity.CombatGroup.Id = "<peaceful-unit>";
			}
			if (m_BossMusicEnable)
			{
				baseUnitEntity.MusicBossFightType = m_MusicBossFightType;
			}
			return baseUnitEntity;
		}
	}

	public void HandleUnitSpawned()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (base.HasSpawned && abstractUnitEntity != null && abstractUnitEntity == base.SpawnedUnit)
		{
			CustomIdleAnimationMonoComponent component = GetComponent<CustomIdleAnimationMonoComponent>();
			if (!(component == null) && abstractUnitEntity.View.AnimationManager != null)
			{
				abstractUnitEntity.View.AnimationManager.CustomIdleWrappers = component.IdleClips;
			}
		}
	}

	public void HandleUnitDestroyed()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (base.HasSpawned && abstractUnitEntity == base.SpawnedUnit)
		{
			base.Data.HasDied = true;
		}
	}

	public void HandleUnitDeath()
	{
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
}
