using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[ComponentName("Actions/SetDeviceState")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("c547380dbcf29304081aeaa567a8752e")]
public class SetDeviceState : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator Device;

	[ValidateNotNull]
	[SerializeReference]
	public IntEvaluator State;

	public override string GetCaption()
	{
		return $"Set device state ({Device}:{State})";
	}

	protected override void RunAction()
	{
		Device.GetValue().GetOptional<InteractionDevicePart>()?.SetState(State.GetValue());
	}
}
