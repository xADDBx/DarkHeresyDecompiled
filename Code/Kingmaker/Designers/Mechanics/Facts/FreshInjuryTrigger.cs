using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[TypeId("28bf52e8da71401ab5e8ed286bac5385")]
public class FreshInjuryTrigger : UnitFactComponentDelegate, IUnitWoundHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private ActionList m_ActionOnFreshInjuryReceived;

	[SerializeField]
	private ActionList m_ActionOnFreshInjuryAvoided;

	public void HandleWoundReceived()
	{
		if (EventInvokerExtensions.BaseUnitEntity != base.Fact.Owner || !m_Restrictions.IsPassed(base.Context))
		{
			return;
		}
		using (base.Context.SetScope(base.Owner.ToITargetWrapper()))
		{
			m_ActionOnFreshInjuryReceived.Run();
		}
	}

	public void HandleWoundAvoided()
	{
		if (EventInvokerExtensions.BaseUnitEntity != base.Fact.Owner || !m_Restrictions.IsPassed(base.Context))
		{
			return;
		}
		using (base.Context.SetScope(base.Owner.ToITargetWrapper()))
		{
			m_ActionOnFreshInjuryAvoided.Run();
		}
	}
}
