using System;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[Obsolete]
[TypeId("4cede40e39bd4ca4ae1b3422486ddaaa")]
public class ContextActionDealTraumas : ContextAction
{
	[Tooltip("Count as 1 if 0")]
	public ContextValue Count;

	public override string GetCaption()
	{
		return $"Deal {(Count.IsZero ? ((ContextValue)1) : Count)} trauma(s)";
	}

	protected override void RunAction()
	{
	}
}
