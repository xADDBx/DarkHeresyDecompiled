using System;
using Kingmaker.Blueprints;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Solvers;

[Obsolete]
[TypeId("e05dc3b6210e92e4b890c65666d19ce8")]
public class TutorialSolverSpellFromList : TutorialSolverSpellOrUsableItem
{
	[SerializeField]
	[ValidateNotEmpty]
	[ValidateNoNullEntries]
	private BlueprintAbilityReference[] m_Spells;

	public ReferenceArrayProxy<BlueprintAbility> Spells
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] spells = m_Spells;
			return spells;
		}
	}

	protected override int GetPriority(AbilityData ability)
	{
		return 0;
	}

	protected override int GetPriority(ItemEntity item)
	{
		return -1;
	}
}
