using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Settings;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/CheckGameDifficulty")]
[AllowMultipleComponents]
[TypeId("480eb195fd644d43a25eac5d2d38fc6f")]
public class CheckGameDifficulty : Condition
{
	[SerializeField]
	private GameDifficultyOption m_minDifficulty;

	public GameDifficultyOption MinDifficulty => m_minDifficulty;

	protected override string GetConditionCaption()
	{
		return $"Check current difficulty is [{MinDifficulty}]";
	}

	protected override bool CheckCondition()
	{
		return SettingsController.Instance.DifficultyPresetsController.CurrentDifficultyCompareTo(MinDifficulty) >= 0;
	}
}
