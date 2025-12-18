using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[Obsolete]
[TypeId("dffda8080d276d94581a59e62ef57a90")]
public class AbilityCustomStarshipFocusedEfforts : BlueprintComponent
{
	[SerializeField]
	private BlueprintAbilityGroupReference m_AffectedGroup;

	[SerializeField]
	private ActionList Actions;

	public BlueprintAbilityGroup AffectedGroup => m_AffectedGroup?.Get();
}
