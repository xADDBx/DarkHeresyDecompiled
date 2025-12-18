using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers;

[Serializable]
[Obsolete]
public class AbilityDescription
{
	public bool AbilityFromList;

	[ShowIf("AbilityFromList")]
	[SerializeField]
	[FormerlySerializedAs("AllowedAbilities")]
	private BlueprintAbilityReference[] m_AllowedAbilities;

	public ReferenceArrayProxy<BlueprintAbility> AllowedAbilities
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] allowedAbilities = m_AllowedAbilities;
			return allowedAbilities;
		}
	}

	public override string ToString()
	{
		if (!AbilityFromList)
		{
			return string.Empty;
		}
		return string.Join(", ", AllowedAbilities.Select((BlueprintAbility a) => a.ToString()));
	}
}
