using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("cef37152812a4ba9b58d62fa76bc252c")]
public class AbilityAttackSpawnAreaEffect : BlueprintComponent
{
	public BlueprintAreaEffectReference AreaEffect;

	public bool OverridePatternWithAttackPattern;

	public ContextDurationValue DurationValue;

	[SerializeField]
	private bool m_GetOrientationFromCaster;

	private BlueprintAreaEffect BlueprintAreaEffect => AreaEffect.Get();

	public void SpawnAreaEffect(AbilityExecutionContext context, TargetWrapper target)
	{
		TimeSpan seconds = DurationValue.Calculate(context).Seconds;
		AreaEffectEntity areaEffectEntity = AreaEffectsController.CreateSpawner(BlueprintAreaEffect, context, target).Duration(seconds).UsePatternFromAbility(OverridePatternWithAttackPattern)
			.GetOrientationFromCaster(m_GetOrientationFromCaster)
			.Spawn();
		if (areaEffectEntity == null)
		{
			return;
		}
		foreach (BaseUnitEntity u in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (!u.LifeState.IsDead && u.IsInGame && !areaEffectEntity.Blueprint.IsAllArea && areaEffectEntity.Contains(u) && (areaEffectEntity.AffectEnemies || !context.Caster.IsEnemy(u)))
			{
				EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
				{
					h.OnTryToApplyAbilityEffect(context, new AbilityDeliveryTarget(u));
				});
			}
		}
	}
}
