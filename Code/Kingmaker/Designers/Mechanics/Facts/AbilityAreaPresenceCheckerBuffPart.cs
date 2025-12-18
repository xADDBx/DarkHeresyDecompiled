using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("d1220b217b8a4dc38ad25ae7c625e75b")]
public class AbilityAreaPresenceCheckerBuffPart : UnitBuffComponentDelegate, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber
{
	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		AreaEffectEntity areaEffectEntity = Game.Instance.EntityPools.AreaEffects.FirstOrDefault((AreaEffectEntity p) => base.Buff.Sources.HasItem((EntityFactSource i) => i.Entity == p));
		AreaEffectUnitPresenceChecker areaEffectUnitPresenceChecker = areaEffectEntity?.Blueprint.GetComponent<AreaEffectUnitPresenceChecker>();
		if (areaEffectEntity == null || areaEffectUnitPresenceChecker == null)
		{
			return;
		}
		MechanicEntity maybeCaster = areaEffectEntity.Context.MaybeCaster;
		if (AreaEffectUnitPresenceChecker.CheckTargetType(maybeCaster, evt.ConcreteTarget, areaEffectUnitPresenceChecker.CheckForNoTargetsOfType))
		{
			foreach (MechanicEntity item in areaEffectEntity.InGameEntitiesInside)
			{
				if (AreaEffectUnitPresenceChecker.CheckTargetType(maybeCaster, item, areaEffectUnitPresenceChecker.CheckForNoTargetsOfType))
				{
					return;
				}
				using (areaEffectEntity.Context.SetScope(item))
				{
					areaEffectUnitPresenceChecker.ActionsOnAllUnitsInside.Run();
				}
			}
		}
		foreach (MechanicEntity item2 in areaEffectEntity.InGameEntitiesInside)
		{
			if (!item2.IsDead && item2.IsEnemy(base.Owner))
			{
				break;
			}
		}
	}
}
