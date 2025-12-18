using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete("Use BuffImmunity instead")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Immunity/SpecificBuffImmunity")]
[TypeId("a672a3cd16b6adb46824a887253d88c5")]
public class SpecificBuffImmunity : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateCanApplyBuff>, IRulebookHandler<RuleCalculateCanApplyBuff>, ISubscriber, IInitiatorRulebookSubscriber
{
	[SerializeField]
	[FormerlySerializedAs("Buff")]
	private BlueprintBuffReference m_Buff;

	[SerializeField]
	private ActionList m_ActionsOnImmunity;

	[SerializeField]
	private bool m_DisableGameLog;

	public BlueprintBuff Buff => m_Buff?.Get();

	public void OnEventAboutToTrigger(RuleCalculateCanApplyBuff evt)
	{
		if (evt.Blueprint == Buff)
		{
			evt.Immunity.Add(base.Runtime);
			evt.DisableGameLog = m_DisableGameLog;
			base.Fact.RunActionInContext(m_ActionsOnImmunity, base.Owner.ToITargetWrapper());
		}
	}

	public void OnEventDidTrigger(RuleCalculateCanApplyBuff evt)
	{
	}
}
