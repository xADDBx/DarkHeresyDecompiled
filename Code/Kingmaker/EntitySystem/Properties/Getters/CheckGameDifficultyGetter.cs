using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Settings;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("1892ce922fb24464a4d1572235596426")]
public class CheckGameDifficultyGetter : BoolPropertyGetter
{
	[SerializeField]
	private GameDifficultyOption m_minDifficulty;

	public GameDifficultyOption MinDifficulty => m_minDifficulty;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Check current difficulty is [{MinDifficulty}]";
	}

	protected override bool GetBaseValue()
	{
		return SettingsController.Instance.DifficultyPresetsController.CurrentDifficultyCompareTo(MinDifficulty) >= 0;
	}
}
