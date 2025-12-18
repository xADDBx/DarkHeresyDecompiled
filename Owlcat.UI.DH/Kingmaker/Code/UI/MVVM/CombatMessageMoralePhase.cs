using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Parts;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatMessageMoralePhase : CombatMessageBase
{
	public MoralePhaseType PhaseType;

	public override string GetText()
	{
		return PhaseType switch
		{
			MoralePhaseType.Regular => UIStrings.Instance.CombatLog.MoraleEventRestoreToRegular, 
			MoralePhaseType.Heroic => UIStrings.Instance.CombatLog.MoraleEventBecomeHeroic, 
			MoralePhaseType.Broken => UIStrings.Instance.CombatLog.MoraleEventBecomeBroken, 
			_ => throw new ArgumentOutOfRangeException("PhaseType", PhaseType, null), 
		};
	}

	public override Color? GetColor()
	{
		return PhaseType switch
		{
			MoralePhaseType.Regular => ConfigRoot.Instance.UIConfig.CombatTextColors.Default, 
			MoralePhaseType.Heroic => ConfigRoot.Instance.UIConfig.CombatTextColors.HeroicPhaseColor, 
			MoralePhaseType.Broken => ConfigRoot.Instance.UIConfig.CombatTextColors.BrokenPhaseColor, 
			_ => throw new ArgumentOutOfRangeException("PhaseType", PhaseType, null), 
		};
	}

	public override bool GetAttention()
	{
		return true;
	}
}
