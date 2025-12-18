using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[ComponentName("Ability/AbilityDeliveryDelay")]
[TypeId("87a59fcc03db47398e0e17cf0a2abde2")]
public class AbilityDeliveryDelay : AbilityDeliverEffect
{
	public float DelaySeconds = 1f;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		TimeSpan startTime = Game.Instance.Controllers.TimeController.GameTime;
		while (Game.Instance.Controllers.TimeController.GameTime - startTime < DelaySeconds.Seconds())
		{
			yield return null;
		}
		yield return new AbilityDeliveryTarget(target);
	}
}
