using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("4a7549cdf2c944898ee3e790b36e47b1")]
public class PushTrigger : UnitBuffComponentDelegate, IUnitGetAbilityPush, ISubscriber
{
	public ContextValue ValueMultiplier;

	public ContextValue ValueBonus;

	public ContextPropertyName ContextPropertyName;

	public bool OnlyFromOwner;

	public ActionList Actions;

	public void HandleUnitResultPush(int distanceInCells, MechanicEntity caster, MechanicEntity target, Vector3 fromPoint)
	{
		if (!OnlyFromOwner || caster == base.Owner)
		{
			EvalContext.Current[ContextPropertyName] = distanceInCells * ValueMultiplier.Calculate(base.Context) + ValueBonus.Calculate(base.Context);
			base.Fact.RunActionInContext(Actions, target);
		}
	}

	public void HandleUnitAbilityPushDidActed(int distanceInCells)
	{
	}

	public void HandleUnitResultPush(int distanceInCells, Vector3 targetPoint, MechanicEntity target, MechanicEntity caster)
	{
	}
}
