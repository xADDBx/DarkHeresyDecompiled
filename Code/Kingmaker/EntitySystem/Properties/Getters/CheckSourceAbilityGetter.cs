using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("b9888648240744caaa14e5cb18b734cd")]
public class CheckSourceAbilityGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptionalRule
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

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Source ability is " + string.Join("|", from i in Abilities
			where i != null
			select i.ToString());
	}

	protected override bool GetBaseValue()
	{
		BlueprintAbilityWrapper wrapper = SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Top?.SourceAbility?.Blueprint;
		return Abilities.ContainsAbility(wrapper);
	}
}
