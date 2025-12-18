using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("ba6da85dc6ea9ae4fa8102a767a2e6b3")]
[PlayerUpgraderAllowed(false)]
public class ModifyWoundDamagePerTurnThresholdHp : GameAction
{
	[Tooltip("Wounds threshold hp for player's party will be greater on value percent")]
	public int WoundDamagePerTurnThresholdHPFractionModifier;

	public override string GetCaption()
	{
		return $"Wounds threshold hp for player's party will be greater on {WoundDamagePerTurnThresholdHPFractionModifier} percent";
	}

	protected override void RunAction()
	{
		Game.Instance.Player.TraumasModification.WoundDamagePerTurnThresholdHPFractionModifier += WoundDamagePerTurnThresholdHPFractionModifier;
	}
}
