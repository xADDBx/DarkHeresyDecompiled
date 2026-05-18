using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/ResetCameraBoundsOverride")]
[AllowMultipleComponents]
[TypeId("dd41972ed7f1be944aed87a89cb33011")]
public class ResetCameraBoundsOverride : GameAction
{
	public override string GetDescription()
	{
		return "Resets camera bounds override back to the default area bounds";
	}

	protected override void RunAction()
	{
		CameraRig.Instance?.ResetCameraBoundsOverride();
	}

	public override string GetCaption()
	{
		return "Reset Camera Bounds Override";
	}
}
