using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/SetCameraBoundsOverride")]
[AllowMultipleComponents]
[TypeId("f04f85d5ed8670046b2cb19f449e0f54")]
public class SetCameraBoundsOverride : GameAction
{
	[SerializeField]
	private AreaPartBounds m_Bounds;

	public override string GetDescription()
	{
		return "Overrides camera bounds with values from the specified AreaPartBounds asset";
	}

	protected override void RunAction()
	{
		if (m_Bounds != null)
		{
			CameraRig.Instance?.SetCameraBoundsOverride(m_Bounds.CameraBounds);
		}
	}

	public override string GetCaption()
	{
		return "Set Camera Bounds Override (" + (m_Bounds?.name ?? "none") + ")";
	}
}
