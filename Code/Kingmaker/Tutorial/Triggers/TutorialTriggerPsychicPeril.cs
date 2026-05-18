using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("2a4d1e01e0cc4665b23e6ea2bac89eaa")]
public class TutorialTriggerPsychicPeril : TutorialTrigger, IPsychicPerilHandler, ISubscriber
{
	[SerializeField]
	private bool ReactToPeril;

	[SerializeField]
	private bool ReactToPhenomena;

	public void HandlePsychicPeril(RulePerformPsychicPhenomena rule)
	{
		BaseUnitEntity initiatorUnit = rule.InitiatorUnit;
		if (initiatorUnit == null || !initiatorUnit.IsInPlayerParty)
		{
			return;
		}
		if (rule.ResultIsPerils && ReactToPeril)
		{
			TryToTrigger(rule, delegate(TutorialContext context)
			{
				context.SourceUnit = rule.InitiatorUnit;
			});
		}
		if (!rule.ResultIsPerils && ReactToPhenomena)
		{
			TryToTrigger(rule, delegate(TutorialContext context)
			{
				context.SourceUnit = rule.InitiatorUnit;
			});
		}
	}
}
