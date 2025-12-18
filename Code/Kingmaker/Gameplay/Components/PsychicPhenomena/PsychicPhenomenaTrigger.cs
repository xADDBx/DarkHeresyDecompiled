using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.PsychicPhenomena;

[Serializable]
[TypeId("a0582718d0ab4699891b59636fa01948")]
public abstract class PsychicPhenomenaTrigger : MechanicEntityFactComponentDelegate
{
	public enum FilterType
	{
		NoFilter,
		OnlyPhenomena,
		OnlyPerils,
		PhenomenaOrPerils
	}

	public RestrictionCalculator Restrictions;

	public FilterType Filter;

	public ActionList Actions;

	protected void TryTrigger(RulePerformPsychicPhenomena evt)
	{
		if ((Filter == FilterType.OnlyPerils && !evt.ResultIsPerils && evt.ResultPhenomenaIsAppear) || (Filter == FilterType.OnlyPhenomena && evt.ResultIsPerils && evt.ResultPhenomenaIsAppear) || (Filter == FilterType.PhenomenaOrPerils && !evt.ResultPhenomenaIsAppear) || !Restrictions.IsPassed(base.Context, null, evt.Initiator, evt))
		{
			return;
		}
		using (base.Context.SetScope(evt.Initiator, evt))
		{
			Actions.Run();
		}
	}
}
