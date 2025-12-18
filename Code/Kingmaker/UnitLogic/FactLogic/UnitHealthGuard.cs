using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Heal/UnitHealthGuard")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("0dc054950e8a441f85a27a27e021d947")]
public class UnitHealthGuard : UnitFactComponentDelegate
{
	[Tooltip("0 means 1 HP")]
	public int HealthPercent;

	protected override void OnActivateOrPostLoad()
	{
		int value = Math.Max(1, (int)(0.01 * (double)HealthPercent * (double)base.Owner.Health.MaxHitPoints));
		base.Owner.Features.Immortality.Retain();
		base.Owner.Health.AddHealthGuard(base.Fact, this, value);
		base.Owner.Health.HitPoints.AddDependentFact(base.Fact);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Features.Immortality.Release();
		base.Owner.Health.RemoveHealthGuard(base.Fact, this);
		base.Owner.Health.HitPoints.RemoveDependentFact(base.Fact);
	}
}
