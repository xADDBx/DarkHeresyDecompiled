using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("a5bbec43fe6354d46b816ca537eb7688")]
public class TutorialTriggerForcePush : TutorialTrigger, IUnitGetAbilityPush, ISubscriber
{
	public void HandleUnitResultPush(int distanceInCells, MechanicEntity caster, MechanicEntity target, Vector3 fromPoint)
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (unit != null && unit.IsPlayerFaction)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = unit;
			});
		}
	}

	public void HandleUnitAbilityPushDidActed(int distanceInCells)
	{
	}

	public void HandleUnitResultPush(int distanceInCells, Vector3 targetPoint, MechanicEntity target, MechanicEntity caster)
	{
	}
}
