using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("a0905c3e64e84a978a09a1c77eb114dc")]
public abstract class AbilityTrigger : UnitFactComponentDelegate
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	protected ActionList Action;

	[SerializeField]
	protected bool ForOneAbility;

	[ShowIf("ForOneAbility")]
	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	[SerializeField]
	protected bool ForMultipleAbilities;

	[ShowIf("ForMultipleAbilities")]
	[SerializeField]
	protected List<BlueprintAbilityReference> Abilities;

	[SerializeField]
	protected bool ForAbilityGroup;

	[ShowIf("ForAbilityGroup")]
	[SerializeField]
	private BlueprintAbilityGroupReference m_AbilityGroup;

	[SerializeField]
	protected bool ForUltimateAbilities;

	public BlueprintAbility Ability => m_Ability?.Get();

	public BlueprintAbilityGroup AbilityGroup => m_AbilityGroup.Get();

	public ActionList Actions => Action;

	public bool AbilityRestrictionsIsPassed(BlueprintAbilityWrapper ability)
	{
		return CanRunActions(ability);
	}

	public bool RestrictionsIsPassed(IEvalContext context, MechanicEntity owner, MechanicEntity target)
	{
		return Restrictions.IsPassed(context, owner, target);
	}

	protected void RunAction(BlueprintAbilityWrapper abilityWrapper, [CanBeNull] MechanicEntity initiator, [CanBeNull] TargetWrapper target, bool assignAsTarget, AbilityTrigger componentType)
	{
		MechanicEntity mechanicEntity = initiator ?? target?.Entity;
		if (mechanicEntity == null)
		{
			PFLog.Default.Error("AbilityTrigger: Both initiator and target are null!");
		}
		else if (((mechanicEntity == base.Context.MaybeOwner && componentType is AbilityRuleTriggerInitiator) || (target == base.Context.MaybeOwner && componentType is AbilityRuleTriggerTarget)) && CanRunActions(abilityWrapper))
		{
			base.Fact.RunActionInContext(Action, assignAsTarget ? ((TargetWrapper)initiator) : target);
		}
	}

	protected void RunAction(BlueprintAbilityWrapper ability, AbilityExecutionContext context, TargetWrapper target, bool assignAsTarget, bool assignContextFromAbility)
	{
		if (!CanRunActions(ability))
		{
			return;
		}
		if (assignContextFromAbility)
		{
			using (EvalContext.PushContext(context, assignAsTarget ? ((TargetWrapper)context.Caster) : target))
			{
				Action.Run();
				return;
			}
		}
		base.Fact.RunActionInContext(Action, assignAsTarget ? ((TargetWrapper)context.Caster) : target);
	}

	private bool CanRunActions(BlueprintAbilityWrapper ability)
	{
		if ((!ForOneAbility || ability.SameAbility(Ability)) && (!ForMultipleAbilities || Abilities.HasItem((BlueprintAbilityReference r) => ability.SameAbility(r))) && (!ForAbilityGroup || ability.AbilityGroups.Contains(AbilityGroup)))
		{
			return !ForUltimateAbilities;
		}
		return false;
	}
}
