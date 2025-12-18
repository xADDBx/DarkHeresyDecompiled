using System;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("1441c1c4053751d4ab4d4fa0c107aaef")]
public class WarhammerContextActionManageVoidShields : ContextAction
{
	private enum ActionType
	{
		Reinforce,
		RestoreWeakest
	}

	[SerializeField]
	private ActionType actionType;

	[SerializeField]
	[HideIf("IsRestoreWeakest")]
	private StarshipSectorShieldsType shieldsSector;

	[SerializeField]
	[HideIf("IsRestoreWeakest")]
	private bool IsUpgraded;

	[SerializeField]
	[ShowIf("IsRestoreWeakest")]
	private int pctOfMaxStrength;

	private bool IsRestoreWeakest => actionType == ActionType.RestoreWeakest;

	public override string GetCaption()
	{
		return actionType switch
		{
			ActionType.Reinforce => string.Format("Reinforce{0} {1} shields", IsUpgraded ? " (upgraded)" : "", shieldsSector), 
			ActionType.RestoreWeakest => $"Restore up to {pctOfMaxStrength}% of weakest shield sector", 
			_ => "Unknown shield operation", 
		};
	}

	protected override void RunAction()
	{
	}
}
