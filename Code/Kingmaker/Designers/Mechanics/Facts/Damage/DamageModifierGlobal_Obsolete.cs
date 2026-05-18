using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Obsolete]
[AllowMultipleComponents]
[TypeId("751154c0214f445d8b3062786992503d")]
public class DamageModifierGlobal_Obsolete : DamageModifier, IGlobalRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IGlobalRulebookSubscriber
{
	public bool OnlyAgainstAllies;

	public bool NotAgainstOwner;

	public bool OnlyIfTargetHasBuff;

	[ShowIf("OnlyIfTargetHasBuff")]
	[SerializeField]
	private BlueprintBuffReference[] m_Buffs;

	[ShowIf("OnlyIfTargetHasBuff")]
	public bool BuffOnlyFromCaster;

	public ReferenceArrayProxy<BlueprintBuff> Buffs
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] buffs = m_Buffs;
			return buffs;
		}
	}

	protected override StatModifierScope Scope => StatModifierScope.Owner;

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		MechanicEntity initiator = evt.Initiator;
		MechanicEntity maybeTarget = evt.MaybeTarget;
		if (maybeTarget != null && initiator != null && (!OnlyAgainstAllies || (initiator.IsAlly(maybeTarget) && maybeTarget.IsAlly(base.Owner))) && (!NotAgainstOwner || maybeTarget != base.Owner) && (!OnlyIfTargetHasBuff || maybeTarget.Buffs.Enumerable.Any((Buff p) => Buffs.Contains(p.Blueprint) && (!BuffOnlyFromCaster || p.Context.MaybeCaster == base.Owner))))
		{
			TryApply(evt);
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}
}
