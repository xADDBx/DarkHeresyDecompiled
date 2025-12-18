using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("6a45c42d96204b7d930bd97fa922b35d")]
public class CheckAbilityGroupGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

	public ReferenceArrayProxy<BlueprintAbilityGroup> Groups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] groups = m_Groups;
			return groups;
		}
	}

	protected override bool GetBaseValue()
	{
		AbilityData ability = this.GetAbility();
		if (ability != null)
		{
			return Groups.FirstOrDefault(ability.Blueprint.AbilityGroups.HasReference) != null;
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "AbilityGroup is " + string.Join("|", from i in Groups
			where i != null
			select i.ToString());
	}
}
