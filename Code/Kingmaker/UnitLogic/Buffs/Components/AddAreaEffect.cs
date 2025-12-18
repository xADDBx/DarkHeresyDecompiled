using System;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
[TypeId("25b073dd90738ed46939db4777aafe17")]
public class AddAreaEffect : UnitFactComponentDelegate, IAreaHandler, ISubscriber, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>
{
	[SerializeField]
	private BlueprintAreaEffectReference m_AreaEffect;

	public BlueprintAreaEffect AreaEffect => m_AreaEffect?.Get();

	protected override void OnActivate()
	{
		if (ContextData<UnitHelper.PreviewUnit>.Current == null && !base.IsReapplying && Game.Instance.CurrentlyLoadedArea != null && base.Owner.IsInState)
		{
			SpawnAreaEffect();
		}
	}

	protected override void OnDeactivate()
	{
		if (!base.IsReapplying)
		{
			base.Owner.GetOptional<UnitPartSpawnedAreaEffects>()?.RemoveAndEnd(base.Fact, this);
		}
	}

	public void OnAreaBeginUnloading()
	{
		base.Owner.GetOptional<UnitPartSpawnedAreaEffects>()?.RemoveAndEnd(base.Fact, this);
	}

	public void OnAreaDidLoad()
	{
		if (!base.Owner.IsInState)
		{
			PFLog.Default.Error($"Area effect from wrong unit: {AreaEffect.NameSafe()} on {base.Owner}");
		}
		else if (base.Owner.GetOptional<UnitPartSpawnedAreaEffects>()?.Get(base.Fact, this) == null)
		{
			SpawnAreaEffect();
		}
	}

	void IUnitSpawnHandler.HandleUnitSpawned()
	{
		if (base.Owner.GetOptional<UnitPartSpawnedAreaEffects>()?.Get(base.Fact, this) == null)
		{
			SpawnAreaEffect();
		}
	}

	private void SpawnAreaEffect()
	{
		if (base.Owner.IsInGame)
		{
			AreaEffectEntity areaEffectEntity = AreaEffectsController.CreateSpawner(AreaEffect, base.Fact.Context, base.Owner).OnUnit().Spawn();
			areaEffectEntity.SourceFact = base.Fact;
			base.Owner.GetOrCreate<UnitPartSpawnedAreaEffects>().Add(base.Fact, this, areaEffectEntity);
		}
	}

	protected override void OnApplyPostLoadFixes()
	{
		if (!base.Owner.IsInGame)
		{
			base.Owner.GetOptional<UnitPartSpawnedAreaEffects>()?.RemoveAndEnd(base.Fact, this);
		}
	}
}
