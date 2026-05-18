using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("622b2e3118c34edebe1597cf4c03339e")]
public class CheckAbilityBlueprintGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintAbilityReference[] m_Abilities;

	public ReferenceArrayProxy<BlueprintAbility> Abilities
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] abilities = m_Abilities;
			return abilities;
		}
	}

	protected override bool GetBaseValue()
	{
		AbilityData ability = EvalContext.Current.Ability;
		if (ability != null)
		{
			return Abilities.ContainsAbility(ability.Blueprint);
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability is " + string.Join("|", from i in Abilities
			where i != null
			select i.ToString());
	}
}
