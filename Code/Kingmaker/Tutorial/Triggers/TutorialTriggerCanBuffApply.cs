using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[ClassInfoBox("Triggers on enter one of the specified areas, if buff could be applied\n`t|SolutionAbility` - ability with buff\n`t|SolutionUnit` - unit who can cast ability\n`t|TargetUnit` - tank unit (if `Check Tank Stat` is on)")]
[TypeId("89de4bacd6af9bf478054dc6aeddb93c")]
public class TutorialTriggerCanBuffApply : TutorialTrigger, IAreaHandler, ISubscriber
{
	[SerializeField]
	private BlueprintAreaReference[] m_TriggerAreas;

	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	[SerializeField]
	[InfoBox("Check if stat modificator from abilityBuff is greater than current modificator on top-AC unit (Tank)\nDesigned for Barkskin buff. Could work badly with some specific stat modifiers. Contact Dev, if not sure")]
	private bool m_CheckTankStat;

	[SerializeField]
	private bool m_AllowItemsWithSpell;

	public ReferenceArrayProxy<BlueprintArea> TriggerAreas
	{
		get
		{
			BlueprintReference<BlueprintArea>[] triggerAreas = m_TriggerAreas;
			return triggerAreas;
		}
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
	}
}
