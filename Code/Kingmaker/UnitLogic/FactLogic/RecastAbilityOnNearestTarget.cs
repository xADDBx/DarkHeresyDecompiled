using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete]
[TypeId("e9476dcc54a85384d984617614a19a07")]
public class RecastAbilityOnNearestTarget : MechanicEntityFactComponentDelegate, IAbilityExecutionProcessHandler<EntitySubscriber>, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IAbilityExecutionProcessHandler, EntitySubscriber>
{
	public RestrictionCalculator Restriction;

	[SerializeField]
	private TargetType m_TargetType;

	public RestrictionCalculator TargetRestriction;

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
	}

	[CanBeNull]
	private MechanicEntity SelectTarget(AbilityExecutionContext context)
	{
		return (from i in Game.Instance.EntityPools.AllBaseAwakeUnits.InCombat()
			where i != context.ClickedTarget.Entity
			where (m_TargetType == TargetType.Ally && base.Owner.IsAlly(i)) || (m_TargetType == TargetType.Enemy && base.Owner.IsEnemy(i)) || m_TargetType == TargetType.Any
			where context.Ability.IsValid(i)
			where TargetRestriction.IsPassed(base.Context, i)
			select i).MinBy((BaseUnitEntity i) => (i.Position - context.ClickedTarget.Point).sqrMagnitude);
	}
}
