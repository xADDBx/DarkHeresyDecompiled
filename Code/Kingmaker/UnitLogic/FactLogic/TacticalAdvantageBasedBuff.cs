using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete]
[TypeId("14d3f8afb20572d448adee353c8d3f09")]
public class TacticalAdvantageBasedBuff : UnitBuffComponentDelegate
{
	[SerializeField]
	private BlueprintFeatureReference m_ReductionFeature;

	[SerializeField]
	private BlueprintBuffReference m_TacticalAdvantageBuff;

	public BlueprintFeature ReductionFeature => m_ReductionFeature?.Get();

	public BlueprintBuff TacticalAdvantageBuff => m_TacticalAdvantageBuff?.Get();

	protected override void OnActivate()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster != null)
		{
			Buff buff = maybeCaster.Buffs.GetBuff(TacticalAdvantageBuff);
			if (buff != null)
			{
				int count = (maybeCaster.Facts.Contains(ReductionFeature) ? ((buff.GetRank() + 1) * 50 / 100) : ((buff.GetRank() + 3) * 25 / 100));
				buff.RemoveRank(count);
			}
		}
	}
}
