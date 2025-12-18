using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.ContextActions;

[Serializable]
[TypeId("8f5d827d6e304924a0a60d207a1dd9d0")]
public sealed class ContextActionRhymeAbility : ContextAction
{
	public static class Scope
	{
		public sealed class Ability : SimpleContextData<AbilityData, Ability>
		{
		}

		public sealed class Target : SimpleContextData<TargetWrapper, Target>
		{
		}
	}

	[ValidateNotNull]
	public BpRef<BlueprintBuff> RhymedBuff = new BpRef<BlueprintBuff>();

	public RestrictionCalculator AbilityRestriction = new RestrictionCalculator();

	private static IEnumerable<MechanicEntity> UnitsInCombat => Game.Instance.Controllers.TurnController.UnitsInCombat;

	public override string GetCaption()
	{
		return "Rhyme random ability on random target";
	}

	protected override void RunAction()
	{
		List<AbilityData> list;
		using (GetSuitableAbilities(base.TargetEntity).ToPooledList(out list))
		{
			list.Shuffle(PFStatefulRandom.Action);
			List<MechanicEntity> list2;
			using (UnitsInCombat.ToPooledList(out list2))
			{
				list2.Shuffle(PFStatefulRandom.Action);
				AbilityData abilityData = null;
				TargetWrapper value = null;
				foreach (AbilityData item in list)
				{
					TargetWrapper firstValidTarget = GetFirstValidTarget(item, list2);
					if ((object)firstValidTarget != null)
					{
						abilityData = item;
						value = firstValidTarget;
						break;
					}
				}
				if (abilityData == null)
				{
					return;
				}
				using (SimpleContextData<AbilityData, Scope.Ability>.Set(abilityData))
				{
					using (SimpleContextData<TargetWrapper, Scope.Target>.Set(value))
					{
						base.TargetEntity.AddFact((BlueprintBuff?)RhymedBuff, base.Context);
					}
				}
			}
		}
	}

	private IEnumerable<AbilityData> GetSuitableAbilities(MechanicEntity entity)
	{
		List<Ability> all = entity.Facts.GetAll<Ability>();
		foreach (Ability item in all)
		{
			if (AbilityRestriction.IsPassed(base.Context, null, null, null, item.Data))
			{
				yield return item.Data;
			}
		}
	}

	[CanBeNull]
	private static TargetWrapper GetFirstValidTarget(AbilityData ability, List<MechanicEntity> possibleTargets)
	{
		foreach (MechanicEntity possibleTarget in possibleTargets)
		{
			if (ability.IsValid(possibleTarget))
			{
				return possibleTarget;
			}
		}
		return null;
	}
}
