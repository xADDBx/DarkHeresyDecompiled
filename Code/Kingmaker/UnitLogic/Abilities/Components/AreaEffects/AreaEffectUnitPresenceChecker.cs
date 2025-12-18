using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

[TypeId("9bfada3e8bee43f2aa4bfad9f7627778")]
public class AreaEffectUnitPresenceChecker : AreaEffectLogic
{
	public TargetType CheckForNoTargetsOfType;

	public ActionList ActionsOnAllUnitsInside;

	[SerializeField]
	private BlueprintAreaEffectReference m_NewAreaEffect;

	public ActionList ActionsOnAllUnitsInsideIfFailed;

	public BlueprintAreaEffect NewAreaEffect => m_NewAreaEffect.Get();

	protected override void OnEntityExit(MechanicsContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			return;
		}
		if (CheckTargetType(maybeCaster, entity, CheckForNoTargetsOfType))
		{
			foreach (MechanicEntity item in areaEffect.InGameEntitiesInside)
			{
				if (CheckTargetType(maybeCaster, item, CheckForNoTargetsOfType) && !item.IsDead && item != entity)
				{
					return;
				}
				if (!item.IsDead)
				{
					using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(item))
					{
						ActionsOnAllUnitsInside.Run();
					}
				}
			}
		}
		if (NewAreaEffect != null)
		{
			TargetWrapper target = new TargetWrapper(areaEffect.Position);
			AreaEffectEntity areaEffectEntity = AreaEffectsController.CreateSpawner(NewAreaEffect, context, target).Duration(5.Rounds().Seconds).Spawn();
			if (areaEffectEntity != null)
			{
				foreach (BaseUnitEntity u in Game.Instance.EntityPools.AllBaseUnits)
				{
					if (u.LifeState.IsDead || !u.IsInGame || areaEffectEntity.Blueprint.IsAllArea || !areaEffectEntity.Contains(u))
					{
						continue;
					}
					if (!areaEffectEntity.AffectEnemies)
					{
						MechanicEntity maybeCaster2 = context.MaybeCaster;
						if (maybeCaster2 == null || maybeCaster2.IsEnemy(u))
						{
							continue;
						}
					}
					EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
					{
						h.OnTryToApplyAbilityEffect(context.AsAbilityContext, new AbilityDeliveryTarget(u));
					});
				}
			}
		}
		areaEffect.ForceEnd();
	}

	protected override void OnEndForEachEntity(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			return;
		}
		foreach (MechanicEntity item in areaEffect.InGameEntitiesInside)
		{
			if (!CheckTargetType(maybeCaster, item, CheckForNoTargetsOfType) && !item.IsDead)
			{
				using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.Set(item))
				{
					ActionsOnAllUnitsInsideIfFailed.Run();
				}
			}
		}
	}

	public static bool CheckTargetType(MechanicEntity caster, MechanicEntity target, TargetType targetType)
	{
		if (caster == null || target == null)
		{
			return false;
		}
		if (targetType == TargetType.Any)
		{
			return true;
		}
		PartCombatGroup combatGroupOptional = target.GetCombatGroupOptional();
		if (targetType == TargetType.Ally && combatGroupOptional != null && combatGroupOptional.IsAlly(caster))
		{
			return true;
		}
		if (targetType == TargetType.Enemy && combatGroupOptional != null && combatGroupOptional.IsEnemy(caster))
		{
			return true;
		}
		return false;
	}
}
