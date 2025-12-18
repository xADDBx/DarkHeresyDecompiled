using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("72abaa04158a4af498ec61527311b1bc")]
public class UpdateBuffEndCondition : MechanicEntityFactComponentDelegate
{
	[SerializeField]
	private BuffEndCondition m_EndCondition = BuffEndCondition.CombatEnd;

	[SerializeField]
	private BuffExpireMoment m_ExpireMoment;

	protected override void OnApplyPostLoadFixes()
	{
		if (base.Fact is Buff buff)
		{
			if (buff.EndCondition != m_EndCondition)
			{
				buff.EndCondition = m_EndCondition;
			}
			if (buff.ExpireMoment != m_ExpireMoment)
			{
				buff.ExpireMoment = m_ExpireMoment;
			}
		}
	}
}
