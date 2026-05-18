using System;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UICombatEndWindowTexts
{
	public LocalizedString VictoryTitle;

	public LocalizedString MoraleVictoryTitle;

	public LocalizedString MoraleVictoryDescription;

	public LocalizedString ExecuteEnemiesButton;

	public LocalizedString DetainEnemiesButton;

	public LocalizedString ContinueCombatButton;

	public static UICombatEndWindowTexts Instance => UIStrings.Instance.CombatEndWindow;
}
