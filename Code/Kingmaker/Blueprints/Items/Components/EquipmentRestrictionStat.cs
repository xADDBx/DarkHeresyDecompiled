using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowMultipleComponents]
[TypeId("6dfdda28c94860241a112b404538e2a7")]
public class EquipmentRestrictionStat : EquipmentRestriction
{
	public StatType Stat;

	public int MinValue;

	[SerializeField]
	private bool m_ChangeRestrictionStatIfHasFact;

	[ShowIf("m_ChangeRestrictionStatIfHasFact")]
	[SerializeField]
	private BlueprintUnitFactReference m_Fact;

	[ShowIf("m_ChangeRestrictionStatIfHasFact")]
	[SerializeField]
	private int m_SubtractionValue;

	public override bool CanBeEquippedBy(MechanicEntity unit)
	{
		if (m_ChangeRestrictionStatIfHasFact && unit.Facts.Contains(m_Fact.Get()))
		{
			return (int)unit.Actor.GetStat(Stat, null, default(StatContext), "CanBeEquippedBy") >= MinValue - m_SubtractionValue;
		}
		return (int)unit.Actor.GetStat(Stat, null, default(StatContext), "CanBeEquippedBy") >= MinValue;
	}
}
