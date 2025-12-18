using System;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("bc7154ed0f90e564daa577ebd433f137")]
public class WarhammerContextActionSwitchVoidShields : ContextAction
{
	private enum SwitchActionType
	{
		Activate,
		Deactivate
	}

	[SerializeField]
	private SwitchActionType SwitchAction;

	public override string GetCaption()
	{
		return $"{SwitchAction} shields";
	}

	protected override void RunAction()
	{
	}
}
