using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete("Use AddAbilityRestriction instead")]
[ComponentName("Caster Restriction/AbilityRestriction")]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("4154c6f5c7b64b23a2102278fc83645b")]
public class AbilityRestriction : UnitBuffComponentDelegate
{
	[SerializeField]
	private bool m_UseAbilityGroups;

	[SerializeField]
	private bool m_UseOnlyListedAbilities;

	[SerializeField]
	[HideIf("m_UseAbilityGroups")]
	private List<BlueprintAbilityReference> m_Abilities;

	[SerializeField]
	[ShowIf("m_UseAbilityGroups")]
	private List<BlueprintAbilityGroupReference> m_AbilityGroups;

	[SerializeField]
	[Space(4f)]
	[ShowIf("m_UseAbilityGroups")]
	private bool m_UltimateAbilities;

	public bool AbilityIsRestricted(AbilityData abilityData)
	{
		bool flag = ((!m_UseAbilityGroups) ? m_Abilities.ContainsAbility(abilityData.Blueprint) : (m_AbilityGroups.Any((BlueprintAbilityGroupReference group) => abilityData.AbilityGroups.Contains(group)) || m_UltimateAbilities));
		if (!m_UseOnlyListedAbilities)
		{
			return flag;
		}
		return !flag;
	}
}
