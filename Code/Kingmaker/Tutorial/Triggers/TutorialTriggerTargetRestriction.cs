using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.Utility;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[ClassInfoBox("Triggers on attempt to cast ability on restricted target unit\n`t|SourceAbility` - currently selected spell with target restriction\n`t|TargetUnit` - unit who is wrong spell target")]
[TypeId("6ace81c55f9bbe94da93738ac09d50a7")]
public class TutorialTriggerTargetRestriction : TutorialTrigger, IClickActionHandler, ISubscriber
{
	private static Type[] s_AllowedRestrictions = new Type[2]
	{
		typeof(AbilityTargetHasFact),
		typeof(AbilityTargetHasNoFactUnless)
	};

	public void OnMoveRequested(Vector3 target)
	{
	}

	public void OnCastRequested(AbilityData ability, TargetWrapper target)
	{
	}

	public void OnItemUseRequested(AbilityData ability, TargetWrapper target)
	{
	}

	public void OnAbilityCastRefused(AbilityData ability, TargetWrapper target, IAbilityTargetRestriction failedRestriction)
	{
		if (!target.HasEntity)
		{
			return;
		}
		Type[] array = s_AllowedRestrictions;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IsInstanceOfType(failedRestriction))
			{
				TryToTrigger(null, delegate(TutorialContext context)
				{
					context.SourceAbility = ability;
					context.TargetUnit = target.Entity as BaseUnitEntity;
				});
				break;
			}
		}
	}

	public void OnAttackRequested(BaseUnitEntity unit, UnitEntityView target)
	{
	}
}
