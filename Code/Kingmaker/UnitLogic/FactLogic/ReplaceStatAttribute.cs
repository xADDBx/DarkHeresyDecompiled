using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete("Use OverrideStat instead")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowMultipleComponents]
[TypeId("abc6ff48520749c0b4d161484dd6486f")]
public class ReplaceStatAttribute : ReplaceStat
{
	protected override void OnActivateOrPostLoad()
	{
		StatType stat = ReplaceStat.GetStat(m_NewAttribute);
		if (m_OnlyIfHigher)
		{
			stat = (((int)base.Owner.GetStatOptional(base.PreviousAttributeToCompare) > (int)base.Owner.GetStatOptional(stat)) ? base.PreviousAttributeToCompare : stat);
		}
	}

	protected override void OnDeactivate()
	{
	}
}
