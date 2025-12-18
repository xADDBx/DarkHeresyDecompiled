using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Target Restriction/AbilityTargetHasNoFactUnless")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("c86d7ab572ddfea4ca09cb8c04facb87")]
public class AbilityTargetHasNoFactUnless : BlueprintComponent, IAbilityTargetRestriction
{
	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintUnitFactReference[] m_CheckedFacts;

	[SerializeField]
	private BlueprintUnitFactReference m_UnlessFact;

	public ReferenceArrayProxy<BlueprintUnitFact> CheckedFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] checkedFacts = m_CheckedFacts;
			return checkedFacts;
		}
	}

	[CanBeNull]
	public BlueprintUnitFact UnlessFact => m_UnlessFact?.Get();

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		MechanicEntity entity = target.Entity;
		if (entity == null)
		{
			return false;
		}
		bool flag = false;
		foreach (BlueprintUnitFact checkedFact in CheckedFacts)
		{
			flag = entity.Facts.Contains(checkedFact);
			if (flag)
			{
				break;
			}
		}
		bool flag2 = UnlessFact != null && ability.Caster.Facts.Contains(UnlessFact);
		return !flag || flag2;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		string noFacts = string.Join(", ", CheckedFacts.Select((BlueprintUnitFact f) => f.Name));
		return ConfigRoot.Instance.LocalizedTexts.Reasons.TargetHasNoFactUnless.ToString(delegate
		{
			GameLogContext.Text = noFacts;
			GameLogContext.Description = UnlessFact?.Name;
		});
	}
}
