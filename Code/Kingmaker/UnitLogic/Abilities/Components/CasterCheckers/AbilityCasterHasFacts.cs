using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintToggleAbility))]
[AllowMultipleComponents]
[ComponentName("Caster Restriction/AbilityCasterHasFacts")]
[TypeId("3244a7f6b105c654db650034076be4a5")]
public class AbilityCasterHasFacts : BlueprintComponent, IAbilityCasterRestriction
{
	[SerializeField]
	[FormerlySerializedAs("Facts")]
	private BlueprintUnitFactReference[] m_Facts;

	[Tooltip("It will not be showed in any UI screens")]
	public bool HideInUI;

	public bool NeedsAll;

	public ReferenceArrayProxy<BlueprintUnitFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] facts = m_Facts;
			return facts;
		}
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		bool flag = NeedsAll;
		foreach (BlueprintUnitFact fact in Facts)
		{
			flag = caster.Facts.Contains(fact);
			if (flag && !NeedsAll)
			{
				return true;
			}
		}
		return flag;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}

	public IEnumerable<string> GetAbilityCasterRestrictionShortUITexts(MechanicEntity caster)
	{
		CasterRestrictionsStrings restrictionsStrings = ConfigRoot.Instance.LocalizedTexts.CasterRestrictionsStrings;
		BlueprintUnitFactReference[] facts = m_Facts;
		foreach (BlueprintUnitFactReference blueprintUnitFactReference in facts)
		{
			bool hasFact = caster.Facts.Contains((BlueprintUnitFact)blueprintUnitFactReference);
			yield return restrictionsStrings.GetHasFactRestrictionText(blueprintUnitFactReference, hasFact);
		}
	}
}
