using System;
using JetBrains.Annotations;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Mechanics;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Features.Concentration;

[Serializable]
[TypeId("75cd5b5536034ce9ac4747222cac1be1")]
public abstract class ConcentrationBrokenTrigger : MechanicEntityFactComponentDelegate
{
	[InfoBox("CurrentEntity - кто сбил концентрацию; CurrentTarget - в кого Owner делает channel")]
	public RestrictionCalculator Restrictions;

	public ActionList ActionsOnOwner;

	public ActionList ActionsOnConcentrationTarget;

	[InfoBox("Экшены для того, кто сбил концентрацию")]
	public ActionList ActionsOnReason;

	[InfoBox("Экшены для того, кому сбили концентрацию")]
	public ActionList ActionsOnBrokenConcentrationOwner;

	protected void TryTrigger([CanBeNull] MechanicEntity reason)
	{
		TargetWrapper targetWrapper = base.Owner.GetOptional<PartConcentration>()?.Target;
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (Restrictions.IsPassed(base.Context, reason, targetWrapper))
		{
			ActionsOnConcentrationTarget.RunWithTargetIfNotNull(targetWrapper);
			ActionsOnOwner.RunWithTargetIfNotNull(base.Owner);
			ActionsOnReason.RunWithTargetIfNotNull(reason);
			ActionsOnBrokenConcentrationOwner.RunWithTargetIfNotNull(mechanicEntity);
		}
	}
}
