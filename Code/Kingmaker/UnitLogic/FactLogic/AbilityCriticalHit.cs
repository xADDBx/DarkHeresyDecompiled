using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("7db3b46096964e54bebd90deeee5235b")]
public class AbilityCriticalHit : BlueprintComponent
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor ModifierDescriptor;

	[SerializeField]
	private BlueprintAbilityReference m_BaseAbility;

	[SerializeField]
	private BlueprintMechanicEntityFact.Reference[] m_RestrictionIgnoringCriticalSources;

	public BlueprintAbility BaseAbility => m_BaseAbility;

	public ReferenceArrayProxy<BlueprintMechanicEntityFact> RestrictionIgnoringCriticalSources
	{
		get
		{
			BlueprintReference<BlueprintMechanicEntityFact>[] restrictionIgnoringCriticalSources = m_RestrictionIgnoringCriticalSources;
			return restrictionIgnoringCriticalSources;
		}
	}
}
