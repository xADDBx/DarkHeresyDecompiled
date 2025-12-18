using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[TypeId("b81fe82d8ccd42b3b2864f38c833ccfa")]
public abstract class WarhammerDefenseTriggerBase : MechanicEntityFactComponentDelegate
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList ActionOnSelfHit;

	public ActionList ActionOnSelfMiss;

	public ActionList ActionsOnTargetHit;

	public ActionList ActionsOnTargetMiss;

	public bool TriggerOnDodge;

	public bool TriggerOnParry;

	public bool TriggerOnCover;
}
