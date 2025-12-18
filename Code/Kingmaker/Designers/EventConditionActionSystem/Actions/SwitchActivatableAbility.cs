using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("6c9ae03ab2e86154abca4e6cf77c9651")]
public class SwitchActivatableAbility : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeField]
	private BlueprintActivatableAbilityReference m_Ability;

	public bool IsOn;

	public BlueprintActivatableAbility Ability => m_Ability;

	public override string GetCaption()
	{
		if (!IsOn)
		{
			return $"Turn off activatable ability {Ability} for unit {Unit}";
		}
		return $"Turn on activatable ability {Ability} for unit {Unit}";
	}

	protected override void RunAction()
	{
		Unit.GetValue().Facts.Get<ActivatableAbility>(Ability).IsOn = IsOn;
	}
}
