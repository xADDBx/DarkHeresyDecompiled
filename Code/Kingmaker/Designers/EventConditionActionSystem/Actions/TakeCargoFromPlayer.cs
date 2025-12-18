using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("3daf38bcdace40f084d93677cd92dba0")]
public class TakeCargoFromPlayer : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintCargoReference m_Cargo;

	[SerializeField]
	private int m_CargoAmount;

	private BlueprintCargo Cargo => m_Cargo.Get();

	public override string GetCaption()
	{
		string arg = ((Cargo == null) ? "NULL" : (Cargo.name ?? ""));
		return $"Takes {m_CargoAmount} {arg} from Player if he has it";
	}

	protected override void RunAction()
	{
	}
}
