using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("00a08a99fe7dc8c4fafa405d328c14c4")]
public class AbilityCustomStarshipTorpedoControl : BlueprintComponent
{
	[SerializeField]
	private BlueprintBuffReference m_TorpedoBuff;

	[SerializeField]
	private bool AllowFirstTurn;

	public ActionList ActionsOnTorpedo;

	public ActionList ActionsOnFinish;

	public BlueprintBuff TorpedoBuff => m_TorpedoBuff?.Get();
}
