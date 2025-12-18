using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UICombatEndWindowTexts
{
	public LocalizedString VictoryTitle;

	public LocalizedString RegularVictoryDescription;

	public LocalizedString MoraleVictoryDescription;

	public LocalizedString AdditionalCombatObjectiveVictoryDescription;

	public LocalizedString CombatEndButton;

	public LocalizedString CombatContinueButton;

	public static UICombatEndWindowTexts Instance => UIStrings.Instance.CombatEndWindow;

	public LocalizedString GetDescriptionText(CombatEndReason reason)
	{
		return reason switch
		{
			CombatEndReason.RegularVictory => RegularVictoryDescription, 
			CombatEndReason.MoraleVictory => MoraleVictoryDescription, 
			CombatEndReason.AdditionalCombatObjectiveVictory => AdditionalCombatObjectiveVictoryDescription, 
			_ => throw new ArgumentOutOfRangeException("reason", reason, null), 
		};
	}
}
