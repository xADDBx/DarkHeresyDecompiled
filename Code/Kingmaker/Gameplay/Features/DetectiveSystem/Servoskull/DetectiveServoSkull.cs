using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
[ComponentName("Detective System/DetectiveServoSkull")]
[TypeId("64145720060546e189f581266ecfb38b")]
public class DetectiveServoSkull : MechanicEntityFactComponentDelegate
{
	protected override void OnFactAttached()
	{
		base.OnFactAttached();
		base.Owner.GetOrCreate<PartDetectiveServoSkull>();
	}

	protected override void OnFactDetached()
	{
		base.OnFactDetached();
		base.Owner.Remove<PartDetectiveServoSkull>();
	}
}
