using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[TypeId("c724a08e76a9485ab867a60389466524")]
public abstract class WarhammerWeaponHitTriggerBase : UnitFactComponentDelegate
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList ActionOnSelfHit;

	public ActionList ActionOnSelfMiss;

	public ActionList ActionsOnTargetHit;

	public ActionList ActionsOnTargetMiss;

	protected void TryRunActions(MechanicEntity initiator, MechanicEntity target, bool isHit)
	{
		if (isHit)
		{
			base.Fact.RunActionInContext(ActionOnSelfHit, initiator.ToITargetWrapper());
			base.Fact.RunActionInContext(ActionsOnTargetHit, target.ToITargetWrapper());
		}
		if (!isHit)
		{
			base.Fact.RunActionInContext(ActionOnSelfMiss, initiator.ToITargetWrapper());
			base.Fact.RunActionInContext(ActionsOnTargetMiss, target.ToITargetWrapper());
		}
		base.ExecutesCount++;
	}
}
