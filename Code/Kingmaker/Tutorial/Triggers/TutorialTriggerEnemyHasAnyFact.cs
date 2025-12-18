using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[ClassInfoBox("Triggers on enter combat against enemy with any of EnemyFacts.\n't|SourceUnit' - mob with necessary fact")]
[TypeId("8d251b765553d6942a10bff0f110e3a1")]
public class TutorialTriggerEnemyHasAnyFact : TutorialTriggerEnterCombatWithUnit
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference[] m_EnemyFacts;

	public ReferenceArrayProxy<BlueprintUnitFact> EnemyFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] enemyFacts = m_EnemyFacts;
			return enemyFacts;
		}
	}

	protected override bool IsSuitableUnit(BaseUnitEntity unit)
	{
		foreach (BlueprintUnitFact enemyFact in EnemyFacts)
		{
			if (unit.Facts.Contains(enemyFact))
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnSetupContext(TutorialContext context, BaseUnitEntity unit)
	{
		context[TutorialContextKey.SourceUnit] = unit;
		context[TutorialContextKey.TargetUnit] = unit;
		context.RevealUnitInfo = unit;
	}
}
