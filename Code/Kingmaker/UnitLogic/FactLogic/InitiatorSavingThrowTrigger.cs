using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[TypeId("2bc987225a4a45af9d6232a97c43d23b")]
[AllowMultipleComponents]
public class InitiatorSavingThrowTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber
{
	public bool CheckSavingThrowType;

	[ShowIf("CheckSavingThrowType")]
	public SavingThrowType m_Type;

	public ActionList BeforeRoll;

	public ActionList OnSuccessfulSave;

	public ActionList OnFailedSave;

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
		if (CheckConditions(evt))
		{
			base.Fact.RunActionInContext(BeforeRoll);
		}
	}

	public void OnEventDidTrigger(RulePerformSavingThrow evt)
	{
		if (CheckConditions(evt))
		{
			if (evt.IsPassed)
			{
				base.Fact.RunActionInContext(OnSuccessfulSave);
			}
			else
			{
				base.Fact.RunActionInContext(OnFailedSave);
			}
		}
	}

	private bool CheckConditions(RulePerformSavingThrow evt)
	{
		if (!CheckSavingThrowType || m_Type == SavingThrowType.Unknown || m_Type == evt.Type)
		{
			return true;
		}
		return false;
	}
}
